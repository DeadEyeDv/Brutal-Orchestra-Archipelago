using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace BrutalOrchestraAr
{
    public partial class BrutalAPMod : BaseUnityPlugin
    {
        public static void SaveRunCounter()
        {
            PlayerPrefs.SetInt("BrutalAP_RunCounter_" + (APClient.StateKey), runCounter);
            PlayerPrefs.Save();
        }
        public static void LoadRunCounter()
        {
            runCounter = PlayerPrefs.GetInt("BrutalAP_RunCounter_" + (APClient.StateKey), 0);
        }
        
        private static string GetPlayerPrefsKey() =>
            !string.IsNullOrEmpty(APClient.CurrentSeed) ? "BrutalAP_" + APClient.CurrentSeed : "BrutalAP_ReceivedItems";

        public static void LoadReceivedItemsForCurrentSeed()
        {
            string key = GetPlayerPrefsKey();
            if (!PlayerPrefs.HasKey(key)) return;
            string data = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(data)) return;
            foreach (var item in data.Split(';'))
                if (!string.IsNullOrEmpty(item))
                    receivedAPItems.Add(item);
            Debug.Log("AP: Loaded received items for seed " + APClient.CurrentSeed + ": " + data);
        }
        
        static void SaveReceivedItems()
        {
            string data = string.Join(";", receivedAPItems);
            PlayerPrefs.SetString(GetPlayerPrefsKey(), data);
            PlayerPrefs.Save();
            Debug.Log("AP: Saved received items: " + data);
        }

        public static void SaveCounters()
        {
            var counters = new Dictionary<string, int>
            {
                { "farBattle", farBattleCount }, { "orpBattle", orpBattleCount },
                { "farMoney", farMoneyChestCount }, { "orpMoney", orpMoneyChestCount },
                { "farArtifact", farArtifactChestCount }, { "orpArtifact", orpArtifactChestCount },
                { "farShop", farShopCount }, { "orpShop", orpShopCount }, { "buyHero", buyHeroCount },
                { "gardenBattle", gardenBattleCount }, { "gardenMoney", gardenMoneyChestCount },
                { "gardenArtifact", gardenArtifactChestCount }, { "gardenShop", gardenShopCount }
            };
            string data = string.Join(";", counters.Select(kv => kv.Key + "=" + kv.Value));
            string key = "BrutalAP_Counters_" + (APClient.StateKey);
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
            Debug.Log("AP: Saved counters: " + data);
        }
        
        public static void LoadCountersForCurrentSeed()
        {
            string key = "BrutalAP_Counters_" + (APClient.StateKey);
            if (!PlayerPrefs.HasKey(key)) return;
            string data = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(data)) return;
            foreach (var pair in data.Split(';'))
            {
                var parts = pair.Split('=');
                if (parts.Length != 2 || !int.TryParse(parts[1], out int val)) continue;
                switch (parts[0])
                {
                    case "farBattle": farBattleCount = val; break;
                    case "orpBattle": orpBattleCount = val; break;
                    case "farMoney": farMoneyChestCount = val; break;
                    case "orpMoney": orpMoneyChestCount = val; break;
                    case "farArtifact": farArtifactChestCount = val; break;
                    case "orpArtifact": orpArtifactChestCount = val; break;
                    case "farShop": farShopCount = val; break;
                    case "orpShop": orpShopCount = val; break;
                    case "buyHero": buyHeroCount = val; break;
                    case "gardenBattle": gardenBattleCount = val; break;
                    case "gardenMoney": gardenMoneyChestCount = val; break;
                    case "gardenArtifact": gardenArtifactChestCount = val; break;
                    case "gardenShop": gardenShopCount = val; break;
                }
            }

            Debug.Log("AP: Loaded counters for seed " + APClient.CurrentSeed);
        }
        
        public static void SaveSentChecks()
        {
            if (string.IsNullOrEmpty(APClient.CurrentSeed)) return;  // не сохраняем без сида
            string key = "BrutalAP_SentChecks_" + APClient.CurrentSeed;
            PlayerPrefs.SetString(key, string.Join(";", sentChecks.ToArray()));
            PlayerPrefs.Save();
        }

        public static void LoadSentChecks()
        {
            if (string.IsNullOrEmpty(APClient.CurrentSeed)) return;  // сид не готов — не трогаем sentChecks
            sentChecks.Clear();
            string key = "BrutalAP_SentChecks_" + APClient.CurrentSeed;
            if (!PlayerPrefs.HasKey(key)) { Debug.Log("AP: No saved checks for this seed (fresh)"); return; }
            string data = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(data)) return;
            foreach (var c in data.Split(';'))
                if (!string.IsNullOrEmpty(c))
                    sentChecks.Add(c);
            Debug.Log("AP: Loaded sent checks: " + data);
        }
        
        public static void LoadBossDefeatCount()
        {
            string key = "BrutalAP_BossDefeat_" + (APClient.StateKey);
            if (PlayerPrefs.HasKey(key)) bossDefeatCount = PlayerPrefs.GetInt(key);
        }

        public static void SaveBossDefeatCount()
        {
            string key = "BrutalAP_BossDefeat_" + (APClient.StateKey);
            PlayerPrefs.SetInt(key, bossDefeatCount);
            PlayerPrefs.Save();
        }
        
        public static void LoadProcessedItemIndex()
        {
            string key = "BrutalAP_ItemIndex_" + (APClient.CurrentSeed);
            if (PlayerPrefs.HasKey(key)) processedItemIndex = PlayerPrefs.GetInt(key);
        }

        public static void SaveProcessedItemIndex()
        {
            if (string.IsNullOrEmpty(APClient.CurrentSeed)) return; // сид ещё не известен — не пишем
            PlayerPrefs.SetInt("BrutalAP_ItemIndex_" + APClient.CurrentSeed, processedItemIndex);
            PlayerPrefs.Save();
        }
        
        public static void SaveAbilityLevels()
        {
            string key = "BrutalAP_Abilities_" + (APClient.StateKey);
            PlayerPrefs.SetString(key, string.Join(";", abilityLevels.Select(kv => kv.Key + "=" + kv.Value)));
            PlayerPrefs.Save();
        }

        public static void LoadAbilityLevels()
        {
            abilityLevels.Clear();
            string key = "BrutalAP_Abilities_" + (APClient.StateKey);
            string data = PlayerPrefs.GetString(key, "");
            foreach (var pair in data.Split(';'))
            {
                var parts = pair.Split('=');
                if (parts.Length == 2 && int.TryParse(parts[1], out int v)) abilityLevels[parts[0]] = v;
            }
        }
        
        public static void SaveRunAbilityPick()
        {
            string key = "BrutalAP_RunAbilities_" + (APClient.StateKey);
            // формат: entity:idx,idx;entity2:idx,idx
            var parts = runAbilityPick.Select(kv => kv.Key + ":" + string.Join(",", kv.Value));
            PlayerPrefs.SetString(key, string.Join(";", parts));
            PlayerPrefs.Save();
        }

        public static void LoadRunAbilityPick()
        {
            runAbilityPick.Clear();
            string key = "BrutalAP_RunAbilities_" + (APClient.StateKey);
            string data = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(data)) return;
            foreach (var entry in data.Split(';'))
            {
                if (string.IsNullOrEmpty(entry)) continue;
                var kv = entry.Split(':');
                if (kv.Length != 2) continue;
                var idxs = kv[1].Split(',').Where(s => s.Length > 0).Select(int.Parse).ToList();
                runAbilityPick[kv[0]] = idxs;
            }
        }

        public static void ClearRunAbilityPick()
        {
            runAbilityPick.Clear();
            PlayerPrefs.DeleteKey("BrutalAP_RunAbilities_" + (APClient.StateKey));
            runCounter++;
            SaveRunCounter();
            PlayerPrefs.Save();
            Debug.Log($"AP: Rerolled ability picks for new run (run #{runCounter})");
        }
    }
}