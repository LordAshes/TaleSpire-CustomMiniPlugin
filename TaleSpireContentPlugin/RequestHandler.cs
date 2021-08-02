using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Dummiesman;
using UnityEngine;

namespace LordAshes
{
    public partial class CustomMiniPlugin : BaseUnityPlugin
    {
        public class RequestHandler
        {
            // Playing Animation Status
            public List<CreatureBoardAsset> playingAnimation = new List<CreatureBoardAsset>();

            // Temporary Effects
            public int effectDuration = 100;
            private Dictionary<CreatureGuid, int> effectTimers = new Dictionary<CreatureGuid, int>();

            /// <summary>
            /// Pseudo Update method called from CMP main thread
            /// </summary>
            public void Update()
            {
                for(int d=0; d<effectTimers.Count; d++)
                {
                    effectTimers[effectTimers.ElementAt(d).Key] = effectTimers.ElementAt(d).Value - 1;
                    Debug.Log(effectTimers.ElementAt(d).Key + " is at " + effectTimers.ElementAt(d).Value);
                    if (effectTimers[effectTimers.ElementAt(d).Key]<=0)
                    {
                        StatMessaging.SetInfo(effectTimers.ElementAt(d).Key, CustomMiniPlugin.Guid + ".effectTemporary", "");
                        effectTimers.Remove(effectTimers.ElementAt(d).Key);
                        d = d - 1;
                    }
                }
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
                        string source = change.value.Contains(".") ? change.value.Substring(0, change.value.IndexOf(".")) : change.value;
                        string prefab = change.value.Contains(".") ? change.value.Substring(change.value.IndexOf(".") + 1) : change.value;
                        if (FileAccessPlugin.GetProtocol(source) == "") { source = (source + "/" + source); }

                        if (change.key.Contains(".assetAnimation"))
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
                        else if (change.key.Contains(".effectTemporary"))
                        {
                            // Process the request (since remove has a blank value this will trigger mesh removal)
                            Debug.Log("Effect Request For '" + asset.Creature.Name + "' (" + change.cid + ") To '" + change.value + "'");
                            LoadCustomContent(asset, LoadType.effectTemporary, source, prefab);
                        }
                        else if (change.key.Contains(".effectScaled"))
                        {
                            // Process the request (since remove has a blank value this will trigger mesh removal)
                            Debug.Log("Effect Scaled Request For '" + asset.Creature.Name + "' (" + change.cid + ") To '" + change.value + "'");
                            LoadCustomContent(asset, LoadType.effectScaled, source, prefab);
                        }
                        else if (change.key.Contains(".effect"))
                        {
                            // Process the request (since remove has a blank value this will trigger mesh removal)
                            Debug.Log("Effect Request For '" + asset.Creature.Name + "' (" + change.cid + ") To '" + change.value + "'");
                            LoadCustomContent(asset, LoadType.effect, source, prefab);
                        }
                        else
                        {
                            // Process the request (since remove has a blank value this will trigger mesh removal)
                            Debug.Log("Transfromation Request For '" + asset.Creature.Name + "' (" + change.cid + ") To '" + change.value + "'");
                            LoadCustomContent(asset, LoadType.mini, source, prefab);
                        }
                        if (change.value == "") { StatMessaging.ClearInfo(change.cid, change.key); }
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
                string source = StatMessaging.ReadInfo(asset.Creature.CreatureId, CustomMiniPlugin.Guid);
                if (source.Contains(".")) { source = source.Substring(0, source.IndexOf(".")); }
                LoadCustomContent(asset, LoadType.animation, source+"/"+source,animationName);
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
                                Debug.Log("Switching " + StatMessaging.GetCreatureName(asset.Creature) + " (" + asset.Creature.CreatureId + ") to GO renderer");
                                FindRenderer(asset.CreatureLoaders[0]).enabled = false;
                                playingAnimation.Add(asset);
                                Debug.Log("Activating Animation '"+state.name+"'");
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
            public void LoadCustomContent(CreatureBoardAsset asset, LoadType style, string source, string prefab, Action<string,CreatureGuid> missingContentCallback = null)
            {
                try
                {
                    UnityEngine.Debug.Log("Customizing Mini '" + StatMessaging.GetCreatureName(asset.Creature) + "' Using '" + source + " ("+prefab+")' Type '" + style.ToString() + "'...");

                    string prefix = (style == LoadType.mini || style == LoadType.animation) ? "CustomContent:" : style.ToString().Substring(0,1).ToUpper()+style.ToString().Substring(1)+":";

                    if (asset == null) { Debug.Log("No mini selection provided."); return; }

                    if (GameObject.Find(prefix + asset.Creature.CreatureId) != null)
                    { 
                        // Destroy previous GO if there was one
                        GameObject.Destroy(GameObject.Find(prefix + asset.Creature.CreatureId));
                    }
                    else
                    {
                        Debug.Log("GO object '" + prefix + asset.Creature.CreatureId + "' not found");
                    }
                    // Remove current mesh(s) in case original mini has multiple CreatureLoaders if transformation is a mini transformation
                    if (style == LoadType.mini)
                    {
                        bool pass = false;
                        if (asset.CreatureLoaders != null)
                        {
                            pass = (asset.CreatureLoaders.Length>0);
                            Debug.Log("CreatureLoaders Has "+asset.CreatureLoaders.Length+" Entries");
                            foreach (AssetLoader loader in asset.CreatureLoaders)
                            {
                                MeshFilter mf = loader.LoadedAsset.GetComponent<MeshFilter>();
                                if (mf != null)
                                {
                                    Mesh mesh = mf.mesh;
                                    if (mesh != null)
                                    {
                                        mesh.triangles = new int[0];
                                    }
                                    else
                                    {
                                        Debug.Log("Mesh Does Not Exist");
                                        pass = false; break;
                                    }
                                }
                                else
                                {
                                    Debug.Log("MeshFilter Does Not Exist");
                                    pass = false; break;
                                }
                            }
                        }
                        if(!pass)
                        {
                            Debug.Log("Creature Did Not Pass CreatureLoaders Test");
                            SystemMessage.DisplayInfoText("Selected Mini Is Not Compatible\r\nWith The CMP Plugin.");
                            return;
                        }
                    }
                    else if (style == LoadType.animation)
                    {
                        // Turn of mini renderer
                        FindRenderer(asset.CreatureLoaders[0]).enabled = false;
                    }

                    // Exit if a remove request was issued
                    if (source == "/") { return; }

                    // Find the indicated source file(s)
                    string[] fullPathSources = FileAccessPlugin.File.Find(source);
                    bool foundUsableSource = false;
                    GameObject content = null;
                    foreach (string fullPathSource in fullPathSources)
                    {
                        Debug.Log("Found '" + fullPathSource + "' (" + System.IO.Path.GetExtension(fullPathSource).ToUpper() + ")");
                        switch(System.IO.Path.GetExtension(fullPathSource).ToUpper())
                        {
                            case ".OBJ":// OBJ/MTL Source
                                Debug.Log("OBJ/MTL Content Load From " + fullPathSource);
                                string mtlFile = fullPathSource.Substring(0, fullPathSource.Length - 4) + ".mtl";
                                Debug.Log("Looking for '" + mtlFile + "'");
                                if (!FileAccessPlugin.File.Exists(mtlFile))
                                {
                                    if (missingContentCallback != null) { missingContentCallback(asset.Creature.Name + " (" +mtlFile+ ")", asset.Creature.CreatureId); }
                                }
                                UnityExtension.ShaderDetector.Reference(mtlFile);
                                content = null;
                                try { content = new OBJLoader().Load(fullPathSource, mtlFile); } catch (Exception) { ; }
                                foundUsableSource = true;
                                break;
                            case "": // AssetBundle Source
                                string assetBundleName = System.IO.Path.GetFileNameWithoutExtension(source);
                                Debug.Log("Prefab '"+prefab+"' from AssetBundle '"+ assetBundleName + "' loaded from '"+ fullPathSource+"'");
                                AssetBundle assetBundle = FileAccessPlugin.AssetBundle.Load(source);
                                try
                                {
                                    content = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>(prefab));
                                    assetBundle.Unload(false);
                                    foundUsableSource = true;
                                }
                                catch (Exception)
                                {
                                    Debug.Log("Unable To Use AssetBundle. Looking For Alterate Source.");
                                    if (assetBundle != null) { assetBundle.Unload(false); }
                                    assetBundle = null;
                                }
                                break;
                            default:
                                break;
                        }
                        if (foundUsableSource) { break; }
                    }
                    if((!foundUsableSource) || (content==null))
                    {
                        if (missingContentCallback != null)
                        {
                            missingContentCallback(asset.Creature.Name, asset.Creature.CreatureId);
                        }
                        SystemMessage.DisplayInfoText("Unable To Load AssetBundle Or OBJ/MTL Files From\r\n" + source);
                        SystemMessage.DisplayInfoText("Ensure content files exist in folder\r\n"+dir+"Minis/"+source);
                        return;
                    }

                    // Rename template GO in case it is used for animations
                    content.name = prefix + asset.Creature.CreatureId;
                    if (style != LoadType.mini)
                    {
                        Debug.Log("Parenting GO to CreatureLoaders[0]");
                        content.transform.position = asset.CreatureLoaders[0].transform.position;
                        content.transform.rotation = asset.CreatureLoaders[0].transform.rotation;
                        content.transform.SetParent(asset.CreatureLoaders[0].transform);
                    }

                    if ((style == LoadType.mini) || (style == LoadType.animation) || (style == LoadType.effectScaled))
                    {
                        // Resizing custom asset
                        float baseRadiusMagicNumber = 0.570697f; // Base size for a regular character
                        float creatureScaleFactor = content.transform.localScale.x * asset.BaseRadius / baseRadiusMagicNumber;
                        Debug.Log("(CreatureScale)" + content.transform.localScale + "x(Base Size)" + asset.BaseRadius + "/(BaseUnitSize)" + baseRadiusMagicNumber + "=(CreatureScale)" + creatureScaleFactor);
                        if (style == LoadType.mini)
                        {
                            asset.CreatureLoaders[0].transform.localScale = new Vector3(content.transform.localScale.x, content.transform.localScale.y, content.transform.localScale.z);
                        }
                        else if (style == LoadType.effectScaled)
                        {
                            content.transform.localScale = new Vector3(content.transform.localScale.x * creatureScaleFactor, content.transform.localScale.y * creatureScaleFactor, content.transform.localScale.z * creatureScaleFactor);
                        }
                        else if(style == LoadType.animation)
                        {
                            content.transform.localScale = new Vector3(creatureScaleFactor, creatureScaleFactor, creatureScaleFactor);
                        }
                    }

                    // Replace original mini mesh and material
                    if (style == LoadType.mini)
                    {
                        asset.CreatureLoaders[0].transform.localPosition = new Vector3(0, 0, 0);
                        Debug.Log("Replacing originl mini mesh");
                        if(!ReplaceGameObjectMesh(content, asset.CreatureLoaders[0].LoadedAsset))
                        {
                            Debug.Log("Non-Compatible Mini Used. Aborting...");
                            SystemMessage.DisplayInfoText("Selected Mini Is Not Compatible\r\nWith The CMP Plugin.");
                            GameObject.Destroy(content);
                            return;
                        }
                    }
                    if (style == LoadType.mini)
                    {
                        Debug.Log("Removing template GO for non-animated minis");
                        GameObject.Destroy(content);
                    }

                    // Add temporary effects to duration timers
                    if (style == LoadType.effectTemporary)
                    {
                        Debug.Log("Turning on duration timer");
                        effectTimers.Add(asset.Creature.CreatureId, effectDuration);
                    }
                }
                catch (Exception x)
                {
                    Debug.Log("CMP encountered a exception in LoadCustomContent(): " + x);
                }
            }

