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
        public const string Version = "5.0.0.0";

        // Content directory
        public static string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        // Triggers
        private ConfigEntry<KeyboardShortcut>[] actionTriggers { get; set; } = new ConfigEntry<KeyboardShortcut>[3];
        private ConfigEntry<KeyboardShortcut>[] animTriggers { get; set; } = new ConfigEntry<KeyboardShortcut>[5];

        // Request handelr
        private static RequestHandler requestHandler = new RequestHandler();

        // Show Effect Dialog
        private CreatureGuid showEffectDialog = CreatureGuid.Empty;

        // Dialog Open
        private bool showContentAssist = false;

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            // Setup cache settings
            if(Config.Bind("Settings", "Use Cache For First List", true).Value)
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
            actionTriggers[2] = Config.Bind("Hotkeys", "Play Animation", new KeyboardShortcut(KeyCode.P, KeyCode.LeftControl));

            animTriggers[0] = Config.Bind("Hotkeys", "Animation 1", new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl));
            animTriggers[1] = Config.Bind("Hotkeys", "Animation 2", new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl));
            animTriggers[2] = Config.Bind("Hotkeys", "Animation 3", new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl));
            animTriggers[3] = Config.Bind("Hotkeys", "Animation 4", new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl));
            animTriggers[4] = Config.Bind("Hotkeys", "Animation 5", new KeyboardShortcut(KeyCode.Alpha8, KeyCode.LeftControl));

            // Update creature detection system for number of cycles during which creatures are allowed to load
            StateDetection.stageStart = Config.Bind("Settings", "Creature Load Cycles", -200).Value;
            StateDetection.stage = (StateDetection.stageStart-10);

            // Add transformation main menu
            RadialUI.RadialSubmenu.EnsureMainMenuItem(  RadialUI.RadialUIPlugin.Guid + ".Transformation",
                                                        RadialUI.RadialSubmenu.MenuType.character,
                                                        "Transformation",
                                                        FileAccessPlugin.Image.LoadSprite("Images/Icons/Transformation.png")
                                                      );
            // Add effect sub-menu
            RadialUI.RadialSubmenu.CreateSubMenuItem(RadialUI.RadialUIPlugin.Guid + ".Transformation",
                                                        "Effect",
                                                        FileAccessPlugin.Image.LoadSprite("Images/Icons/Effect.png"),
                                                        ActivateEffect,
                                                        true,
                                                        null
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
                // Check for Transformation 
                if (StrictKeyCheck(actionTriggers[0].Value))
                {
                    showContentAssist = true;
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Make me a: ", "OK",
                                                    (s) => 
                                                    { 
                                                        showContentAssist = false; 
                                                        StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid, s);
                                                    }, null,
                                                    "Remove", () => 
                                                    { 
                                                        showContentAssist = false; 
                                                        StatMessaging.ClearInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid); 
                                                    },
                                                    "");
                }

                // Check for Effects
                if (StrictKeyCheck(actionTriggers[1].Value) || showEffectDialog!=CreatureGuid.Empty)
                {
                    showContentAssist = true;
                    CreatureGuid active = (showEffectDialog != CreatureGuid.Empty) ? showEffectDialog : LocalClient.SelectedCreatureId;
                    showEffectDialog = CreatureGuid.Empty;
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

                // Check for ended animations
                for(int c=0; c<requestHandler.playingAnimation.Count; c++)
                {
                    GameObject go = GameObject.Find("CustomContent:" + requestHandler.playingAnimation.ElementAt(c).Creature.CreatureId);

                    if (go.GetComponent<Animation>().isPlaying==false)
                    {
                        Debug.Log("Switching " + StatMessaging.GetCreatureName(requestHandler.playingAnimation.ElementAt(c)) + " (" + requestHandler.playingAnimation.ElementAt(c).Creature.CreatureId + ") to mini mesh renderer");
                        requestHandler.FindRenderer(requestHandler.playingAnimation.ElementAt(c).CreatureLoaders[0]).enabled = true;
                        requestHandler.FindRenderer(go).enabled = false;
                        requestHandler.playingAnimation.RemoveAt(c);
                        c = c - 1;
                    }
                }
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
            foreach (KeyCode modifier in new KeyCode[]{KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftShift, KeyCode.RightShift })
            {
                if (Input.GetKey(modifier) != check.Modifiers.Contains(modifier)) { return false; }
            }
            return true;
        }
    }
}
