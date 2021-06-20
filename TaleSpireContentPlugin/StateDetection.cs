using BepInEx;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LordAshes
{
    public partial class CustomMiniPlugin : BaseUnityPlugin
    {

        public static class StateDetection
        {
            // Initialization stage
            private static int stageStart = -50;
            private static int stage = stageStart;
            private static int assetCount = 0;

            private static System.Guid subscriptionGuid = System.Guid.Empty;

            private static bool IsBoardLoaded = false;

            public static void Initiailze(System.Reflection.MemberInfo plugin)
            {
                BoardSessionManager.OnStateChange += (s) =>
                {
                    Debug.Log("StateDetection: Board Changed To " + s.ToString());
                    if (s.ToString().Contains("+Active")) { IsBoardLoaded = true; } else { IsBoardLoaded = false; }
                };

                SceneManager.sceneLoaded += (scene, mode) =>
                {
                    try
                    {
                        if (scene.name == "UI")
                        {
                            TextMeshProUGUI betaText = GetUITextByName("BETA");
                            if (betaText)
                            {
                                betaText.text = "INJECTED BUILD - unstable mods";
                            }
                        }
                        else
                        {
                            TextMeshProUGUI modListText = GetUITextByName("TextMeshPro Text");
                            if (modListText)
                            {
                                BepInPlugin bepInPlugin = (BepInPlugin)Attribute.GetCustomAttribute(plugin, typeof(BepInPlugin));
                                if (modListText.text.EndsWith("</size>"))
                                {
                                    modListText.text += "\n\nMods Currently Installed:\n";
                                }
                                modListText.text += "\nLord Ashes' " + bepInPlugin.Name + " - " + bepInPlugin.Version;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                };
            }

            public static bool Ready()
            {
                if (IsBoardLoaded)
                {
                    if (stage >= 11)
                    {
                        return true;
                    }
                    //
                    // Stage 10: Ready
                    //
                    else if (stage == 10)
                    {
                        Debug.Log("Subscribing To '" + CustomMiniPlugin.Guid + "' Messages");
                        subscriptionGuid = StatMessaging.Subscribe(CustomMiniPlugin.Guid, CustomMiniPlugin.requestHandler.Request);
                        StatMessaging.Reset();
                        stage++;
                    }
                    //
                    // Stage 2+: Waiting To Process SystemMessage
                    //
                    else if (stage >= 2)
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
                            if (asset.BaseLoader.LoadedAsset == null)
                            {
                                Debug.Log("Asset " + asset.name + " has a null BaseLoader");
                                loaded = false; break;
                            }
                            else
                            {
                                if (asset.BaseLoader.LoadedAsset.GetComponent<MeshFilter>() == null)
                                {
                                    Debug.Log("Asset " + asset.name + " has a null BaseLoader MeshFilter");
                                    loaded = false; break;
                                }
                                else
                                {
                                    if (asset.BaseLoader.LoadedAsset.GetComponent<MeshFilter>().mesh == null)
                                    {
                                        Debug.Log("Asset " + asset.name + " has a null BaseLoader MeshFilter mesh");
                                        loaded = false; break;
                                    }
                                    else
                                    {
                                        Debug.Log("Asset " + asset.name + " is ready");
                                    }
                                }
                            }
                        }
                        if (loaded)
                        {
                            Debug.Log("Minis mesh test passed. Processing transformations...");
                            StatMessaging.Reset();
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
                }
                //
                // Board is re-loading, request startup seqeunce
                //
                else if (stage >= 0)
                {
                    Debug.Log("Board Is Re-loading...");
                    StatMessaging.Unsubscribe(subscriptionGuid);
                    StatMessaging.Reset();
                    stage = stageStart;
                }
                return false;
            }

            public static TextMeshProUGUI GetUITextByName(string name)
            {
                TextMeshProUGUI[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i].name == name)
                    {
                        return texts[i];
                    }
                }
                return null;
            }
        }
    }
}