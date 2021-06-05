using Dummiesman;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace LordAshes
{
    public class StatHandler
    {
        // Plugin guid
        private string guid;

        // Directory for custom content
        private string dir = "";

        // Determines if transformation is in progress
        private bool transformationInProgress = false;

        // Config
        private CustomAssetIndex customAssetIndex = new CustomAssetIndex();

        // Transformations
        private Dictionary<CreatureGuid, string> transformations = new Dictionary<CreatureGuid, string>();

        /// <summary>
        /// Constructor taking in the content directory and identifiers
        /// </summary>
        /// <param name="requestIdentifiers"></param>
        /// <param name="path"></param>
        public StatHandler(string guid, string path)
        {
            this.guid = guid;
            this.dir = path;
            transformations.Clear();
        }

        /// <summary>
        /// Method to detect character Stat3 requests 
        /// </summary>
        public void CheckStatRequests()
        {
            if (!transformationInProgress)
            {
                foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets)
                {
                    // Get transform namne if any
                    string transformName = (asset.Creature.Name.Contains(":")) ? asset.Creature.Name.Substring(asset.Creature.Name.LastIndexOf(":") + 1).Trim() : "";

                    // If creature doesn't have a current transformName add an empty one to the transform dictionary
                    if (!transformations.ContainsKey(asset.Creature.CreatureId)) { transformations.Add(asset.Creature.CreatureId, ""); }

                    // If the transform has changed
                    if (transformations[asset.Creature.CreatureId] != transformName)
                    {
                        Debug.Log("Creature '" + asset.Creature.Name + "' (" + asset.Creature.CreatureId + ") has changed from '" + transformations[asset.Creature.CreatureId] + "' to '" + transformName + "'");

                        // Prevent transformation for being applied multiple times
                        transformations[asset.Creature.CreatureId] = transformName;

                        // Process request
                        LoadCustomContent(asset, dir + "Minis/" + transformName + "/" + transformName);

                        // Reduce stress on system by processing one transformation per cycle
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Method to make character Stat3 requests
        /// </summary>
        /// <param name="asset">Asset making the request</param>
        /// <param name="content">Name of the contents</param>
        public void SetTransformationRequest(CreatureGuid cid, string content)
        {
            CreatureBoardAsset asset;
            CreaturePresenter.TryGetAsset(cid, out asset);
            string origName = asset.Creature.Name;
            if (origName.IndexOf(":") > -1) { origName = origName.Substring(0, origName.LastIndexOf(":")).Trim(); }
            Debug.Log("Setting creature '" + asset.Creature.Name + "' name (" + asset.Creature.CreatureId + ") to '" + origName + " : " + content + "'");
            CreatureManager.SetCreatureName(cid, origName + " : " + content);
        }

        /// <summary>
        /// Used to reset the transformation list so that all transformation will be re-applied
        /// </summary>
        public void Reset()
        {
            transformations.Clear();
        }

        /// <summary>
        /// Method to sync the transformation mesh with the character's stealth mode setting
        /// </summary>
        public void SyncStealthMode()
        {
            foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets.ToArray())
            {
                if (transformations.ContainsKey(asset.Creature.CreatureId))
                {
                    GameObject child = GameObject.Find("CustomContent:" + asset.Creature.CreatureId);
                    if (child != null)
                    {
                        if (asset.Creature.IsExplicitlyHidden && child.transform.localScale.x != 0f)
                        {
                            UnityEngine.Debug.Log("Hiding Custom Mesh...");
                            child.transform.localScale = new Vector3(0f, 0f, 0f);
                        }
                        else if (!asset.Creature.IsExplicitlyHidden && child.transform.localScale.x != 1f)
                        {
                            UnityEngine.Debug.Log("Unhiding Custom Mesh...");
                            child.transform.localScale = new Vector3(1f, 1f, 1f);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a custom mesh game object to the indicated asset remove any previous attached mesh objects
        /// </summary>
        /// <param name="asset">Parent asset to whom the custom mesh will be attached</param>
        /// <param name="source">Path and name of the content file</param>
        private void LoadCustomContent(CreatureBoardAsset asset, string source)
        {
            transformationInProgress = true;

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
                    if(System.IO.File.Exists(source))
                    {
                        // Asset Bundle
                    }
                    else if (System.IO.File.Exists(source+".OBJ"))
                    {
                        // OBJ File
                        source = source + ".OBJ";
                    }
                }
                else
                {
                    // Source is blank meaning this is a remove request
                    source = "";
                }

                if (source=="" || System.IO.File.Exists(source))
                {
                    Debug.Log("Destroying Game Object '" + prefix + asset.Creature.CreatureId + "'");
                    GameObject.Destroy(GameObject.Find(prefix + asset.Creature.CreatureId));

                    if (source != "")
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
                                content = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>(System.IO.Path.GetFileNameWithoutExtension(source)));
                                break;
                            case ".OBJ": // OBJ/MTL Source
                                UnityEngine.Debug.Log("Using OBJ/MTL Loader");
                                UnityExtension.ShaderDetector.Reference(System.IO.Path.GetDirectoryName(source) + "/" + System.IO.Path.GetFileNameWithoutExtension(source) + ".mtl");
                                content = new OBJLoader().Load(source);
                                break;
                            default: // Unrecognized Source
                                Debug.Log("Content Type '" + System.IO.Path.GetExtension(source).ToUpper() + "' is not supported. Use OBJ/MTL or FBX.");
                                break;
                        }

                        content.name = prefix + asset.Creature.CreatureId;
                        content.transform.position = asset.gameObject.transform.position;
                        content.transform.rotation = asset.BaseLoader.gameObject.transform.rotation;
                        content.transform.SetParent(asset.BaseLoader.gameObject.transform);

                        if (!effect)
                        {
                            UnityEngine.Debug.Log("Removing Core Mini Meshes");
                            asset.CreatureLoader.LoadedAsset.GetComponent<MeshFilter>().mesh.triangles = new int[0];
                        }
                    }
                }
                else
                {
                    SystemMessage.DisplayInfoText("I don't know about\r\n" + System.IO.Path.GetFileNameWithoutExtension(source));
                }
            }
            catch(Exception){ ;}

            transformationInProgress = false;
        }

        public class CustomAssetIndex
        {
            public Dictionary<UInt32, CustomAsset> assets { get; private set; }  = new Dictionary<uint, CustomAsset>();

            public void Load(string configPath = "")
            {
                this.assets = JsonConvert.DeserializeObject<Dictionary<UInt32, CustomAsset>>(System.IO.File.ReadAllText(configPath));
            }

            public void Save(string configPath = "")
            {
                System.IO.File.WriteAllText(configPath,JsonConvert.SerializeObject(this.assets, Formatting.Indented));
            }
        }

        public class CustomAsset
        {
            public string content { get; set; } 
            public string source { get; set; }
        }
    }
}