            /// <summary>
            /// Method to replace the destination MeshFilter and MeshRenderer with that of the source.
            /// Since component cannot be actually switched, all properties are copied over.
            /// </summary>
            /// <param name="source"></param>
            /// <param name="destination"></param>
            public bool ReplaceGameObjectMesh(GameObject source, GameObject destination)
            {
                if (destination == null) { Debug.Log("Destination Is Null"); return false; }
                MeshFilter dMF = destination.GetComponent<MeshFilter>();
                MeshRenderer dMR = destination.GetComponent<MeshRenderer>();
                if (dMF == null || dMR == null) { Debug.LogWarning("Unable get destination MF or MR."); return false; }

                destination.transform.position = new Vector3(0, 0, 0);
                destination.transform.rotation = Quaternion.Euler(0, 0, 0);
                destination.transform.eulerAngles = new Vector3(0, 0, 0);
                destination.transform.localPosition = new Vector3(0, 0, 0);
                destination.transform.localRotation = Quaternion.Euler(0, 0, 0);
                destination.transform.localEulerAngles = new Vector3(0, 180, 0);
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

                MeshFilter sMF = (source.GetComponent<MeshFilter>() != null) ? source.GetComponent<MeshFilter>() : source.GetComponentInChildren<MeshFilter>();
                if (sMF != null)
                {
                    Debug.Log("Copying MF->MF");
                    Debug.Log("Mesh From " + sMF.mesh.name + " / "+sMF.sharedMesh.name+" (" + (sMF.mesh.triangles.Length / 3).ToString() + " Polygons / "+(sMF.mesh.triangles.Length / 3).ToString()+" Polygons)");
                    dMF.mesh = sMF.mesh;
                    dMF.sharedMesh = sMF.sharedMesh;
                }

                MeshRenderer sMR = (source.GetComponent<MeshRenderer>() != null) ? source.GetComponent<MeshRenderer>() : source.GetComponentInChildren<MeshRenderer>();
                if (sMR != null)
                {
                    Debug.Log("Copying MR->MR");
                    Debug.Log("Material From " + sMR.material.name + " / " + sMR.sharedMaterial.name );
                    Shader shaderSave = dMR.material.shader;  // Shader must be maintained in order for the Stealth mode to work automatically
                    dMR.sharedMaterials = sMR.sharedMaterials;
                    dMR.material.shader = shaderSave;
                }

                SkinnedMeshRenderer sSMR = (source.GetComponent<SkinnedMeshRenderer>() != null) ? source.GetComponent<SkinnedMeshRenderer>() : source.GetComponentInChildren<SkinnedMeshRenderer>();
                if (sSMR != null)
                {
                    Debug.Log("Copying SMR->MF/MR");
                    Debug.Log("Mesh From "+ sSMR.sharedMesh.name+" ("+(sSMR.sharedMesh.triangles.Length/3).ToString()+" Polygons)");
                    dMF.sharedMesh = sSMR.sharedMesh;
                    Shader shaderSave = dMR.material.shader; // Shader must be maintained in order for the Stealth mode to work automatically
                    Debug.Log("Material From " + sSMR.material.name);
                    dMR.material = sSMR.material;
                    dMR.material.shader = shaderSave;
                }

                return true;
            }

            public Renderer FindRenderer(AssetLoader asset)
            {
                Renderer renderer = null;
                renderer = asset.GetComponent<MeshRenderer>();
                if (renderer != null) { return renderer; }
                renderer = asset.GetComponent<SkinnedMeshRenderer>();
                if (renderer != null) { return renderer; }
                renderer = asset.GetComponentInChildren<MeshRenderer>();
                if (renderer != null) { return renderer; }
                renderer = asset.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null) { return renderer; }
                return null;
            }

            public Renderer FindRenderer(GameObject go)
            {
                Renderer renderer = null;
                renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null) { return renderer; }
                renderer = go.GetComponent<SkinnedMeshRenderer>();
                if (renderer != null) { return renderer; }
                renderer = go.GetComponentInChildren<MeshRenderer>();
                if (renderer != null) { return renderer; }
                renderer = go.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer != null) { return renderer; }
                return null;
            }

            public enum LoadType
            {
                mini = 1,
                effect = 11,
                effectScaled = 12,
                effectTemporary = 13,
                animation = 101
            }
        }
    }
}

