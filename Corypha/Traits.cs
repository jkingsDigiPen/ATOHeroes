using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using static Corypha.CustomFunctions;
using static Corypha.Plugin;

namespace Corypha
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs
        public static string[] myTraitList = 
        { 
            "sirencallofthedeep", 
            "sirenpredator", 
            "sirensonar",
            "sirenduality",
            "sirenbloodwater"
        };

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
                // Wet +1, Chill +1. Wet on enemies also reduces their Mind resistance by 1% per charge.
                // (Handled in GlobalAuraCurseModificationByTraitsAndItemsPostFix)
            }
            else if(_trait == myTraitList[1])
            {
                // Wet on this hero does not lose charges at the end of the turn.
                // At the end of your turn, suffer 1 Wet and if you have at least
                // 4 Wet charges, gain 1 Stealth.
                ApplyAuraCurseToTarget("wet", 1, _character, _character, true);

                if(_character.GetAuraCharges("wet") >= 4)
                {
                    ApplyAuraCurseToTarget("stealth", 1, _character, _character, true);
                }
            }
            else if(_trait == myTraitList[2])
            {
                // Mark +1, Insane +1. When you cast a Song spell, gain 1 Evasion
                // and apply 1 Mark on all enemies (3 times/turn).
                // Mage duality can be activated 4 times per turn.

                if(!CanIncrementTraitActivations(_trait)) return;

                if(!_castedCard.HasCardType(Enums.CardType.Song)) return;

                ApplyAuraCurseToTarget("evasion", 1, _character, _character, true);
                ApplyAuraCurseToAll("mark", 1, AppliesTo.Monsters, _character, true);

                IncrementTraitActivations(_trait);
            }
            else if(_trait == myTraitList[3])
            {
                // When you play a Mage card, reduce the cost of the highest cost Scout card
                // in your hand by 1 until discarded. When you play a Scout card, reduce the
                // cost of the highest cost Mage card in your hand by 1 until discarded. (3 times/turn)
                // (4x if you have sonar)
                int bonusActivations = _character.HaveTrait(myTraitList[2]) ? 1 : 0;
                Duality(ref _character,ref _castedCard, Enums.CardClass.Scout, 
                    Enums.CardClass.Mage, _trait, bonusActivations : bonusActivations);
            }
            else if(_trait == myTraitList[4])
            {
                // Bleed +1, Dark +1. At the start of your turn, apply 1 Bleed and 1 Dark on all monsters
                // for each Wet charge on them. -The amounts applied do not benefit from bonuses.-
                int chargesToApply = 0;

                foreach(NPC enemy in teamNpc)
                {
                    if(!IsLivingNPC(enemy)) continue;
                    chargesToApply = enemy.GetAuraCharges("wet");
                    ApplyAuraCurseToTarget("bleed", chargesToApply, _character, enemy, false);
                    ApplyAuraCurseToTarget("dark", chargesToApply, _character, enemy, false);
                }
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
                case "wet":
                    // Wet +1, Chill +1. Wet on enemies also reduces their Mind resistance by 1% per charge.
                    traitOfInterest = myTraitList[0];
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.Monsters))
                    {
                        LogDebug($"Trait {traitOfInterest} - GACM");
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Mind, 0, -1.0f);
                    }

                    // Wet on this hero does not lose charges at the end of the turn.
                    // At the end of your turn, suffer 1 Wet and if you have at least
                    // 4 Wet charges, gain 1 Stealth.
                    traitOfInterest = myTraitList[1];
                    if(IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        LogDebug($"Trait {traitOfInterest} - GACM");
                        __result.ConsumedAtTurn = false;
                        __result.AuraConsumed = 0;
                    }
                    break;
            }
        }
    }
}
