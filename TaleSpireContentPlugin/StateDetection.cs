using BepInEx;
using UnityEngine;

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

            public static bool Ready(ref LordAshes.CustomMiniPlugin.RequestHandler requestHandler)
            {
                if (IsBoardLoaded())
                {
                    //
                    // Stage 10: Ready
                    //
                    if (stage == 10)
                    {
                        return true;
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
                    StatMessaging.Reset();
                    stage = stageStart;
                }
                return (stage == 10);
            }

            private static bool IsBoardLoaded()
            {
                return (CameraController.HasInstance && BoardSessionManager.HasInstance && BoardSessionManager.HasBoardAndIsInNominalState && !BoardSessionManager.IsLoading);
            }
        }
    }
}

