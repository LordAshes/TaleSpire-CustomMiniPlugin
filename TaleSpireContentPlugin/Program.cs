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
    [BepInPlugin(Guid, Name, Version)]
    public class CustomMiniPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Custom Mini Plug-In";
        public const string Guid = "org.lordashes.plugins.custommini";
        public const string Version = "3.0.0.0";

        // Content directory
        private static string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        // Triggers
        private ConfigEntry<KeyboardShortcut>[] triggers { get; set; } = new ConfigEntry<KeyboardShortcut>[6];
        private ConfigEntry<string>[] animNames { get; set; } = new ConfigEntry<string>[5];

        // Chat handelr
        StatHandler statHandler = new StatHandler(Guid, dir);

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
            triggers[1] = Config.Bind("Hotkeys", "Play Animation/Pose 1", new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl));
            triggers[2] = Config.Bind("Hotkeys", "Play Animation/Pose 2", new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl));
            triggers[3] = Config.Bind("Hotkeys", "Play Animation/Pose 3", new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl));
            triggers[4] = Config.Bind("Hotkeys", "Play Animation/Pose 4", new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl));
            triggers[5] = Config.Bind("Hotkeys", "Play Animation/Pose 5", new KeyboardShortcut(KeyCode.Alpha8, KeyCode.LeftControl));

            // Setup default animation names
            animNames[0] = Config.Bind("Animation", "Animation/Pose 1 Name", "Idle");
            animNames[1] = Config.Bind("Animation", "Animation/Pose 2 Name", "Ready");
            animNames[2] = Config.Bind("Animation", "Animation/Pose 3 Name", "Attack");
            animNames[3] = Config.Bind("Animation", "Animation/Pose 4 Name", "Dance");
            animNames[4] = Config.Bind("Animation", "Animation/Pose 5 Name", "Die");
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if(StateDetection.Ready(ref statHandler))
            {
                statHandler.CheckStatRequests();
                statHandler.SyncStealthMode();

                // Check for Transformation 
                if (StrictKeyCheck(triggers[0].Value))
                {
                    SystemMessage.AskForTextInput("Custom Mini Plugin", "Make me a: ", "OK", (s) => { statHandler.SetTransformationRequest(LocalClient.SelectedCreatureId, s); }, null, "Cancel", null, "");
                }

                for(int a=1; a<=animNames.Length; a++)
                {
                    if(StrictKeyCheck(triggers[a].Value))
                    {
                        Debug.Log("Request Animation/Pose " + a + "...");
                        CreatureBoardAsset asset;
                        CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out asset);
                        if(asset!=null)
                        {
                            GameObject go = GameObject.Find("CustomContent:" + asset.Creature.CreatureId);
                            if (go != null)
                            {
                                Animation anim = go.GetComponent<Animation>();
                                if (anim != null)
                                {
                                    if (anim.isPlaying)
                                    {
                                        Debug.Log("Stopping Animations On Mini");
                                        anim.Stop();
                                    }
                                    else
                                    {
                                        Debug.Log("Activating Animation '" + animNames[a - 1].Value + "' On Mini");
                                        try
                                        {
                                            anim.Play(animNames[a - 1].Value);
                                        }
                                        catch (System.Exception)
                                        {
                                            Debug.Log("Creature '" + asset.Creature.Name + "' does not have animation '" + animNames[a - 1].Value + "'");
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.Log("Creature '" + asset.Creature.Name + "' does not have animations");
                                }
                            }
                            else
                            {
                                Debug.Log("Creature '" + asset.Creature.Name + "' does not have animations");
                            }
                        }
                    }
                }
            }
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
