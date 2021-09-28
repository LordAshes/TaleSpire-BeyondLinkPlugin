using BepInEx;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace LordAshes
{
    public partial class BeyondLinkPlugin : BaseUnityPlugin
    {
        public Dictionary<string,int> ExtractInfo(string characterId)
        {
            try
            {
                Dictionary<string, int> stats = new Dictionary<string, int>();
                string content = "";
                using (WebClient wc = new WebClient())
                {
                    content = wc.DownloadString("https://character-service.dndbeyond.com/character/v3/character/" + characterId);
                }
                stats.Add("sHP", int.Parse(ConsumeKey(ref content, "\"baseHitPoints\"")));
                string bonusHitpoints = ConsumeKey(ref content, "\"bonusHitPoints\"");
                if (bonusHitpoints == "null") { stats.Add("bHP", 0); } else { stats.Add("bHP", int.Parse(bonusHitpoints)); }
                stats.Add("damage", int.Parse(ConsumeKey(ref content, "\"removedHitPoints\"")));
                stats.Add("tHP", int.Parse(ConsumeKey(ref content, "\"temporaryHitPoints\"")));
                MovePast(ref content, "stats");
                foreach (string stat in new string[] { "sSTR", "sDEX", "sCON", "sINT", "sWIS", "sCHA", "bSTR", "bDEX", "bCON", "bINT", "bWIS", "bCHA", "oSTR", "oDEX", "oCON", "oINT", "oWIS", "oCHA" })
                {
                    string value = ConsumeKey(ref content, "\"value\"");
                    if (value != "null")
                    {
                        stats.Add(stat, int.Parse(value));
                    }
                    else
                    {
                        stats.Add(stat, 0);
                    }
                }
                MovePast(ref content, "Racial Traits");
                MovePast(ref content, "/>");
                content = content.Trim();
                stats.Add("rSTR", 0);
                stats.Add("rDEX", 0);
                stats.Add("rCON", 0);
                stats.Add("rINT", 0);
                stats.Add("rWIS", 0);
                stats.Add("rCHA", 0);
                while (content.Substring(0, 1) == "+" || content.Substring(0, 1) == "-")
                {
                    string value = ConsumeKey(ref content, "");
                    int num = int.Parse(value.Substring(0, value.IndexOf(" ")));
                    value = value.Substring(value.IndexOf(" ")).Trim();
                    switch (value)
                    {
                        case "Strength": stats["rSTR"] = num; break;
                        case "Dexterity": stats["rDEX"] = num; break;
                        case "Constitution": stats["rCON"] = num; break;
                        case "Intelligence": stats["rINT"] = num; break;
                        case "Wisdom": stats["rWIS"] = num; break;
                        case "Charisma": stats["rCHA"] = num; break;
                        default: break;
                    }
                    content = content.Substring(1).Trim();
                }

                string temp = content;
                stats.Add("level", int.Parse(ConsumeKey(ref temp, "\"level\"")));
                stats.Add("usedHD", stats["level"] - int.Parse(ConsumeKey(ref temp, "\"hitDiceUsed\"")));

                if (stats["oSTR"] > 0) { stats.Add("STR", stats["oSTR"]); } else { stats.Add("STR", stats["sSTR"] + stats["rSTR"] + stats["bSTR"]); }
                if (stats["oDEX"] > 0) { stats.Add("DEX", stats["oDEX"]); } else { stats.Add("DEX", stats["sDEX"] + stats["rDEX"] + stats["bDEX"]); }
                if (stats["oCON"] > 0) { stats.Add("CON", stats["oCON"]); } else { stats.Add("CON", stats["sCON"] + stats["rCON"] + stats["bCON"]); }
                if (stats["oINT"] > 0) { stats.Add("INT", stats["oINT"]); } else { stats.Add("INT", stats["sINT"] + stats["rINT"] + stats["bINT"]); }
                if (stats["oWIS"] > 0) { stats.Add("WIS", stats["oWIS"]); } else { stats.Add("WIS", stats["sWIS"] + stats["rWIS"] + stats["bWIS"]); }
                if (stats["oCHA"] > 0) { stats.Add("CHA", stats["oCHA"]); } else { stats.Add("CHA", stats["sCHA"] + stats["rCHA"] + stats["bCHA"]); }

                stats.Add("HP", stats["sHP"] + stats["bHP"] + stats["level"] * Mod(stats["CON"]));
                stats.Add("cHP", stats["HP"] - stats["damage"]);

                stats.Add("bAC", 0);
                stats.Add("AC", 10+Mod(stats["DEX"]));
                stats.Add("Shield", 0);
                while (content.Contains("baseArmorName"))
                {
                    string armorType = ConsumeKey(ref content, "\"baseArmorName\"");
                    if (armorType != "null")
                    {
                        string ac = ConsumeKey(ref content, "\"armorClass\"");
                        Debug.Log("Found '" + armorType + "' AC '" + ac + "'");
                        switch (armorType)
                        {
                            case "Padded": stats["AC"] = int.Parse(ac) + Mod(stats["DEX"]); break;
                            case "Leather": stats["AC"] = int.Parse(ac) + Mod(stats["DEX"]); break;
                            case "Studded Leather": stats["AC"] = int.Parse(ac) + Mod(stats["DEX"]); break;
                            case "Hide": stats["AC"] = int.Parse(ac) + Math.Min(Mod(stats["DEX"]), 2); break;
                            case "Chain Shirt": stats["AC"] = int.Parse(ac) + Math.Min(Mod(stats["DEX"]), 2); break;
                            case "Scale Mail": stats["AC"] = int.Parse(ac) + Math.Min(Mod(stats["DEX"]), 2); break;
                            case "Breastplate": stats["AC"] = int.Parse(ac) + Math.Min(Mod(stats["DEX"]), 2); break;
                            case "Half Plate": stats["AC"] = int.Parse(ac) + Math.Min(Mod(stats["DEX"]), 2); break;
                            case "Ring Mail": stats["AC"] = int.Parse(ac); break;
                            case "Chain Mail": stats["AC"] = int.Parse(ac); break;
                            case "Splint": stats["AC"] = int.Parse(ac); break;
                            case "Plate": stats["AC"] = int.Parse(ac); break;
                            case "Shield": stats["Shield"] = int.Parse(ac); break;
                            default: stats["bAC"] = stats["bAC"] + int.Parse(ac); break;
                        }
                    }
                }
                stats["AC"] = stats["AC"] + stats["bAC"];
                stats["shAC"] = stats["AC"] + stats["Shield"];

                /*
                MovePast(ref content, "\"modifiers\"");
                while(content.Contains("friendlyTypeName\":\"Proficiency"))
                {
                    string prof = ConsumeKey(ref content,"\"friendlySubtypeName\"");
                    Debug.Log("Prof: " + prof);
                }
                */

                return stats;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public static bool MoveTo(ref string content, string key)
        {
            if (content.IndexOf(key) > -1)
            {
                content = content.Substring(content.IndexOf(key));
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool MovePast(ref string content, string key)
        {
            if (MoveTo(ref content, key))
            {
                content = content.Substring(key.Length);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string ConsumeKey(ref string content, string key)
        {
            MovePast(ref content, key);
            if (content.StartsWith(":")) { content = content.Substring(1); }
            string value = content.Substring(0, content.IndexOf(","));
            content = content.Substring(content.IndexOf(","));
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1);
                value = value.Substring(0, value.Length - 1);
            }
            if (value.EndsWith("}")) { value = value.Substring(0, value.Length - 1); }
            if (value.EndsWith("}]")) { value = value.Substring(0, value.Length - 2); }
            return value;
        }

        public static int Mod(int stat)
        {
            if (stat >= 10) { return (int)Math.Floor((decimal)((stat - 10) / 2)); } else { return -1 * (int)Math.Ceiling((decimal)((10 - stat) / 2)); }
        }
    }
}
