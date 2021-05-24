using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using BepInEx;
using Bounce.Unmanaged;
using System.Linq;

using Dummiesman;
using TMPro;
using System.Reflection;
using System.Collections.Generic;

namespace CustomMiniPlugin
{
    [BepInPlugin("org.d20armyknife.plugins.CustomMini", "Custom Mini Plug-In", "1.5.0.0")]
    public class CustomMiniPlugin : BaseUnityPlugin
    {
        // Used to turn diagnostics on or off
        private bool diagnostic = true;

        // Content directory
        private static string dir = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.LastIndexOf("/")) + "/TaleSpire_CustomData/";

        // Chat handelr
        ChatHandler chatHandler = new ChatHandler(dir);

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.Log("Custom Mini Plugin Active. Using Custom Minis In '" + dir + "'");
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            // Ensure that there is a camera controller instance
            if (CameraController.HasInstance)
            {
                // Ensure that there is a board session manager instance
                if (BoardSessionManager.HasInstance)
                {
                    // Ensure that there is a board
                    if (BoardSessionManager.HasBoardAndIsInNominalState)
                    {
                        // Ensure that the board is not loading
                        if (!BoardSessionManager.IsLoading)
                        {
                            chatHandler.CheckForPickedUpAssets();
                            chatHandler.CheckChatRequests();
                            chatHandler.SyncStealthMode();
                        }
                    }
                }
            }
        }
    }
}
