using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using BepInEx;
using Bounce.Unmanaged;
using System.Linq;

using Dummiesman;
using TMPro;
using System.Reflection;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace LordAshes
{
    [BepInPlugin(Guid, "Custom Mini Plug-In", Version)]
    public class CustomMiniPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Guid = "org.lordashes.plugins.custommini";
        public const string Version = "2.0.0.0";

        // Content directory
        private static string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        // Triggers
        private ConfigEntry<KeyboardShortcut>[] triggers { get; set; } = new ConfigEntry<KeyboardShortcut>[1];

        // Chat handelr
        StatHandler statHandler = new StatHandler(Guid, dir);

        // Initialization stage
        int stageStart = -50;
        int stage = 0;
        int assetCount = 0;

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.Log("Custom Mini Plugin Active. Using Custom Minis In '" + dir + "'");

            if (!System.IO.Directory.Exists(dir))
            {
                // Warn user about not having the TaleSpire_CustomData folder
                UnityEngine.Debug.LogWarning("Custom Mini Plugin: Your not going to get too far with this plugin if you don't have the '" + dir + "' folder");
                UnityEngine.Debug.LogWarning("Custom Mini Plugin: I will create it but since it will be empty, you won't be able to use this plugin until you put some content there.");
                UnityEngine.Debug.LogWarning("Custom Mini Plugin: The good news is that you can drop content there even when TaleSpire is running.");
                System.IO.Directory.CreateDirectory(dir);
            }
            if (!System.IO.Directory.Exists(dir + "Minis/"))
            {
                // Warn user about not having the TaleSpire_CustomData/Minis folder
                UnityEngine.Debug.LogWarning("Custom Mini Plugin: Your not going to get too far with this plugin if you don't have the '" + dir + "Minis/' folder");
                UnityEngine.Debug.LogWarning("Custom Mini Plugin: I will create it but since it will be empty, you won't be able to use this plugin until you put some content there.");
                UnityEngine.Debug.LogWarning("Custom Mini Plugin: The good news is that you can drop content there even when TaleSpire is running.");
                System.IO.Directory.CreateDirectory(dir + "/Minis/");
            }

            // Setup default trigger
            triggers[0] = Config.Bind("Hotkeys", "Transform Mini", new KeyboardShortcut(KeyCode.M, KeyCode.LeftControl));

            // Setup initial stage
            stage = stageStart;
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if (IsBoardLoaded())
            {
                //
                // Stage 10: Ready
                //
                if (stage == 10) 
                {
                    statHandler.CheckStatRequests();
                    statHandler.SyncStealthMode();
                }
                //
                // Stage 2+: Waiting To Process SystemMessage
                //
                else if (stage>=2)
                {
                    stage++;
                }
                //
                // Stage 1: Check Minis For MeshFilter
                //
                else if (stage == 1)
                {
                    Debug.Log("Check to see if minis' mesh can be manipulated");
                    bool loaded = true;
                    foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets)
                    {
                        if (asset.CreatureLoader.LoadedAsset == null)
                        {
                            // Debug.Log("Asset " + asset.name + " has a null CreatureLoader");
                            loaded = false; break;
                        }
                        else
                        {
                            if (asset.CreatureLoader.LoadedAsset.GetComponent<MeshFilter>() == null)
                            {
                                // Debug.Log("Asset " + asset.name + " has a null CreatureLoader MeshFilter");
                                loaded = false; break;
                            }
                            else
                            {
                                if (asset.CreatureLoader.LoadedAsset.GetComponent<MeshFilter>().mesh == null)
                                {
                                    // Debug.Log("Asset " + asset.name + " has a null CreatureLoader MeshFilter mesh");
                                    loaded = false; break;
                                }
                                else
                                {
                                    // Debug.Log("Asset " + asset.name + " is ready");
                                }
                            }
                        }
                    }
                    if (loaded)
                    {
                        Debug.Log("Minis mesh test passed. Processing transformations...");
                        statHandler.Reset();
                        SystemMessage.DisplayInfoText("Please Be Patient...\r\nLoading Mini Transformations");
                        stage = 2;
                    }
                }
                //
                // Stage <=0: Mini Loaded Detection
                //
                else if (stage <= 0)
                {
                    if (CreaturePresenter.AllCreatureAssets.Count == assetCount)
                    { 
                        // Debug.Log("Creature Count No Change (" + assetCount + "): " + stage); 
                        stage++; 
                    } 
                    else 
                    { 
                        assetCount = CreaturePresenter.AllCreatureAssets.Count;
                        Debug.Log("Creature Count Change (" + assetCount + ") @ Stage " + stage);
                        stage = stageStart; 
                    }
                }

                // Check for Transformation 
                if (triggers[0].Value.IsUp())
                {
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Make me a: ", "OK", (s) => { statHandler.SetTransformationRequest(LocalClient.SelectedCreatureId, s); }, null, "Cancel", null, "");
                }
            }
            //
            // Board is re-loading, request startup seqeunce
            //
            else if (stage >= 0)
            {
                Debug.Log("Board Is Re-loading...");
                stage = stageStart;
            }
        }

        private bool IsBoardLoaded()
        {
            return (CameraController.HasInstance && BoardSessionManager.HasInstance && BoardSessionManager.HasBoardAndIsInNominalState && !BoardSessionManager.IsLoading);
        }
    }
}
