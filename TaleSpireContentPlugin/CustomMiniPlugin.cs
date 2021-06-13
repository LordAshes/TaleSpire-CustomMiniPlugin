using BepInEx;
using BepInEx.Configuration;

using UnityEngine;

using System.Linq;
using System.Collections.Generic;

namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(StatMessaging.Guid)]
    public partial class CustomMiniPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Custom Mini Plug-In";
        public const string Guid = "org.lordashes.plugins.custommini";
        public const string Version = "4.2.0.0";

        // Content directory
        public static string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        // Triggers
        private ConfigEntry<KeyboardShortcut>[] actionTriggers { get; set; } = new ConfigEntry<KeyboardShortcut>[2];
        private ConfigEntry<KeyboardShortcut>[] animTriggers { get; set; } = new ConfigEntry<KeyboardShortcut>[5];

        // Chat handelr
        private static RequestHandler requestHandler = new RequestHandler(Guid, dir);

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

            animTriggers[0] = Config.Bind("Hotkeys", "Animatio 1", new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl));
            animTriggers[1] = Config.Bind("Hotkeys", "Animatio 2", new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl));
            animTriggers[2] = Config.Bind("Hotkeys", "Animatio 3", new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl));
            animTriggers[3] = Config.Bind("Hotkeys", "Animatio 4", new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl));
            animTriggers[4] = Config.Bind("Hotkeys", "Animatio 5", new KeyboardShortcut(KeyCode.Alpha8, KeyCode.LeftControl));

            // Activate State Detection Board Subscription 
            StateDetection.Initiailze();
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
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Make me a: ", "OK",
                                                    (s) => { StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid, s); }, null,
                                                    "Remove", () => { StatMessaging.ClearInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid); },
                                                    "");
                }

                

                if (StrictKeyCheck(actionTriggers[1].Value))
                {
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Add effect: ", "OK",
                                                    (s) => { StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid, "#"+s); }, null,
                                                    "Remove", () => { StatMessaging.SetInfo(LocalClient.SelectedCreatureId, CustomMiniPlugin.Guid, "#"); },
                                                    "");
                }

                for(int a=0; a<animTriggers.Length; a++)
                {
                    if(StrictKeyCheck(animTriggers[a].Value))
                    {
                        Debug.Log("Animation " + (a + 1) + " Key Pressed");

                        CreatureBoardAsset asset;
                        CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out asset);
                        if (asset != null)
                        {
                            AudioSource sound = asset.GetComponentInChildren<AudioSource>();
                            Animation anim = asset.GetComponentInChildren<Animation>();
                            if (anim.isPlaying)
                            {
                                Debug.Log("Stopping Current Animation");
                                anim.Stop();
                            }
                            Debug.Log("Starting Animation " + (a + 1));
                            anim.Play("Anim" + (a + 1));
                            if (sound != null)
                            {
                                Debug.Log("Starting Sound");
                                sound.Play(0);
                            }
                        }
                    }
                }
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
