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
    [BepInDependency(FileAccessPlugin.Guid)]
    public partial class CustomMiniPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Custom Mini Plug-In";
        public const string Guid = "org.lordashes.plugins.custommini";
        public const string Version = "5.4.0.0";

        // Content directory
        public static string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        // Triggers
        private ConfigEntry<KeyboardShortcut>[] actionTriggers { get; set; } = new ConfigEntry<KeyboardShortcut>[6];
        private ConfigEntry<KeyboardShortcut>[] animTriggers { get; set; } = new ConfigEntry<KeyboardShortcut>[5];
        private ConfigEntry<KeyboardShortcut>[] poseTriggers { get; set; } = new ConfigEntry<KeyboardShortcut>[5];

        // Request handelr
        private static RequestHandler requestHandler = new RequestHandler();

        // Show Effect Dialog
        private Tuple<CreatureGuid,int> showDialog = new Tuple<CreatureGuid,int>(CreatureGuid.Empty,-1);

        // Use mini transformation icon
        private bool useMiniTransformationMenuItem = true;

        // Dialog Open
        private bool showContentAssist = false;

        // Diagnostic Mode
        public static bool diagnosticMode = false;

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            // Setup cache settings
            if(Config.Bind("Settings", "Use Cache For File List", true).Value)
            {
                UnityEngine.Debug.Log("Custom Mini Plugin Active. Using Cached Asset List. New Assets Cannot Be Added At Runtime.");
                FileAccessPlugin.File.SetCacheType(FileAccessPlugin.CacheType.CacheCustomData);
            }
            else
            {
                UnityEngine.Debug.Log("Custom Mini Plugin Active. Not Using Cached Asset List. New Assets Can Be Added At Runtime.");
                FileAccessPlugin.File.SetCacheType(FileAccessPlugin.CacheType.NoCacheCustomData);
            }

            // Setup default trigger
            actionTriggers[0] = Config.Bind("Hotkeys", "Transform Mini", new KeyboardShortcut(KeyCode.M, KeyCode.LeftControl));
            actionTriggers[1] = Config.Bind("Hotkeys", "Add Effect", new KeyboardShortcut(KeyCode.E, KeyCode.LeftControl));
            actionTriggers[2] = Config.Bind("Hotkeys", "Add Scaled Effect", new KeyboardShortcut(KeyCode.S, KeyCode.LeftControl));
            actionTriggers[3] = Config.Bind("Hotkeys", "Add Temporary Scaled Effect", new KeyboardShortcut(KeyCode.W, KeyCode.LeftControl));
            actionTriggers[4] = Config.Bind("Hotkeys", "Play Animation", new KeyboardShortcut(KeyCode.P, KeyCode.LeftControl));
            actionTriggers[5] = Config.Bind("Hotkeys", "Load Transformations", new KeyboardShortcut(KeyCode.T, KeyCode.LeftControl));

            animTriggers[0] = Config.Bind("Hotkeys", "Animation 1", new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl));
            animTriggers[1] = Config.Bind("Hotkeys", "Animation 2", new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl));
            animTriggers[2] = Config.Bind("Hotkeys", "Animation 3", new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl));
            animTriggers[3] = Config.Bind("Hotkeys", "Animation 4", new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl));
            animTriggers[4] = Config.Bind("Hotkeys", "Animation 5", new KeyboardShortcut(KeyCode.Alpha8, KeyCode.LeftControl));

            poseTriggers[0] = Config.Bind("Hotkeys", "Pose 1", new KeyboardShortcut(KeyCode.Alpha4, KeyCode.RightControl));
            poseTriggers[1] = Config.Bind("Hotkeys", "Pose 2", new KeyboardShortcut(KeyCode.Alpha5, KeyCode.RightControl));
            poseTriggers[2] = Config.Bind("Hotkeys", "Pose 3", new KeyboardShortcut(KeyCode.Alpha6, KeyCode.RightControl));
            poseTriggers[3] = Config.Bind("Hotkeys", "Pose 4", new KeyboardShortcut(KeyCode.Alpha7, KeyCode.RightControl));
            poseTriggers[4] = Config.Bind("Hotkeys", "Pose 5", new KeyboardShortcut(KeyCode.Alpha8, KeyCode.RightControl));

            // Update use of mini transformation radial menu item
            useMiniTransformationMenuItem = Config.Bind("Settings", "Use mini transformation radial menu item", true).Value;

            // Update creature detection system for number of cycles during which creatures are allowed to load
            StateDetection.stageStart = Config.Bind("Settings", "Creature Load Cycles", -200).Value;
            StateDetection.stage = (StateDetection.stageStart-10);

            // Update diagnostic mode from configuration
            diagnosticMode = Config.Bind("Settings", "Diagnostic Mode", false).Value;

            // Add transformation main menu
            RadialUI.RadialSubmenu.EnsureMainMenuItem(  RadialUI.RadialUIPlugin.Guid + ".Transformation",
                                                        RadialUI.RadialSubmenu.MenuType.character,
                                                        "Transformation",
                                                        FileAccessPlugin.Image.LoadSprite("Images/Icons/Transformation.png")
                                                      );
            // Add effect sub-menus
            RadialUI.RadialSubmenu.CreateSubMenuItem(RadialUI.RadialUIPlugin.Guid + ".Transformation",
                                                        "Effect",
                                                        FileAccessPlugin.Image.LoadSprite("Images/Icons/Effect.png"),
                                                        ActivateEffect,
                                                        true,
                                                        null
                                                    );

            RadialUI.RadialSubmenu.CreateSubMenuItem(RadialUI.RadialUIPlugin.Guid + ".Transformation",
                                                        "Scaled Effect",
                                                        FileAccessPlugin.Image.LoadSprite("Images/Icons/EffectScaled.png"),
                                                        ActivateEffectScaled,
                                                        true,
                                                        null
                                                    );

            RadialUI.RadialSubmenu.CreateSubMenuItem(RadialUI.RadialUIPlugin.Guid + ".Transformation",
                                                        "Temporary Effect",
                                                        FileAccessPlugin.Image.LoadSprite("Images/Icons/EffectTemporary.png"),
                                                        ActivateEffectTemporary,
                                                        true,
                                                        null
                                                    );

            // Add mini sub-menu
            if (useMiniTransformationMenuItem)
            {
                RadialUI.RadialSubmenu.CreateSubMenuItem(RadialUI.RadialUIPlugin.Guid + ".Transformation",
                                                            "Mini",
                                                            FileAccessPlugin.Image.LoadSprite("Images/Icons/Mini.png"),
                                                            ActivateMini,
                                                            true,
                                                            null
                                                        );
            };

            // Activate State Detection Board Subscription 
            StateDetection.Initiailze(this.GetType());
        }

        private void ActivateEffect(CreatureGuid cid, string menu, MapMenuItem mmi)
        {
            showDialog = new Tuple<CreatureGuid,int>(cid, 1);
        }

        private void ActivateEffectScaled(CreatureGuid cid, string menu, MapMenuItem mmi)
        {
            showDialog = new Tuple<CreatureGuid, int>(cid, 2);
        }

        private void ActivateEffectTemporary(CreatureGuid cid, string menu, MapMenuItem mmi)
        {
            showDialog = new Tuple<CreatureGuid, int>(cid, 3);
        }

        private void ActivateMini(CreatureGuid cid, string menu, MapMenuItem mmi)
        {
            showDialog = new Tuple<CreatureGuid, int>(cid, 0);
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if (StateDetection.Ready())
            {
                // Check for Transformation 
                if (StrictKeyCheck(actionTriggers[0].Value) || showDialog.Item2==0)
                {
                    showContentAssist = true;
                    CreatureGuid active = (showDialog.Item1 != CreatureGuid.Empty) ? showDialog.Item1 : LocalClient.SelectedCreatureId;
                    showDialog = new Tuple<CreatureGuid, int>(CreatureGuid.Empty, -1);
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Make me a: ", "OK",
                                                    (s) => 
                                                    { 
                                                        showContentAssist = false; 
                                                        StatMessaging.SetInfo(active, CustomMiniPlugin.Guid, s);
                                                    }, null,
                                                    "Remove", () => 
                                                    { 
                                                        showContentAssist = false; 
                                                        StatMessaging.ClearInfo(active, CustomMiniPlugin.Guid); 
                                                    },
                                                    "");
                }

                // Check for Effects
                if (StrictKeyCheck(actionTriggers[1].Value) || showDialog.Item2==1)
                {
                    showContentAssist = true;
                    CreatureGuid active = (showDialog.Item1 != CreatureGuid.Empty) ? showDialog.Item1 : LocalClient.SelectedCreatureId;
                    showDialog = new Tuple<CreatureGuid, int>(CreatureGuid.Empty, -1);
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Add effect: ", "OK",
                                                    (s) => 
                                                    {
                                                        showContentAssist = false;
                                                        StatMessaging.SetInfo(active, CustomMiniPlugin.Guid+".effect", s); 
                                                    }, null,
                                                    "Remove", () =>
                                                    {
                                                        showContentAssist = false;
                                                        StatMessaging.SetInfo(active, CustomMiniPlugin.Guid+".effect", "");
                                                    },
                                                    "");
                }

                // Check for Effects Scaled
                if (StrictKeyCheck(actionTriggers[2].Value) || showDialog.Item2 == 2)
                {
                    showContentAssist = true;
                    CreatureGuid active = (showDialog.Item1 != CreatureGuid.Empty) ? showDialog.Item1 : LocalClient.SelectedCreatureId;
                    showDialog = new Tuple<CreatureGuid, int>(CreatureGuid.Empty, -1);
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Add effect: ", "OK",
                                                    (s) =>
                                                    {
                                                        showContentAssist = false;
                                                        StatMessaging.SetInfo(active, CustomMiniPlugin.Guid + ".effectScaled", s);
                                                    }, null,
                                                    "Remove", () =>
                                                    {
                                                        showContentAssist = false;
                                                        StatMessaging.SetInfo(active, CustomMiniPlugin.Guid + ".effectScaled", "");
                                                    },
                                                    "");
                }

                // Check for Effects Temporary
                if (StrictKeyCheck(actionTriggers[3].Value) || showDialog.Item2 == 3)
                {
                    showContentAssist = true;
                    CreatureGuid active = (showDialog.Item1 != CreatureGuid.Empty) ? showDialog.Item1 : LocalClient.SelectedCreatureId;
                    showDialog = new Tuple<CreatureGuid, int>(CreatureGuid.Empty, -1);
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Add effect: ", "OK",
                                                    (s) =>
                                                    {
                                                        showContentAssist = false;
                                                        StatMessaging.SetInfo(active, CustomMiniPlugin.Guid + ".effectTemporary", s);
                                                    }, null,
                                                    "Remove", () =>
                                                    {
                                                        showContentAssist = false;
                                                        StatMessaging.SetInfo(active, CustomMiniPlugin.Guid + ".effectTemporary", "");
                                                    },
                                                    "");
                }

                // Check for Animations
                if (StrictKeyCheck(actionTriggers[4].Value))
                {
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Play animation: ", "OK",
                                                    (s) => { StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid+".assetAnimation", s); }, null,
                                                    "Remove", () => { StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid+".animation", ""); },
                                                    "");
                }

                // Manual Load Transformations
                if (StrictKeyCheck(actionTriggers[5].Value))
                {
                    StateDetection.stage = 1;
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

                // Check for Quick Poses
                for (int a = 0; a < poseTriggers.Length; a++)
                {
                    if (StrictKeyCheck(poseTriggers[a].Value))
                    {
                        Debug.Log("Pose " + (a + 1) + " Key Pressed");
                        string source = StatMessaging.ReadInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid);
                        if (source.Contains(".")) { source = source.Substring(0, source.IndexOf(".")); }
                        if (a == 0)
                        {
                            StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid, source);
                        }
                        else
                        {
                            StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid, source + ".Pose" + a);
                        }
                    }
                }

                // Check for ended animations
                for (int c=0; c<requestHandler.playingAnimation.Count; c++)
                {
                    GameObject go = GameObject.Find("CustomContent:" + requestHandler.playingAnimation.ElementAt(c).Creature.CreatureId);

                    if (go.GetComponent<Animation>().isPlaying==false)
                    {
                        Debug.Log("Switching " + StatMessaging.GetCreatureName(requestHandler.playingAnimation.ElementAt(c)) + " (" + requestHandler.playingAnimation.ElementAt(c).Creature.CreatureId + ") to mini mesh renderer");
                        requestHandler.FindRenderer(requestHandler.playingAnimation.ElementAt(c).CreatureLoaders[0]).enabled = true;
                        GameObject.Destroy(go);
                        requestHandler.playingAnimation.RemoveAt(c);
                        c = c - 1;
                    }
                }

                // Handle duration effects
                requestHandler.Update();
            }
        }

        /// <summary>
        /// Method to display the common folder for content (while the transformation/effect dialog is open)
        /// </summary>
        public void OnGUI()
        {
            if (showContentAssist)
            {
                GUIStyle gs = new GUIStyle() { wordWrap = true };
                gs.normal.textColor = UnityEngine.Color.yellow;
                gs.alignment = TextAnchor.UpperCenter;
                GUI.Label(new Rect(15, 35, 1900, 80), "Notice: Ensure That Indicated Content Exists In Folder: "+dir+"Minis/ContentName", gs);
            }
        }

        public static RequestHandler GetRequestHander()
        {
            return requestHandler;
        }

        /// <summary>
        /// Method to properly evaluate shortcut keys. 
        /// </summary>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool StrictKeyCheck(KeyboardShortcut check)
        {
            if(!check.IsUp()) { return false; }
            foreach (KeyCode modifier in new KeyCode[]{KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.RightControl, KeyCode.RightShift })
            {
                if (Input.GetKey(modifier) != check.Modifiers.Contains(modifier)) { return false; }
            }
            return true;
        }
    }
}
