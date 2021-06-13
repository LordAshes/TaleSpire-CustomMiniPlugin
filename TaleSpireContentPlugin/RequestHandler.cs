using Dummiesman;
using System;
using System.Linq;
using UnityEngine;
using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LordAshes
{

    public partial class CustomMiniPlugin : BaseUnityPlugin
    {
        public class RequestHandler
        {
            // Directory for custom content
            private string dir = "";

            /// <summary>
            /// Constructor taking in the content directory and identifiers
            /// </summary>
            /// <param name="requestIdentifiers"></param>
            /// <param name="path"></param>
            public RequestHandler(string guid, string path)
            {
                this.dir = path;
            }

            /// <summary>
            /// Callback method that is informed by StatMessaging when the Stat Block has changed.
            /// This is triggered on any changes in the Stat Block and thus it is the responsibilty of the
            /// callback function to determine which Stat Block changes are applicable to the plugin
            /// (typically by checking the change.key for an expected key)
            /// </summary>
            /// <param name="changes">Array of changes detected by the Stat Messaging plugin</param>
            public void Request(StatMessaging.Change[] changes)
            {
                Debug.Log("Stat Messaging Request(s)");
                // Process all changes
                foreach (StatMessaging.Change change in changes)
                {
                    Debug.Log("Stat Messaging Request: Cid: "+change.cid+", Key: "+change.key+", Type: "+change.action+", Value: "+change.value+", Previous: "+change.previous);
                    // Find a reference to the indicated mini
                    CreatureBoardAsset asset = null;
                    CreaturePresenter.TryGetAsset(change.cid, out asset);
                    if (asset != null)
                    {
                        // Process the request (since remove has a blank value this will trigger mesh removal)
                        Debug.Log("Transfromation Request For '" + asset.Creature.Name + "' (" + change.cid + ") To '" + change.value + "'");
                        LoadCustomContent(asset, dir+"Minis/"+change.value+"/"+change.value);
                    }
                    else
                    {
                        Debug.Log("Asset With Creature Id '"+change.cid+"' Cannot Be Found.");
                    }
                }
            }

            /// <summary>
            /// Adds a custom mesh game object to the indicated asset remove any previous attached mesh objects
            /// </summary>
            /// <param name="asset">Parent asset to whom the custom mesh will be attached</param>
            /// <param name="source">Path and name of the content file</param>
            public void LoadCustomContent(CreatureBoardAsset asset, string source, Action<string,CreatureGuid> missingContentCallback = null)
            {
                try
                {
                    UnityEngine.Debug.Log("Customizing Mini '" + asset.Creature.Name + "' Using '" + source + "'...");

                    // Effects are prefixed by # tag
                    bool effect = (source.IndexOf("#") > -1);
                    source = source.Replace("#", "");
                    string prefix = (effect) ? "CustomEffect:" : "CustomContent:";
                    Debug.Log("Effect = " + effect);

                    // Look up the content name to see if the actual file has an extenion or not
                    if (System.IO.Path.GetFileNameWithoutExtension(source) != "")
                    {
                        // Obtain file name of the content
                        if (System.IO.File.Exists(source))
                        {
                            // Asset Bundle
                        }
                        else if (System.IO.File.Exists(source + ".OBJ"))
                        {
                            // OBJ File
                            source = source + ".OBJ";
                        }
                        else
                        {
                            // No Compatibale Content Found
                            missingContentCallback(asset.Creature.Name, asset.Creature.CreatureId);
                            return;
                        }
                    }
                    else
                    {
                        // Source is blank meaning this is a remove request
                        if (!effect)
                        {
                            Debug.Log("Destorying '" + asset.Creature.Name + "' mesh...");
                            asset.CreatureLoader.LoadedAsset.GetComponent<MeshFilter>().mesh.triangles = new int[0];
                        }
                        else
                        {
                            Debug.Log("Destorying '" + asset.Creature.Name + "' effect...");
                            GameObject.Destroy(GameObject.Find(prefix+asset.Creature.CreatureId));
                        }
                        return;
                    }

                    if (System.IO.File.Exists(source))
                    {
                        GameObject content = null;
                        // Determine which type of content it is 
                        switch (System.IO.Path.GetExtension(source).ToUpper())
                        {
                            case "": // AssetBundle Source
                                UnityEngine.Debug.Log("Using AssetBundle Loader");
                                string assetBundleName = System.IO.Path.GetFileNameWithoutExtension(source);
                                AssetBundle assetBundle = null;
                                foreach (AssetBundle ab in AssetBundle.GetAllLoadedAssetBundles())
                                {
                                    // Debug.Log("Checking Existing AssetBundles: Found '" + ab.name + "'. Seeking '"+assetBundleName+"'");
                                    if (ab.name == assetBundleName) { UnityEngine.Debug.Log("AssetBundle Is Already Loaded. Reusing."); assetBundle = ab; break; }
                                }
                                if (assetBundle == null) { UnityEngine.Debug.Log("AssetBundle Is Not Already Loaded. Loading."); assetBundle = AssetBundle.LoadFromFile(source); }
                                content = null;
                                try
                                {
                                    content = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>(System.IO.Path.GetFileNameWithoutExtension(source)));
                                }
                                catch(Exception x)
                                {
                                    Debug.Log("Error Instantiating Asset: Asset Source = '" + source+"'");
                                    Debug.Log("Error Instantiating Asset: " + x);
                                }
                                break;
                            case ".OBJ": // OBJ/MTL Source
                                UnityEngine.Debug.Log("Using OBJ/MTL Loader");
                                if (!System.IO.File.Exists(System.IO.Path.GetDirectoryName(source) + "/" + System.IO.Path.GetFileNameWithoutExtension(source) + ".mtl"))
                                {
                                    missingContentCallback(asset.Creature.Name + " (" + System.IO.Path.GetDirectoryName(source) + "/" + System.IO.Path.GetFileNameWithoutExtension(source) + ".mtl)", asset.Creature.CreatureId);
                                }
                                UnityExtension.ShaderDetector.Reference(System.IO.Path.GetDirectoryName(source) + "/" + System.IO.Path.GetFileNameWithoutExtension(source) + ".mtl");
                                content = null;
                                try
                                {
                                    content = new OBJLoader().Load(source);
                                }
                                catch(Exception x)
                                {
                                    Debug.Log("Error Instantiating OBJ/MTL: OBJ Source = '" + source + "'");
                                    Debug.Log("Error Instantiating OBJ/MTL: " + x);
                                }
                                break;
                            default: // Unrecognized Source
                                Debug.Log("Content Type '" + System.IO.Path.GetExtension(source).ToUpper() + "' is not supported. Use OBJ/MTL or FBX.");
                                break;
                        }
                        if (content == null) { return; }
                        content.name = prefix + asset.Creature.CreatureId;

                        if (!effect)
                        {
                            try
                            {
                                asset.CreatureLoader.transform.position = new Vector3(0, 0, 0);
                                asset.CreatureLoader.transform.rotation = Quaternion.Euler(0, 0, 0);
                                asset.CreatureLoader.transform.eulerAngles = new Vector3(0, 0, 0);
                                asset.CreatureLoader.transform.localPosition = new Vector3(0, 0, 0);
                                asset.CreatureLoader.transform.localRotation = Quaternion.Euler(0, 180, 0);
                                asset.CreatureLoader.transform.localEulerAngles = new Vector3(0, 180, 0);
                                asset.CreatureLoader.transform.localScale = new Vector3(1f, 1f, 1f);
                                ReplaceGameObjectMesh(content, asset.CreatureLoader.LoadedAsset);
                            }
                            catch (Exception) {; }
                            UnityEngine.Debug.Log("Destroying Template...");
                            GameObject.Destroy(GameObject.Find(prefix + asset.Creature.CreatureId));
                        }
                        else
                        {
                            UnityEngine.Debug.Log("Attaching To Base...");
                            content.transform.position = asset.BaseLoader.transform.position;
                            content.transform.rotation = asset.BaseLoader.transform.rotation;
                            content.transform.SetParent(asset.BaseLoader.transform);
                        }
                    }
                    else
                    {
                        SystemMessage.DisplayInfoText("I don't know about\r\n" + System.IO.Path.GetFileNameWithoutExtension(source));
                    }
                }
                catch (Exception) {; }
            }

            /// <summary>
            /// Method to replace the destination MeshFilter and MeshRenderer with that of the source.
            /// Since component cannot be actually switched, all properties are copied over.
            /// </summary>
            /// <param name="source"></param>
            /// <param name="destination"></param>
            public void ReplaceGameObjectMesh(GameObject source, GameObject destination)
            {
                MeshFilter dMF = destination.GetComponent<MeshFilter>();
                MeshRenderer dMR = destination.GetComponent<MeshRenderer>();
                if(dMF==null || dMR==null) { Debug.LogWarning("Unable get destination MF or MR."); return; }

                destination.transform.position = new Vector3(0, 0, 0);
                destination.transform.rotation = Quaternion.Euler(0, 0, 0);
                destination.transform.eulerAngles = new Vector3(0, 0, 0);
                destination.transform.localPosition = new Vector3(0, 0, 0);
                destination.transform.localRotation = Quaternion.Euler(0, 0, 0);
                destination.transform.localEulerAngles = new Vector3(0, 0, 0);
                destination.transform.localScale = new Vector3(1f, 1f, 1f);

                dMF.transform.position = new Vector3(0, 0, 0);
                dMF.transform.rotation = Quaternion.Euler(0, 0, 0);
                dMF.transform.eulerAngles = new Vector3(0, 0, 0);
                dMF.transform.localPosition = new Vector3(0, 0, 0);
                dMF.transform.localRotation = Quaternion.Euler(0, 0, 0);
                dMF.transform.localEulerAngles = new Vector3(0, 0, 0);
                dMF.transform.localScale = new Vector3(1, 1, 1);

                dMR.transform.position = new Vector3(0, 0, 0);
                dMR.transform.rotation = Quaternion.Euler(0, 0, 0);
                dMR.transform.eulerAngles = new Vector3(0, 0, 0);
                dMR.transform.localPosition = new Vector3(0, 0, 0);
                dMR.transform.localRotation = Quaternion.Euler(0, 0, 0);
                dMR.transform.localEulerAngles = new Vector3(0, 0, 0);
                dMR.transform.localScale = new Vector3(1, 1, 1);

                MeshFilter sMF = (source.GetComponent<MeshFilter>()!=null) ? source.GetComponent<MeshFilter>() : source.GetComponentInChildren<MeshFilter>(); 
                if(sMF != null)
                {
                    Debug.Log("Copying MF->MF");
                    dMF.mesh = sMF.mesh;
                    dMF.sharedMesh = sMF.sharedMesh;
                }

                MeshRenderer sMR = (source.GetComponent<MeshRenderer>() != null) ? source.GetComponent<MeshRenderer>() : source.GetComponentInChildren<MeshRenderer>();
                if (sMR != null)
                {
                    Debug.Log("Copying MR->MR");
                    Shader shaderSave = dMR.material.shader;  // Shader must be maintained in order for the Stealth mode to work automatically
                    dMR.sharedMaterials = sMR.sharedMaterials;
                    dMR.material.shader = shaderSave;
                }

                SkinnedMeshRenderer sSMR = (source.GetComponent<SkinnedMeshRenderer>() != null) ? source.GetComponent<SkinnedMeshRenderer>() : source.GetComponentInChildren<SkinnedMeshRenderer>();
                if (sSMR != null)
                {
                    Debug.Log("Copying SMR->MF/MR");
                    dMF.sharedMesh = sSMR.sharedMesh;
                    Shader shaderSave = dMR.material.shader; // Shader must be maintained in order for the Stealth mode to work automatically
                    dMR.material = sSMR.material;
                    dMR.material.shader = shaderSave;
                }
            }
        }
    }
}

