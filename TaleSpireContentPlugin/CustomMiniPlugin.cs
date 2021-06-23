using System.Linq;
using System.Collections.Generic;

using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System;

namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(StatMessaging.Guid)]
    [BepInDependency(RadialUI.RadialUIPlugin.Guid)]
    public partial class CustomMiniPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Custom Mini Plug-In";
        public const string Guid = "org.lordashes.plugins.custommini";
        public const string Version = "4.5.0.0";

        // Content directory
        public static string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        // Triggers
        private ConfigEntry<KeyboardShortcut>[] actionTriggers { get; set; } = new ConfigEntry<KeyboardShortcut>[3];
        private ConfigEntry<KeyboardShortcut>[] animTriggers { get; set; } = new ConfigEntry<KeyboardShortcut>[5];

        // Request handelr
        private static RequestHandler requestHandler = new RequestHandler();

        // Show Effect Dialog
        private CreatureGuid showEffectDialog = CreatureGuid.Empty;

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
            actionTriggers[0] = Config.Bind("Hotkeys", "Transform Mini", new KeyboardShortcut(KeyCode.M, KeyCode.LeftControl));
            actionTriggers[1] = Config.Bind("Hotkeys", "Add Effect", new KeyboardShortcut(KeyCode.E, KeyCode.LeftControl));
            actionTriggers[2] = Config.Bind("Hotkeys", "Play Animation", new KeyboardShortcut(KeyCode.P, KeyCode.LeftControl));

            animTriggers[0] = Config.Bind("Hotkeys", "Animation 1", new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl));
            animTriggers[1] = Config.Bind("Hotkeys", "Animation 2", new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl));
            animTriggers[2] = Config.Bind("Hotkeys", "Animation 3", new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl));
            animTriggers[3] = Config.Bind("Hotkeys", "Animation 4", new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl));
            animTriggers[4] = Config.Bind("Hotkeys", "Animation 5", new KeyboardShortcut(KeyCode.Alpha8, KeyCode.LeftControl));

            // Add transformation main menu
            RadialUI.RadialSubmenu.EnsureMainMenuItem(  RadialUI.RadialUIPlugin.Guid + ".Transformation",
                                                        RadialUI.RadialSubmenu.MenuType.character,
                                                        "Transformation",
                                                        RadialUI.RadialSubmenu.GetIconFromFile(dir + "Images/Icons/Transformation.png")
                                                      );
            // Add effect sub-menu
            RadialUI.RadialSubmenu.CreateSubMenuItem(   RadialUI.RadialUIPlugin.Guid + ".Transformation",
                                                        "Effect",
                                                        RadialUI.RadialSubmenu.GetIconFromFile(dir + "Images/Icons/Effect.png"),
                                                        ActivateEffect
                                                    );

            // Activate State Detection Board Subscription 
            StateDetection.Initiailze(this.GetType());
        }

        private void ActivateEffect(CreatureGuid cid, string menu, MapMenuItem mmi)
        {
            showEffectDialog = cid;
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if (StateDetection.Ready())
            {

                // Implement stealth mode, height bar mode and fly mode
                foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets)
                {
                    GameObject go = GameObject.Find("CustomContent:" + asset.Creature.CreatureId);
                    if (go != null)
                    {
                        if (go.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                        { 
                            go.GetComponentInChildren<SkinnedMeshRenderer>().enabled = !asset.Creature.IsExplicitlyHidden & !asset.IsFlying & (asset.transform.position.y < CameraController.HidePlaneHeight); 
                        }
                        else if (go.GetComponentInChildren<MeshRenderer>() != null) 
                        { 
                            go.GetComponentInChildren<MeshRenderer>().enabled = !asset.Creature.IsExplicitlyHidden & !asset.IsFlying & (asset.transform.position.y < CameraController.HidePlaneHeight);
                        }
                        asset.CreatureLoaders[0].LoadedAsset.GetComponent<MeshRenderer>().enabled = asset.IsFlying;
                    }
                }

                // Check for Transformation 
                if (StrictKeyCheck(actionTriggers[0].Value))
                {
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Make me a: ", "OK",
                                                    (s) => { StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid, s); }, null,
                                                    "Remove", () => { StatMessaging.ClearInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid); },
                                                    "");
                }

                // Check for Effects
                if (StrictKeyCheck(actionTriggers[1].Value) || showEffectDialog!=CreatureGuid.Empty)
                {
                    CreatureGuid active = (showEffectDialog != CreatureGuid.Empty) ? showEffectDialog : LocalClient.SelectedCreatureId;
                    showEffectDialog = CreatureGuid.Empty;
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Add effect: ", "OK",
                                                    (s) => { StatMessaging.SetInfo(active, CustomMiniPlugin.Guid+".effect", s); }, null,
                                                    "Remove", () => { StatMessaging.SetInfo(active, CustomMiniPlugin.Guid+".effect", ""); },
                                                    "");
                }

                // Check for Animations
                if (StrictKeyCheck(actionTriggers[2].Value))
                {
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Play animation: ", "OK",
                                                    (s) => { StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid+".assetAnimation", s); }, null,
                                                    "Remove", () => { StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid+".animation", ""); },
                                                    "");
                }

                // Check for Quick Animations
                for (int a=0; a<animTriggers.Length; a++)
                {
                    if(StrictKeyCheck(animTriggers[a].Value))
                    {
                        Debug.Log("Animation " + (a + 1) + " Key Pressed");
                        StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid + ".assetAnimation", "Anim"+(a+1));
                    }
                }

                // Sync stealth mode & height bar
                for(int t=0; t<requestHandler.transformedAssets.Count; t++)
                {
                    try
                    {
                        // Show additional GO when not flying
                        requestHandler.transformedAssets.ElementAt(t).Value.enabled = (requestHandler.transformedAssets.ElementAt(t).Key.IsVisible && !requestHandler.transformedAssets.ElementAt(t).Key.IsFlying);
                        // Show original mini when flying
                        requestHandler.transformedAssets.ElementAt(t).Key.CreatureLoaders[0].LoadedAsset.GetComponent<MeshRenderer>().enabled = (requestHandler.transformedAssets.ElementAt(t).Key.IsVisible && requestHandler.transformedAssets.ElementAt(t).Key.IsFlying);
                    }
                    catch(System.Exception)
                    {
                        // Creature deleted - remove from transformation list
                        requestHandler.transformedAssets.Remove(requestHandler.transformedAssets.ElementAt(t).Key);
                    }
                }
            }
        }

        public static RequestHandler GetRequestHander()
        {
            return requestHandler;
        }

        /// <summary>
        /// Methiod for extracting the creature name when using Stat Maessaging
        /// </summary>
        /// <param name="creature"></param>
        /// <returns></returns>
        public static string GetCreatureName(Creature creature)
        {
            string result = creature.Name;
            if(result.IndexOf("<size=0>")>=0)
            {
                result = result.Substring(0, result.IndexOf("<size=0>"));
            }
            return result;
        }

        /// <summary>
        /// Method to properly evaluate shortcut keys. 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool StrictKeyCheck(KeyboardShortcut check)
        {
            if(!check.IsUp()) { return false; }
            foreach (KeyCode modifier in new KeyCode[]{KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftShift, KeyCode.RightShift })
            {
                if (Input.GetKey(modifier) != check.Modifiers.Contains(modifier)) { return false; }
            }
            return true;
        }
    }
}
