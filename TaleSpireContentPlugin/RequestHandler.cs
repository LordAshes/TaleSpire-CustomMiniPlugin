using System;
using System.Collections.Generic;

using BepInEx;
using Dummiesman;
using UnityEngine;

namespace LordAshes
{
    public partial class CustomMiniPlugin : BaseUnityPlugin
    {
        public class Transformation
        {
            public GameObject go { get; set; } = null;
            public Transform[] parents { get; set; } = null;
            public int parent { get; set; } = 0;
        }

        public class RequestHandler
        {
            // Directory for custom content
            public Dictionary<CreatureGuid, Transformation> transformedAssets = new Dictionary<CreatureGuid, Transformation>();

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
                        if(change.key.Contains(".assetAnimation"))
                        {
                            if (change.value != "")
                            {
                                // Process the request (since remove has a blank value this will trigger mesh removal)
                                Debug.Log("Play Animation Request For '" + asset.Creature.Name + "' (" + change.cid + ") To '" + change.value + "'");
                                // Reset request
                                StatMessaging.SetInfo(change.cid, change.key, "");
                                // Process animation
                                PlayAnimation(asset, change.value);
                            }
                        }
                        else if (change.key.Contains(".effect"))
                        {
                            // Process the request (since remove has a blank value this will trigger mesh removal)
                            Debug.Log("Effect Request For '" + asset.Creature.Name + "' (" + change.cid + ") To '" + change.value + "'");
                            LoadCustomContent(asset, LoadType.effect, (FileAccessPlugin.GetProtocol(change.value)=="") ? (change.value + "/" + change.value) : change.value);
                        }
                        else
                        {
                            // Process the request (since remove has a blank value this will trigger mesh removal)
                            Debug.Log("Transfromation Request For '" + asset.Creature.Name + "' (" + change.cid + ") To '" + change.value + "'");
                            LoadCustomContent(asset, LoadType.mini, (FileAccessPlugin.GetProtocol(change.value) == "") ? (change.value + "/" + change.value) : change.value);
                        }
                    }
                    else
                    {
                        Debug.Log("Asset With Creature Id '"+change.cid+"' Cannot Be Found.");
                    }
                }
            }

            /// <summary>
            /// Play an animation on an asset with built in animations
            /// </summary>
            /// <param name="asset">Asset which is to be animated</param>
            /// <param name="animationName">Name of the animation to be played</param>
            public void PlayAnimation(CreatureBoardAsset asset, string animationName)
            {
                // Find corresponding attached Game Object
                Debug.Log("Getting Animation Object");
                GameObject animationObject = GameObject.Find("CustomContent:" + asset.Creature.CreatureId);
                if(animationObject!=null)
                {
                    // Check to see if the asset has built in animations
                    Animation anim = (animationObject.GetComponent<Animation>() != null) ? animationObject.GetComponent<Animation>() : animationObject.GetComponentInChildren<Animation>();
                    if(anim!=null)
                    {
                        // Check each built in animations for the animation name
                        foreach(AnimationState state in anim)
                        {
                            Debug.Log("Animation Object Has Clip '" + state.name+ "'. Looking for '"+animationName+"'");
                            // Compare animation clip name to request content
                            if (state.name==animationName)
                            {
                                // Play animation
                                Debug.Log("Activating Animation");
                                anim.Stop();
                                anim.Play(state.name);
                                return;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Animation Object 'CustomContent:" + asset.Creature.CreatureId + "' Does Not Have An Animation Component.");
                    }
                }
                else
                {
                    Debug.Log("Animation Object 'CustomContent:"+asset.Creature.CreatureId+"' Not Found.");
                }
                // Animation not found on asset, re-broadcast in case the General Animation plugin is installed
                StatMessaging.SetInfo(asset.Creature.CreatureId, CustomMiniPlugin.Guid + ".animation", animationName);
            }

            /// <summary>
            /// Adds a custom mesh game object to the indicated asset remove any previous attached mesh objects
            /// </summary>
            /// <param name="asset">Parent asset to whom the custom mesh will be attached</param>
            /// <param name="source">Path and name of the content file</param>
            public void LoadCustomContent(CreatureBoardAsset asset, LoadType style, string source, Action<string,CreatureGuid> missingContentCallback = null)
            {
                try
                {
                    UnityEngine.Debug.Log("Customizing Mini '" + CustomMiniPlugin.GetCreatureName(asset.Creature) + "' Using '" + source + "' Type '" + style.ToString() + "'...");

                    // Effects are prefixed by # tag
                    bool effect = (style == LoadType.effect);
                    string prefix = (effect) ? "CustomEffect:" : "CustomContent:";

                    // Look up the content name to see if the actual file has an extenion or not
                    if (System.IO.Path.GetFileNameWithoutExtension(source) != "")
                    {
                        if (FileAccessPlugin.GetProtocol(source) == "")
                        {
                            // Local File 
                            Debug.Log("Using Local File");
                            string[] seek = FileAccessPlugin.File.Find(source);
                            bool foundSource = false;
                            foreach (string item in seek)
                            {
                                if (System.IO.Path.GetExtension(item) == "")
                                {
                                    // Asset Bundle
                                    UnityEngine.Debug.Log("Corresponding AssetBundle file exists");
                                    foundSource = true;
                                    break;
                                }
                                else if (System.IO.Path.GetExtension(item).ToUpper() == ".OBJ")
                                {
                                    // OBJ File
                                    source = source + ".OBJ";
                                    UnityEngine.Debug.Log("Corresponding OBJ exists");
                                    foundSource = true;
                                    break;
                                }
                            }
                            if (!foundSource)
                            {
                                // No Compatibale Content Found
                                UnityEngine.Debug.Log("No corresponding file exists");
                                if (missingContentCallback != null)
                                { 
                                    missingContentCallback(asset.Creature.Name, asset.Creature.CreatureId);
                                }
                                else
                                {
                                    SystemMessage.DisplayInfoText("'" + source + "' is not found in");
                                    SystemMessage.DisplayInfoText(dir + "Minis/" + source);
                                }
                                return;
                            }
                        }
                        else
                        {
                            // URL
                            Debug.Log("Using URL");
                        }
                    }
                    // Remove existing effect or animation object
                    if (!effect)
                    {
                        Debug.Log("Destorying '" + CustomMiniPlugin.GetCreatureName(asset.Creature) + "' mesh...");
                        Debug.Log("Removing Item From Transform List...");
                        transformedAssets.Remove(asset.Creature.CreatureId);
                        Debug.Log("Destroying Corresponding GO...");
                        GameObject.Destroy(GameObject.Find(prefix + asset.Creature.CreatureId));
                        
                    }
                    else
                    {
                        Debug.Log("Destorying '" + CustomMiniPlugin.GetCreatureName(asset.Creature) + "' effect...");
                        GameObject.Destroy(GameObject.Find(prefix + asset.Creature.CreatureId));
                    }
                    // If source is blank then we are done now that we destoryed the effect or animation object
                    if (System.IO.Path.GetFileNameWithoutExtension(source) == "") { return; }

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
                            if (assetBundle == null) { UnityEngine.Debug.Log("AssetBundle Is Not Already Loaded. Loading."); assetBundle = FileAccessPlugin.AssetBundle.Load(source); }
                            content = null;
                            try
                            {
                                content = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>(System.IO.Path.GetFileNameWithoutExtension(source)));
                            }
                            catch (Exception x)
                            {
                                Debug.Log("Error Instantiating Asset: Asset Source = '" + source + "'");
                                Debug.Log("Error Instantiating Asset: " + x);
                            }
                            break;
                        case ".OBJ": // OBJ/MTL Source
                            UnityEngine.Debug.Log("Using OBJ/MTL Loader");
                            if (!FileAccessPlugin.File.Exists(source.Substring(0,source.Length-4) + ".mtl"))
                            {
                                if (missingContentCallback!=null) { missingContentCallback(asset.Creature.Name + " (" + source.Substring(0, source.Length - 4) + ".mtl)", asset.Creature.CreatureId); }
                            }
                            UnityExtension.ShaderDetector.Reference(source.Substring(0, source.Length - 4) + ".mtl");
                            content = null;
                            try
                            {
                                content = new OBJLoader().Load(source, source.Substring(0, source.Length - 4) + ".mtl");
                            }
                            catch (Exception x)
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

                    // Replace original mimi mesh (used for flying)
                    float baseRadiusMagicNumber = 0.570697f; // Base size for a regular character
                    float creatureScaleFactor = content.transform.localScale.x * asset.BaseRadius / baseRadiusMagicNumber;
                    Debug.Log("(CreatureScale)" + content.transform.localScale + "x(Base Size)" + asset.BaseRadius + "/(BaseUnitSize)" + baseRadiusMagicNumber + "=(CreatureScale)" + creatureScaleFactor);
                    asset.CreatureLoaders[0].transform.position = new Vector3(0, 0, 0);
                    asset.CreatureLoaders[0].transform.rotation = Quaternion.Euler(0, 0, 0);
                    asset.CreatureLoaders[0].transform.eulerAngles = new Vector3(0, 0, 0);
                    asset.CreatureLoaders[0].transform.localPosition = new Vector3(0, 0, 0);
                    asset.CreatureLoaders[0].transform.localRotation = Quaternion.Euler(0, 180, 0);
                    asset.CreatureLoaders[0].transform.localEulerAngles = new Vector3(0, 180, 0);
                    asset.CreatureLoaders[0].transform.localScale = new Vector3(content.transform.localScale.x, content.transform.localScale.y, content.transform.localScale.z);
                    foreach(AssetLoader loader in asset.CreatureLoaders)
                    {
                        Debug.Log("Removing Original Mini Mesh...");
                        loader.LoadedAsset.GetComponent<MeshFilter>().mesh.triangles = new int[0];
                    }

                    // Sync position and rotation to the base and parent it to the base
                    UnityEngine.Debug.Log("Attaching To Base...");
                    content.transform.position = asset.CreatureLoaders[0].transform.position;
                    content.transform.rotation = asset.CreatureLoaders[0].transform.rotation;
                    content.transform.localScale = new Vector3(creatureScaleFactor, creatureScaleFactor, creatureScaleFactor);
                    content.transform.SetParent(asset.CreatureLoaders[0].transform);

                    // Register transformation if it isn't an effect
                    if (!effect)
                    {
                        transformedAssets.Add(asset.Creature.CreatureId, new Transformation
                        {
                            go = GameObject.Find(prefix + asset.Creature.CreatureId),
                            parent = ((asset.IsFlying) ? 1 : 0),
                            parents = new Transform[] { asset.CreatureLoaders[0].transform, asset.FlyingIndicator.transform },
                        }); 
                    }
                }
                catch (Exception) {; }
            }

            public enum LoadType
            {
                mini = 1,
                animatedMini = 2,
                effect = 11
            }
        }
    }
}

