using Dummiesman;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace CustomMiniPlugin
{
    class ChatHandler
    {
        // Directory for custom content
        private string dir = "";

        // Speech font name
        private string fontName = "NAL Hand SDF";

        // Active requests
        private List<string> last = new List<string>();

        // Transformations
        Dictionary<string, string> transformations = new Dictionary<string, string>();

        /// <summary>
        /// Constructor taking in the content directory and identifiers
        /// </summary>
        /// <param name="requestIdentifiers"></param>
        /// <param name="path"></param>
        public ChatHandler(string path)
        {
            this.dir = path;
            transformations.Clear();
        }

        /// <summary>
        /// Method to detect picked up assets
        /// </summary>
        public void CheckForPickedUpAssets()
        {
            CreatureMoveBoardTool moveBoard = SingletonBehaviour<BoardToolManager>.Instance.GetTool<CreatureMoveBoardTool>();
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;
            CreatureBoardAsset cba = (CreatureBoardAsset)typeof(CreatureMoveBoardTool).GetField("_pickupObject", flags).GetValue(moveBoard);
            if (cba != null)
            {
                System.Windows.Forms.Clipboard.SetText("<size=0> ! " + cba.Creature.CreatureId.ToString() + " </size> Make me a ");
            }
        }

        /// <summary>
        /// Method to detect character speech and check for requests 
        /// </summary>
        public void CheckChatRequests()
        {
            List<string> current = new List<string>();

            TextMeshProUGUI[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
            for (int i = 0; i < texts.Length; i++)
            {
                if ((texts[i].name == "Text") && (texts[i].font.name == fontName) && (texts[i].text.Trim().Contains("! ")))
                {
                    current.Add(texts[i].text);
                    if (!last.Contains(texts[i].text))
                    {
                        texts[i].text = texts[i].text.Replace("! ", ": ");
                        UnityEngine.Debug.Log("Chat Request: " + texts[i].text);
                        string result = ProcessChatRequest(texts[i].text);
                        if(result!=null)
                        {
                            SystemMessage.DisplayInfoText(result);
                        }
                    }
                }
            }
            last = current;
        }

        /// <summary>
        /// Method to sync the transformation mesh with the character's stealth mode setting
        /// </summary>
        public void SyncStealthMode()
        {
            foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets.ToArray())
            {
                if (transformations.ContainsKey(asset.Creature.CreatureId.ToString()))
                {
                    GameObject child = GameObject.Find("CustomContent:" + asset.Creature.CreatureId);
                    if(asset.Creature.IsExplicitlyHidden && child.transform.localScale.y != 0)
                    {
                        UnityEngine.Debug.Log("Hiding Custom Mesh...");
                        child.transform.localScale = new Vector3(0, 0, 0);
                    }
                    else if (!asset.Creature.IsExplicitlyHidden && child.transform.localScale.y != 1)
                    {
                        UnityEngine.Debug.Log("Unhiding Custom Mesh...");
                        child.transform.localScale = new Vector3(1, 1, 1);
                    }
                }
            }
        }

        /// <summary>
        /// Method to process char requests
        /// </summary>
        /// <param name="request">Chat request without the identifier</param>
        /// <returns>Message to be displayed or null</returns>
        public string ProcessChatRequest(string request)
        {
            UnityEngine.Debug.Log("Chat Request: " + request);

            // Validate Input
            if (!request.Contains(" ")) { return "Syntax Error: Content file name"; }
            // Extract CreatureId
            string id = request.Substring(request.IndexOf(": ") + 2);
            id = (id+" ").Substring(0, (id+" ").IndexOf(" "));

            switch (id.ToUpper())
            {
                case "TRANSFORM":
                    return LoadTransformationFile();
                    break;
                case "DELETE":
                    return DeleteTransformationFile();
                    break;
                default:
                    if (request.ToUpper().Contains("MAKE"))
                    {
                        // Extract Content Source
                        string source = request.Substring(request.LastIndexOf(" ") + 1).Trim();

                        UnityEngine.Debug.Log("Chat Request: Id=" + id + ", Source=" + source);

                        // Check that indicated content file exits
                        if (!System.IO.File.Exists(dir + source + "/"+source+".obj")) { return "Content '" + dir + source + ".obj" + "' does not exist"; }
                        // Find indicated asset
                        UnityEngine.Debug.Log("Searching for Chat Request Asset");
                        foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets.ToArray())
                        {
                            // Check to see if asset matches indicated asset
                            if (asset.Creature.CreatureId.ToString() == id)
                            {
                                // Applu custom content to asset
                                LoadCustomContent(asset, source);
                                return null;
                            }
                        }
                        // Return an asset not found message
                        return "Mini '" + id + "' not found";
                    }
                    else
                    {
                        // Unrecognized command, do nothing
                        return null;
                    }
                    break;
            }
        }

        private string LoadTransformationFile()
        {
            string loadFile = CampaignSessionManager.Info.CampaignId + "." + BoardSessionManager.CurrentBoardInfo.Id + ".cms";
            if (System.IO.File.Exists(dir + loadFile))
            {
                string[] content = System.IO.File.ReadAllLines(dir + loadFile);
                for (int i = 0; i < content.Length; i++)
                {
                    string[] parts = content[i].Split('=');
                    foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets.ToArray())
                    {
                        if (asset.Creature.CreatureId.ToString() == parts[0])
                        {
                            asset.Creature.Speak("<size=0> ! " + parts[0] + " </size> Make me a " + parts[1]);
                        }
                    }
                }
                return "Custom Mini Transformations Loaded From:\r\n" + dir + loadFile;
            }
            else
            {
                return "Missing Saved Custom Mini Transformations In:\r\n" + dir + loadFile;
            }
        }

        private string DeleteTransformationFile()
        {
            string deleteFile = CampaignSessionManager.Info.CampaignId + "." + BoardSessionManager.CurrentBoardInfo.Id + ".cms";
            if (System.IO.File.Exists(dir + deleteFile)) { System.IO.File.Delete(dir + deleteFile); }
            return "Deleted Custom Mini Transformations Save File:\r\n" + dir + deleteFile;
        }

        /// <summary>
        /// Adds a custom mesh game object to the indicated asset remove any previous attached mesh objects
        /// </summary>
        /// <param name="asset">Parent asset to whom the custom mesh will be attached</param>
        /// <param name="source">Path and name of the content file</param>
        private void LoadCustomContent(CreatureBoardAsset asset, string source)
        {
            UnityEngine.Debug.Log("Customizing Mini '" + asset.Creature.Name + "' Using '" + dir+source + ".obj'...");
            GameObject.Destroy(GameObject.Find("CustomContent:" + asset.Creature.CreatureId));
            GameObject content = new OBJLoader().Load(dir+source+"/"+source+".obj");
            content.name = "CustomContent:" + asset.Creature.CreatureId;
            content.transform.position = asset.gameObject.transform.position;
            content.transform.SetParent(asset.BaseLoader.gameObject.transform);

            string saveFile = CampaignSessionManager.Info.CampaignId + "." + BoardSessionManager.CurrentBoardInfo.Id + ".cms";

            if (transformations.ContainsKey(asset.Creature.CreatureId.ToString()))
            {
                transformations[asset.Creature.CreatureId.ToString()] = source;
            }
            else
            {
                transformations.Add(asset.Creature.CreatureId.ToString(), source);
            }
            System.IO.File.WriteAllText(dir + saveFile, "");
            foreach(KeyValuePair<string,string> entry in transformations)
            {
                System.IO.File.AppendAllText(dir + saveFile, entry.Key+"="+entry.Value+"\r\n");
            }
        }
    }
}
