using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using static Gorio.CustomFunctions;
using static Gorio.Plugin;
using UnityEngine;

namespace Gorio
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs
        public static string[] myTraitList = 
        {
            "shamanduality",
            "shamanpackleader",
            "shamanstarryform",
            "shamanmeteorology",
            "shamanmoonphase"
        };

        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;

        public static void myDoTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = new List<CardData>();
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            if (!IsLivingHero(_character)) return;

            // activate traits
            if (_trait == myTraitList[0])
            {
                // When you play a Mage card, reduce the cost of the highest cost Healer card
                // in your hand by 1 until discarded. When you play a Healer card, reduce the
                // cost of the highest cost Mage card in your hand by 1 until discarded. (3 times/turn)
                // (4x if you have starry form)
                int bonusActivations = _character.HaveTrait(myTraitList[2]) ? 1 : 0;
                Duality(ref _character,ref _castedCard, Enums.CardClass.Healer, 
                    Enums.CardClass.Mage, _trait, bonusActivations : bonusActivations);
            }
            else if(_trait == myTraitList[1])
            {
                // Block +1, Shield +1, Powerful +1
                // At the start of your turn, all heroes gain 1 fast.
                ApplyAuraCurseToAll("fast", 1, AppliesTo.Heroes, _character);
            }
            else if(_trait == myTraitList[2])
            {
                // Bless +1, Sanctify +1, Regeneration +1.
                // At the start of your turn, all heroes gain 1 evasion.
                // Mage duality can be activated 4 times/turn.
                ApplyAuraCurseToAll("evasion", 1, AppliesTo.Heroes, _character);
            }
            else if(_trait == myTraitList[3])
            {
                // When you apply Spark, Chill, or Burn charges,
                // a random hero gains Shield charges equal to that amount.
                // -This amount does not gain bonuses-
                if(!(_auxString == "burn" || _auxString == "chill" || _auxString == "spark"))
                    return;

                ApplyAuraCurseToTarget("shield", _auxInt, GetRandomCharacter(teamHero), _character, false);
            }
            else if(_trait == myTraitList[4])
            {
                // On odd-numbered rounds, gain a 20% bonus to all damage.
                // On even-numbered rounds, gain a 30% bonus to all healing done and a 15% bonus to healing received.
                // (Handled in GetTraitPercentBonusPostfix methods below)
            }
            else return;

            LogDebug($"Trait {_trait}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, 
            Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                myDoTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hero), nameof(Hero.AssignTrait))]
        public static void AssignTraitPostfix(ref Hero __instance, ref bool __result, string traitName)
        {
            TraitData traitData = Globals.Instance.GetTraitData(traitName);
            if (traitData == null)
            {
                __result = false;
                return;
            }

            // Block +1, Shield +1, Powerful +1
            // At the start of your turn, all heroes gain 1 fast.
            // Upon picking up, if you have the "Astral Wolf" pet, corrupt it.
            if (traitData.Id == myTraitList[1] && __instance.Pet.StartsWith("shamanhound", StringComparison.OrdinalIgnoreCase))
            {
                __instance.Pet = "shamanhoundrare";
            }
            __result = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitDamagePercentModifiers))]
        public static void GetTraitDamagePercentModifiersPostfix(ref Character __instance, ref float __result, Enums.DamageType DamageType)
        {
            //LogDebug("GetTraitDamagePercentModifiersPostfix");

            if (isDamagePreviewActive || isCalculateDamageActive)
                return;

            if (IsLivingHero(__instance) && AtOManager.Instance != null 
                && AtOManager.Instance.CharacterHaveTrait(__instance.SubclassName, myTraitList[4]) 
                && MatchManager.Instance != null)
            {
                LogDebug("GetTraitDamagePercentModifiersPostfix - post conditional");

                // On odd-numbered rounds, gain a 20% bonus to all damage.
                // On even-numbered rounds, gain a 30% bonus to all healing done and a 15% bonus to healing received.
                int percentIncrease = MatchManager.Instance.GetCurrentRound() % 2 == 0 ? 0 : 20;

                LogInfo("GetTraitDamagePercentModifiers = " + percentIncrease);
                __result += percentIncrease;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitHealPercentBonus))]
        public static void GetTraitHealPercentBonusPostfix(ref Character __instance, ref float __result)
        {
            //LogDebug("GetTraitHealPercentBonusPostfix");

            if (isDamagePreviewActive || isCalculateDamageActive)
                return;

            if (IsLivingHero(__instance) && AtOManager.Instance != null 
                && AtOManager.Instance.CharacterHaveTrait(__instance.SubclassName, myTraitList[4]) 
                && MatchManager.Instance != null)
            {
                LogDebug("GetTraitHealPercentBonusPostfix - post conditional");

                // On odd-numbered rounds, gain a 20% bonus to all damage.
                // On even-numbered rounds, gain a 30% bonus to all healing done and a 15% bonus to healing received.
                int percentIncrease = MatchManager.Instance.GetCurrentRound() % 2 == 0 ? 30 : 0;

                LogDebug("GetTraitHealPercentBonus = " + percentIncrease);
                __result += percentIncrease;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitHealReceivedPercentBonus))]
        public static void GetTraitHealReceivedPercentBonusPostfix(ref Character __instance, ref float __result)
        {
            //LogDebug("GetTraitHealReceivedPercentBonusPostfix");

            if (isDamagePreviewActive || isCalculateDamageActive)
                return;

            if (IsLivingHero(__instance) && AtOManager.Instance != null 
                && AtOManager.Instance.CharacterHaveTrait(__instance.SubclassName, myTraitList[4]) 
                && MatchManager.Instance != null)
            {
                LogDebug("GetTraitHealReceivedPercentBonusPostfix - post conditional");

                // On odd-numbered turns, gain a 20% bonus to all damage.
                // On even-numbered turns, gain a 30% bonus to all healing done and a 15% bonus to healing received.
                int percentIncrease = MatchManager.Instance.GetCurrentRound() % 2 == 0 ? 15 : 0;

                LogDebug("GetTraitHealReceivedPercentBonus = " + percentIncrease);
                __result += percentIncrease;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPrefix()
        {
            isCalculateDamageActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPostfix()
        {
            isCalculateDamageActive = false;
        }
    }
}
