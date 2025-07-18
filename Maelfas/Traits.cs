using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using static Maelfas.CustomFunctions;
using static Maelfas.Plugin;

namespace Maelfas
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs
        public static string[] myTraitList = 
        { 
            "defilerduality",
            "defilerplague",
            "defilersuffering",
            "defilerflickershadow",
            "defilerspikes"
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
                // When you play a Healer card, reduce the cost of the highest cost Warrior card
                // in your hand by 1 until discarded. When you play a Warrior card, reduce the
                // cost of the highest cost Healer card in your hand by 1 until discarded. (3 times/turn)
                // (4x if you have apocalypse)
                int bonusActivations = _character.HaveTrait(myTraitList[3]) ? 1 : 0;
                Duality(ref _character,ref _castedCard, Enums.CardClass.Warrior, 
                    Enums.CardClass.Healer, _trait, bonusActivations : bonusActivations);
            }
            else if(_trait == myTraitList[1])
            {
                // At the start of your turn, gain 2 of Poison, Burn, or Dark.
                // Gain a percent bonus to damage and healing equal to the total number of
                // curse stacks on this hero.
                string[] curseOptions = { "poison", "burn", "dark" };
                string curseChoice = curseOptions[SafeRandomInt(0, curseOptions.Length - 1)];
                ApplyAuraCurseToTarget(curseChoice, 2, _character, _character, true);

                // (Second part handled in GetTraitDamagePercentModifiersPostfix
                // and GetTraitHealPercentBonusPostfix)
            }
            else if(_trait == myTraitList[2])
            {
                // Thorns on this hero no longer decrease at the end of the turn.
                // When a monster applies a curse on you, heal and gain thorns equal to the number of charges.
                // These amounts do not gain bonuses.
                // (First part handled in GlobalAuraCurseModificationByTraitsAndItemsPostfix)
                // (Second part handled in SetAuraPostfix)
            }
            else if(_trait == myTraitList[3])
            {
                // Dark +1, Burn +1. Dark lowers fire resist by 0.5% per stack,
                // Burn lowers shadow resist by 0.5% per stack.
                // Healer duality can activate four times per turn.
                // (Handled in GlobalAuraCurseModificationByTraitsAndItemsPostfix)
            }
            else if(_trait == myTraitList[4])
            {
                // Thorns +1, Poison +1. When you are damaged by a monster while you have Thorns,
                // apply 1 Poison and 1 Decay to that monster.
                // (4 times/round)
                if(!CanIncrementTraitActivations(_trait)) return;
                if(_target == _character) return;
                if(!IsLivingNPC(_target)) return;

                ApplyAuraCurseToTarget("poison", 1, _target, _character, true);
                ApplyAuraCurseToTarget("decay", 1, _target, _character, true);
                IncrementTraitActivations(_trait);
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

        // Based on code by BinBin
        // https://github.com/binbinmods/Rosalinde/blob/master/RosalindeTraits/Traits.cs

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, 
            ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            //LogDebug($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;

            switch (_acId)
            {
                // Dark +1, Burn +1. Dark lowers monsters' fire resist by 0.5% per stack,
                case "dark":
                    traitOfInterest = myTraitList[3];
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        LogDebug($"Trait {traitOfInterest} - GACM");
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Fire, 0, -0.5f);
                    }
                    break;

                // Burn lowers monsters' shadow resist by 0.5% per stack.
                case "burn":
                    traitOfInterest = myTraitList[3];
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        LogDebug($"Trait {traitOfInterest} - GACM");
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Shadow, 0, -0.5f);
                    }
                    break;

                case "thorns":
                    // Thorns +1. Thorns on this hero no longer decrease at the end of the turn.
                    traitOfInterest = myTraitList[2];
                    if(IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        LogDebug($"Trait {traitOfInterest} - GACM");
                        __result.ConsumedAtTurn = false;
                        __result.AuraConsumed = 0;
                    }
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetAura))]
        public static void SetAuraPostfix(ref Character __instance, ref int __result, Character theCaster, AuraCurseData _acData, int charges, bool fromTrait, Enums.CardClass CC, bool useCharacterMods, bool canBePreventable)
        {
            //LogDebug($"SetAura {subclassName}");

            if (!_acData.IsAura && IsLivingHero(__instance) && IsLivingNPC(theCaster))
            {
                // When a monster applies a curse on you, gain thorns equal to the number of charges
                // and heal yourself for twice that amount.
                // These amounts do not gain bonuses.
                string traitOfInterest = myTraitList[2];
                if (__instance.SubclassName == subclassname && AtOManager.Instance.CharacterHaveTrait(__instance.SubclassName, traitOfInterest))
                {
                    //LogDebug($"{theCaster.GameName} applies {charges} {_acData.ACName} to {__instance.GameName}");
                    LogDebug($"{subclassName} - {traitOfInterest}: {charges} thorns and {2 * charges} heal");
                    ApplyAuraCurseToTarget("thorns", charges, __instance, __instance, false);
                    TraitHeal(ref __instance, __instance, 2 * charges, traitOfInterest);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitDamagePercentModifiers))]
        public static void GetTraitDamagePercentModifiersPostfix(ref Character __instance, ref float __result, Enums.DamageType DamageType)
        {
            //LogDebug("GetTraitDamagePercentModifiersPostfix");
            GetMaelfasTraitPercentBonus(ref __instance, ref __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitHealPercentBonus))]
        public static void GetTraitHealPercentBonusPostfix(ref Character __instance, ref float __result)
        {
            //LogDebug("GetTraitHealPercentBonusPostfix");
            GetMaelfasTraitPercentBonus(ref __instance, ref __result);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitHealReceivedPercentBonus))]
        public static void GetTraitHealReceivedPercentBonusPostfix(ref Character __instance, ref float __result)
        {
            //LogDebug("GetTraitHealReceivedPercentBonusPostfix");
            GetMaelfasTraitPercentBonus(ref __instance, ref __result);
        }

        private static void GetMaelfasTraitPercentBonus(ref Character __instance, ref float __result)
        {
            if (isDamagePreviewActive || isCalculateDamageActive)
                return;

            if (IsLivingHero(__instance) && AtOManager.Instance != null 
                && AtOManager.Instance.CharacterHaveTrait(__instance.SubclassName, myTraitList[1]) 
                && MatchManager.Instance != null)
            {
                LogDebug("GetTraitPercentBonus - post conditional");
                if (__instance.GetCurseList() == null)
                {
                    LogDebug("Empty CurseList");
                    return;
                }

                // You gain a percent bonus to all damage and healing based on the amount of curse
                // charges you have: 6% for each unique curse, plus an addition 6% for every 6 charges
                // you have of each of those curses.
                int percentIncrease = 0;
                for(int i = 0; i < __instance.AuraList.Count; i++)
                {
                    if(__instance.AuraList[i] == null) continue;

                    Aura aura = __instance.AuraList[i];

                    if(aura.ACData.IsAura) continue;

                    percentIncrease += 6 * (aura.GetCharges() / 6 + 1);
                }

                LogDebug("GetTraitPercentBonus - curseStacks = " + percentIncrease);
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
