using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(LordAshes.FileAccessPlugin.Guid)]
    [BepInDependency(LordAshes.StatMessaging.Guid)]
    public partial class BeyondLinkPlugin : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Beyond Link Plug-In";
        public const string Guid = "org.lordashes.plugins.beyondlink";
        public const string Version = "1.0.0.0";

        // Configuration
        public int interval = 10;
        public DateTime lastTime = DateTime.UtcNow;
        public int state = 0;

        private CharacterLink[] links = null;

        /// <summary>
        /// Function for initializing plugin
        /// This function is called once by TaleSpire
        /// </summary>
        void Awake()
        {
            UnityEngine.Debug.Log("Beyond Link Plugin: Active.");

            links = JsonConvert.DeserializeObject<CharacterLink[]>(FileAccessPlugin.File.ReadAllText("BeyondLinks.json"));

            interval = Config.Bind("Settings", "Interval Between Checks (In Seconds)", 10).Value;

            Utility.PostOnMainPage(this.GetType());
        }

        /// <summary>
        /// Function for determining if view mode has been toggled and, if so, activating or deactivating Character View mode.
        /// This function is called periodically by TaleSpire.
        /// </summary>
        void Update()
        {
            if (Utility.isBoardLoaded())
            {
                if (DateTime.UtcNow.Subtract(lastTime).TotalSeconds>=interval)
                {
                    lastTime = DateTime.UtcNow;
                    state = 1;
                    CampaignSessionManager.SetCreatureStatNames(new string[] { "AC", "HD", "Unused", "Unused", "Unused", "Unused", "Unused", "Unused" });
                    foreach (CharacterLink link in links)
                    {
                        foreach(CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets)
                        {
                            if (StatMessaging.GetCreatureName(asset.Creature)==link.name)
                            {
                                Dictionary<string, int> stats = ExtractInfo(link.beyondId);
                                if (stats != null)
                                {
                                    Debug.Log("Beyond Link Plugin: "+link.name+": Current Hitpoints Set To " + stats["cHP"] + " Of " + stats["HP"]);
                                    CreatureManager.SetCreatureStatByIndex(asset.CreatureId, new CreatureStat(stats["cHP"], stats["HP"]), -1);
                                    Debug.Log("Beyond Link Plugin: "+link.name+": AC Set To " + stats["AC"] + " Of " + stats["shAC"]); 
                                    CreatureManager.SetCreatureStatByIndex(asset.CreatureId, new CreatureStat(stats["AC"], stats["shAC"]), 0);
                                    Debug.Log("Beyond Link Plugin: " + link.name + ": Used HitDice Set To " + stats["usedHD"] + " Of " + stats["level"]);
                                    CreatureManager.SetCreatureStatByIndex(asset.CreatureId, new CreatureStat(stats["usedHD"], stats["level"]), 1);
                                }
                                else
                                {
                                    Debug.LogWarning("Beyond Link Plugin: D&D Beyond Request Failed");
                                    state = -1;
                                }
                            }
                        }
                    }
                }
                else if (DateTime.UtcNow.Subtract(lastTime).TotalSeconds > 1)
                {
                    state = 0;
                }
            }
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            switch (state)
            {
                case -1:
                    // Last check failed
                    style.normal.textColor = UnityEngine.Color.red;
                    break;
                case 1:
                    // Last check success
                    style.normal.textColor = UnityEngine.Color.green;
                    break;
                default:
                    // Waiting for next check
                    style.normal.textColor = UnityEngine.Color.blue;
                    break;
            }
            GUI.Label(new Rect(1880, 1060, 30, 20), "Link", style);
        }

        public class CharacterLink
        {
            public string name { get; set; }
            public string beyondId { get; set; }
        }
    }
}
