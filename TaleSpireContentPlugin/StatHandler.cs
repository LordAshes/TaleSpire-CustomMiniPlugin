using Dummiesman;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomMiniPlugin
{
    class StatHandler
    {
        // Directory for custom content
        private string dir = "";

        // Identified used to identfy request and the replacement identifier when request has been processed
        private string[] identifiers = { "!!!", "!-!" };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestIndetifiers"></param>
        /// <param name="path"></param>
        public StatHandler(string[] requestIndetifiers, string path)
        {
            this.identifiers = requestIndetifiers;
            this.dir = path;
        }

        public void CheckStatRequests()
        {
            foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets.ToArray())
            {
                if (asset.Creature.name.Contains(identifiers[0]))
                {
                    string request = asset.Creature.name.Substring(asset.Creature.name.IndexOf(identifiers[0])).Trim();
                    asset.Creature.name = asset.Creature.name.Substring(0, asset.Creature.name.IndexOf(identifiers[0])).Trim();
                    string result = ProcessStatRequest(request);
                    if (result != null)
                    {
                        // Display result message if any
                        UnityEngine.Debug.Log(result);
                        SystemMessage.DisplayInfoText(result);
                    }
                }
            }
        }

        /// <summary>
        /// Method to process char requests
        /// </summary>
        /// <param name="request">Chat request without the identifier</param>
        /// <returns>Message to be displayed or null</returns>
        public string ProcessStatRequest(string request)
        {
            UnityEngine.Debug.Log("Stat Received Request: " + request);
            // Split request
            string[] parts = request.Trim().Split(' ');
            UnityEngine.Debug.Log("Validating Stat Request");
            // Check that request has 4 or more parts
            if (parts.Length < 4) { return "Stat Request: Missing Parameters"; }
            // Check that request is a Make request
            if (parts[1].ToUpper() != "MAKE") { return "Stat Request: Unknown Command"; }
            // Check that indicated content file exits
            if (!System.IO.File.Exists(dir + parts[3] + ".obj")) { return "Content '" + dir + parts[3] + ".obj" + "' does not exist"; }
            // Find indicated asset
            UnityEngine.Debug.Log("Searching for Stat Request Asset");
            foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets.ToArray())
            {
                // Check to see if asset matches indicated asset
                if (asset.Creature.CreatureId.ToString() == parts[2])
                {
                    // Set creature stats to sync the request
                    LoadCustomContent(asset, dir + parts[3] + ".obj");
                    return null;
                }
            }
            // Return an asset not found message
            return "Mini '" + parts[1] + "' not found";
        }

        /// <summary>
        /// Adds a custom mesh game object to the indicated asset remove any previous attached mesh objects
        /// </summary>
        /// <param name="asset">Parent asset to whom the custom mesh will be attached</param>
        /// <param name="source">Path and name of the content file</param>
        private static void LoadCustomContent(CreatureBoardAsset asset, string source)
        {
            UnityEngine.Debug.Log("Customizing Mini '" + asset.Creature.Name + "' Using '" + source + "'...");
            GameObject.Destroy(GameObject.Find("CustomContent:" + asset.Creature.CreatureId));
            GameObject content = new OBJLoader().Load(source);
            content.name = "CustomContent:" + asset.Creature.CreatureId;
            content.transform.position = asset.gameObject.transform.position;
            content.transform.SetParent(asset.BaseLoader.gameObject.transform);
        }
    }
}
