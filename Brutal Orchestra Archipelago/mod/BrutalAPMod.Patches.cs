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
        [HarmonyPatch(typeof(GameInformationHolder), "PostCombatProcess")]
        class BattlePatch
        {
            static void Postfix(GameInformationHolder __instance)
            {
                if (__instance == null) return;

                InCombat = false;
                if (pendingDeath)
                {
                    TryProcessPendingDeath();
                    return;
                }

                if (cachedHolder != __instance)
                {
                    cachedHolder = __instance;
                    Debug.Log("AP: Holder cached");
                }

                ApplyPendingCoins();
                ProcessPendingActions();

                RunDataSO run = __instance.Run;
                int zoneID = run?.CurrentZoneID ?? -1;
                
                if (run != null && IsBossFight(run))
                {
                    Debug.Log("AP: Boss fight — no battle check");
                    return;
                }

                if (IsGardenZone())
                {
                    if (gardenBattleCount < gardenBattleLimit)
                    {
                        gardenBattleCount++;
                        SendCheck("Garden_Battle_" + gardenBattleCount);
                    }
                    return;
                }

                if (zoneID == 0)
                {
                    if (farBattleCount >= farBattleLimit) return;
                    farBattleCount++;
                    SendCheck("Far_Battle_" + farBattleCount);
                }
                else if (zoneID == 1)
                {
                    if (orpBattleCount >= orpBattleLimit) return;
                    orpBattleCount++;
                    SendCheck("Orp_Battle_" + orpBattleCount);
                }
            }
        }

        [HarmonyPatch(typeof(CombatManager), "InitializeCombat")]
        class CombatStartPatch
        {
            static void Postfix()
            {
                InCombat = true;
            }
        }

        [HarmonyPatch(typeof(CombatManager), "ProcessGameOverEnding")]
        class DeathLinkSendPatch
        {
            static void Postfix()
            {
                if (!DeathLinkEnabled) return;
                if (suppressDeathLink)
                {
                    suppressDeathLink = false;
                    return;
                }

                string src = playerIdToName.TryGetValue(mySlot, out var n) ? n : "Player";
                apClient?.SendDeathLink(src + "'s party was wiped");
            }
        }

        [HarmonyPatch(typeof(MoneyChestContentData), "OpenTreasure")]
        class MoneyChestPatch
        {
            static void Postfix()
            {
                Debug.Log("AP: MoneyChestPatch triggered");
                if (cachedHolder == null)
                {
                    cachedHolder = Resources.FindObjectsOfTypeAll<GameInformationHolder>().FirstOrDefault();
                }

                if (cachedHolder?.Run == null)
                {
                    Debug.Log("AP: Holder or Run is null, aborting check");
                    return;
                }

                int zone = cachedHolder.Run.CurrentZoneID;
                if (IsGardenZone())
                {
                    if (gardenMoneyChestCount >= gardenMoneyLimit) return;
                    gardenMoneyChestCount++;
                    SendCheck("Garden_MoneyChest_" + gardenMoneyChestCount);
                    return;
                }

                if (zone == 0)
                {
                    if (farMoneyChestCount >= farMoneyLimit) return;
                    farMoneyChestCount++;
                    SendCheck("Far_MoneyChest_" + farMoneyChestCount);
                }
                else if (zone == 1)
                {
                    if (orpMoneyChestCount >= orpMoneyLimit) return;
                    orpMoneyChestCount++;
                    SendCheck("Orp_MoneyChest_" + orpMoneyChestCount);
                }
            }
        }

        [HarmonyPatch(typeof(PrizeContentData), "OpenTreasure")]
        class ArtifactChestPatch
        {
            static void Postfix()
            {
                Debug.Log("AP: ArtifactChestPatch triggered");
                if (cachedHolder == null)
                {
                    cachedHolder = Resources.FindObjectsOfTypeAll<GameInformationHolder>().FirstOrDefault();
                }

                if (cachedHolder?.Run == null)
                {
                    Debug.Log("AP: Holder or Run is null, aborting check");
                    return;
                }

                int zone = cachedHolder.Run.CurrentZoneID;
                if (IsGardenZone())
                {
                    if (gardenArtifactChestCount >= gardenArtifactLimit) return;
                    gardenArtifactChestCount++;
                    SendCheck("Garden_ArtifactChest_" + gardenArtifactChestCount);
                    return;
                }

                if (zone == 0)
                {
                    if (farArtifactChestCount >= farArtifactLimit) return;
                    farArtifactChestCount++;
                    SendCheck("Far_ArtifactChest_" + farArtifactChestCount);
                }
                else if (zone == 1)
                {
                    if (orpArtifactChestCount >= orpArtifactLimit) return;
                    orpArtifactChestCount++;
                    SendCheck("Orp_ArtifactChest_" + orpArtifactChestCount);
                }
            }
        }

        [HarmonyPatch(typeof(RunDataSO), "BuyFoolCharacter")]
        class BuyFoolPatch
        {
            static void Prefix() => AllowUnlocks = true;

            static void Postfix(CharacterSO __result)
            {
                AllowUnlocks = false;
                if (__result == null || buyHeroCount >= buyHeroLimit) return;
                buyHeroCount++;
                SendCheck("BuyHero_" + buyHeroCount);
            }
        }

        [HarmonyPatch(typeof(RunDataSO), "BuyShopItem")]
        class ShopPatch
        {
            static void Postfix()
            {
                int zone = cachedHolder?.Run?.CurrentZoneID ?? -1;
                if (zone == 0)
                {
                    if (farShopCount >= shopCountFar) return;
                    farShopCount++;
                    SendCheck("Shop_Far_" + farShopCount);
                }
                else if (zone == 1)
                {
                    if (orpShopCount >= shopCountOrp) return;
                    orpShopCount++;
                    SendCheck("Shop_Orp_" + orpShopCount);
                }
            }
        }

        [HarmonyPatch(typeof(OverworldManagerBG), "ChangeZone")]
        class ZoneLockPatch
        {
            static bool Prefix(OverworldManagerBG __instance)
            {
                var holderField = typeof(OverworldManagerBG).GetField("_informationHolder",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var holder = holderField.GetValue(__instance) as GameInformationHolder;
                var run = holder.Run;
                int nextZone = run.CurrentZoneID + 1;

                if (nextZone == 1 && !receivedAPItems.Contains("Orpheum Access"))
                {
                    Debug.Log("AP BLOCKED: Need Orpheum Access");
                    return false;
                }

                if (nextZone == 2)
                {
                    string nextZoneType = run.GetNextZoneType();
                    Debug.Log($"AP: next zone type = '{nextZoneType}'");

                    bool isQuarry = nextZoneType == "Pit";   // обычный Quarry

                    if (isQuarry)
                    {
                        if (!receivedAPItems.Contains("Quarry Access"))
                        {
                            Debug.Log("AP BLOCKED: Need Quarry Access");
                            return false;
                        }
                    }
                    else   // Garden (хардмод-вариант зоны 2) или любое другое имя
                    {
                        if (!receivedAPItems.Contains("Garden Access"))
                        {
                            Debug.Log("AP BLOCKED: Need Garden Access");
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(AchievementsManagerData), "TryUnlockAchievement", new Type[] { typeof(Achievement) })]
        class AchievementCheckPatch
        {
            static void Postfix(Achievement achievement)
            {
                Debug.Log($"AP: AchPatch FIRED ach={achievement} allowUnlocks={AllowUnlocks} apClient={(apClient != null)}");
                if (AllowUnlocks) return;

                if (achievement == Achievement.ACH_Ending_Easy_EnterHardmode)
                {
                    SendCheck("I'll Make You Regret This");
                    return;
                }

                if (achievementCheckMap.TryGetValue(achievement, out string checkName))
                {
                    Debug.Log($"AP: AchPatch mapped -> {checkName}");
                    SendCheck(checkName);
                }
                else Debug.Log($"AP: AchPatch NO MAP for {achievement}");
            }
        }

        [HarmonyPatch(typeof(UnlockablesManager), "TryUnlockContent")]
        class BlockUnlockContentPatch
        {
            static bool Prefix(UnlockablesManager __instance, UnlockableModData data)
            {
                if (AllowUnlocks) return true;
                if (data == null) return false;

                string id = Traverse.Create(data).Field("id").GetValue() as string;
                if (!string.IsNullOrEmpty(id) && itemCheckNames.TryGetValue(id, out string loc))
                    SendCheck(loc);

                if (data.hasQuestCompletion)
                {
                    var game = Traverse.Create(__instance).Field("_game").GetValue<InGameDataSO>();
                    game?.TryCompleteQuest(data.questID);
                }

                if (data.HasAchievementUnlock)
                {
                    var steamAch = Traverse.Create(__instance).Field("_steamAchievements")
                        .GetValue<AchievementsManagerData>();
                    steamAch?.TryUnlockAchievement(data.AchievementID);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(UnlockablesManager), "TryUnlockHardMode")]
        class BlockHardmodePatch
        {
            static bool Prefix() => AllowUnlocks;
        }

        private static void DumpUnlockableIDs()
        {
            var manager = cachedHolder?.UnlockableManager ??
                          Resources.FindObjectsOfTypeAll<UnlockablesManager>().FirstOrDefault();
            if (manager == null)
            {
                Debug.Log("No UnlockablesManager found");
                return;
            }

            Debug.Log("=== UNLOCKABLE IDs IN MANAGER ===");
            foreach (var (key, _) in EnumerateUnlockables(manager))
                Debug.Log($"ID: {key}");
        }

        [HarmonyPatch(typeof(InGameDataSO), "TryUnlockItem")]
        class BlockItemUnlockPatch
        {
            static bool Prefix() => BrutalAPMod.AllowUnlocks; // false = вызов заблокирован
        }

        [HarmonyPatch(typeof(InGameDataSO), "TryUnlockCharacter")]
        class BlockCharacterUnlockPatch
        {
            static bool Prefix() => BrutalAPMod.AllowUnlocks;
        }

        [HarmonyPatch(typeof(GameInformationHolder), "PrepareGameRun")]
        class NewRunPatch
        {
            static void Postfix(RunDataSO run)
            {
                if (abilitySanity) ClearRunAbilityPick();

                if (run?.playerData == null || startMoneyLevel <= 0) return;
                run.playerData.AddCurrency(startMoneyLevel * 10);
                Debug.Log($"AP: Granted start money: {startMoneyLevel * 10}");
            }
        }

        [HarmonyPatch(typeof(CharacterCombat), "SetUpDefaultAbilities")]
        class AbilitySanityPatch
        {
            static void Postfix(CharacterCombat __instance)
            {
                if (!abilitySanity) return;
                if (!heroEntityToAbilityItem.TryGetValue(__instance.EntityID, out string item)) return;

                var ch = __instance.Character;
                if (ch == null) return;

                bool hasSlap = ch.usesBasicAbility;
                int rankCount = GetRankAbilityCount(__instance);

                var listField = Traverse.Create(__instance).Field("_combatAbilities");
                var combat = listField.GetValue<List<CombatAbility>>();
                if (combat == null) return;

                // Сколько родных абилок положила игра (slap + ранговые), чтобы отделить экстра-хвост
                var used = Traverse.Create(__instance).Field("_usedAbilities").GetValue<int[]>() ?? new int[0];
                bool usesAll = ch.usesAllAbilities;
                int nativeOrig = (hasSlap ? 1 : 0) + (usesAll ? rankCount : used.Length);
                if (nativeOrig > combat.Count) nativeOrig = combat.Count;

                var extra = combat.Skip(nativeOrig).ToList();

                var pick = GetOrCreatePick(__instance, item, hasSlap, rankCount);

                var rebuilt = new List<CombatAbility>();
                foreach (int code in pick)
                {
                    CharacterAbility src = (code == 0) ? ch.basicCharAbility : GetRankAbility(__instance, code - 100);
                    if (src != null) rebuilt.Add(new CombatAbility(src));
                }
                rebuilt.AddRange(extra);

                listField.SetValue(rebuilt);
                Debug.Log($"AP: {__instance.EntityID} abilities rebuilt: {rebuilt.Count - extra.Count} native (pick=[{string.Join(",", pick)}]) + {extra.Count} extra");
            }
        }

        [HarmonyPatch(typeof(MainMenuController), "Start")]
        class SkipTutorialMenuPatch
        {
            static void Postfix(MainMenuController __instance)
            {
                var tr = Traverse.Create(__instance);
                tr.Field("_goodEasyEndingUI").GetValue<GameObject>()?.SetActive(false);
                tr.Field("_firstTutorialUI").GetValue<GameObject>()?.SetActive(false);
                tr.Field("_basicMenuUI").GetValue<GameObject>()?.SetActive(true);

                // выдать стартовых героев до того, как меню закеширует список
                var holder = Resources.FindObjectsOfTypeAll<GameInformationHolder>().FirstOrDefault();
                var game = holder?.Game;
                if (game != null)

                Debug.Log("AP: Forced basic menu");
            }
        }
        
        [HarmonyPatch(typeof(SelectableCharacterData), "GetAndTrySetUpIfUnlocked")]
        class ForceStarterCharactersPatch
        {
            static void Postfix(SelectableCharacterData __instance, CharacterSO __result)
            {
                var nameField = Traverse.Create(__instance).Field("_characterName");
                string charName = nameField.GetValue<string>();
                if (charName != "Boyle_CH" && charName != "Hans_CH") return;

                Traverse.Create(__instance).Property("LoadedCharacter").SetValue(__result);
                Debug.Log($"AP: Forced starter character '{charName}' as unlocked");
            }
        }
    }
}