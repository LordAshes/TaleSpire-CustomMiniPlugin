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
            public static int stageStart = -200;
            public static int stage = (stageStart-10);
            private static int assetCount = 0;

            private static System.Guid[] subscriptionGuid = new System.Guid[] { System.Guid.Empty, System.Guid.Empty, System.Guid.Empty };

            public static void Initiailze(System.Reflection.MemberInfo plugin)
            {
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
                if (!BoardSessionManager.HasInstance) { return false; }
                if (!BoardSessionManager.IsLoading)
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
                        Debug.Log("Resetting StatMessage Data...");
                        StatMessaging.Reset(CustomMiniPlugin.Guid);
                        StatMessaging.Reset(CustomMiniPlugin.Guid + ".effect");
                        StatMessaging.Reset(CustomMiniPlugin.Guid + ".assetAnimation");
                        Debug.Log("Subscribing To '" + CustomMiniPlugin.Guid + "' Messages");
                        subscriptionGuid[0] = StatMessaging.Subscribe(CustomMiniPlugin.Guid, CustomMiniPlugin.requestHandler.Request);
                        Debug.Log("Subscribing To '" + CustomMiniPlugin.Guid + ".effect' Messages");
                        subscriptionGuid[1] = StatMessaging.Subscribe(CustomMiniPlugin.Guid+".effect", CustomMiniPlugin.requestHandler.Request);
                        Debug.Log("Subscribing To '" + CustomMiniPlugin.Guid + ".assetAnimation' Messages");
                        subscriptionGuid[2] = StatMessaging.Subscribe(CustomMiniPlugin.Guid+".assetAnimation", CustomMiniPlugin.requestHandler.Request);
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
                            if (CustomMiniPlugin.diagnosticMode){ Debug.Log("Checking '" + asset.name + "'..."); }
                            if (asset.CreatureLoaders[0].LoadedAsset == null)
                            {
                                Debug.Log("Asset " + asset.name + " has a null CreatureLoader[0]");
                                loaded = false; break;
                            }
                            else
                            {
                                if (CustomMiniPlugin.diagnosticMode){ Debug.Log("Asset " + asset.name + " has a CreatureLoader[0]"); }
                                if (asset.CreatureLoaders[0].LoadedAsset.GetComponent<MeshFilter>() == null)
                                {
                                    Debug.Log("Asset " + asset.name + " has a null CreatureLoaders[0] MeshFilter");
                                    loaded = false; break;
                                }
                                else
                                {
                                    if (CustomMiniPlugin.diagnosticMode){ Debug.Log("Asset " + asset.name + " has a MeshFilter"); }
                                    if (asset.CreatureLoaders[0].LoadedAsset.GetComponent<MeshFilter>().mesh == null)
                                    {
                                        Debug.Log("Asset " + asset.name + " has a null CreatureLoaders[0] MeshFilter mesh");
                                        loaded = false; break;
                                    }
                                    else
                                    {
                                        if (CustomMiniPlugin.diagnosticMode){ Debug.Log("Asset " + asset.name + " is ready"); }
                                    }
                                }
                            }
                            if (!loaded) { break; }
                        }
                        if (loaded)
                        {
                            Debug.Log("Minis mesh test passed. Processing transformations...");
                            SystemMessage.DisplayInfoText("Please Be Patient...\r\nLoading Mini Transformations");
                            stage = 2;
                        }
                        else
                        {
                            Debug.Log("Minis mesh test failed...");
                        }
                    }
                    //
                    // Stage <=0: Mini Loaded Detection
                    //
                    else if (stage <= 0)
                    {
                        if (stage == stageStart)
                        { 
                            if(CustomMiniPlugin.diagnosticMode){ Debug.Log("Starting Mini Detection System..."); }
                            SystemMessage.DisplayInfoText("Waiting For Minis To Complete Loading..."); 
                        }
                        if (CreaturePresenter.AllCreatureAssets.Count == assetCount)
                        {
                            if (CustomMiniPlugin.diagnosticMode){ Debug.Log("I still see " + assetCount + " minis (" + stage + " of " + stageStart + ")"); }
                            stage++;
                        }
                        else
                        {
                            assetCount = CreaturePresenter.AllCreatureAssets.Count;
                            if (CustomMiniPlugin.diagnosticMode){ Debug.Log("No! Now I see " + assetCount + " minis (" + stage + " of " + stageStart + ")"); }
                            stage = (stage<(stageStart + 1)) ? stage : (stageStart+1);
                        }
                    }
                    // Debug.Log(stage + " of " + stageStart);
                }
                //
                // Board is re-loading, request startup seqeunce
                //
                else
                {
                    for (int g=0; g<subscriptionGuid.Length; g++)
                    {
                        StatMessaging.Unsubscribe(subscriptionGuid[g]);
                        subscriptionGuid[g] = System.Guid.Empty;
                    }
                    stage = (stageStart-10);
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