using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using static Janus.CustomFunctions;
using static Janus.Plugin;
using UnityEngine;

namespace Janus
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs
        public static string[] myTraitList = 
        {
            "dualistemissary",
            "dualistduality",
            "dualistsacrilege",
            "dualistavatarlight",
            "dualistavatarshadow"
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
								// When you apply "Sanctify" charges, heal the most damaged hero for that amount.
                // When you apply "Dark" charges, apply that amount of "Shield" charges to the most damaged hero.
                // -These amounts do not gain bonuses-"
                Character mostDamagedHero = GetLowestHealthCharacter(teamHero);
                // Just in case!
                if(mostDamagedHero == null) return;

                if(_auxString == "sanctify")
                {
                    TraitHeal(ref _character, mostDamagedHero, _auxInt, _trait);
                }
                else if(_auxString == "dark")
                {
                    ApplyAuraCurseToTarget("shield", _auxInt, mostDamagedHero, _character, false);
                }
            }
            else if(_trait == myTraitList[1])
            {
                // When you play a Healer card, reduce the cost of the highest cost Warrior card in your hand by 1 until discarded.
                // When you play a Warrior card, reduce the cost of the highest cost Healer card in your hand by 1 until discarded.
                // (3 times/turn) [4x if you have Avatar of Light]
                int bonusActivations = _character.HaveTrait(myTraitList[3]) ? 1 : 0;
                Duality(ref _character,ref _castedCard, Enums.CardClass.Warrior, 
                    Enums.CardClass.Healer, _trait, bonusActivations : bonusActivations);
            }
            else if(_trait == myTraitList[2])
            {
                // At the start of your turn, transform all "Bless" charges on monsters to "Sanctify" and "Dark" charges.
                foreach(NPC monster in teamNpc)
                {
                    if(!IsLivingNPC(monster)) continue;

                    int blessStacks = monster.GetAuraCharges("bless");
                    if(blessStacks < 1) continue;

                    ApplyAuraCurseToTarget("bless", -blessStacks, monster, _character, false);
                    ApplyAuraCurseToTarget("sanctify", blessStacks, monster, _character, false);
                    ApplyAuraCurseToTarget("dark", blessStacks, monster, _character, false);
                }
            }
            else if(_trait == myTraitList[3])
            {
                // Sanctify +1, Zeal +1, Burn +1. Heal done +30%, heal received +15%. Healer duality can be activated 4 times per turn.
                // (Handled above)
            }
            else if(_trait == myTraitList[4])
            {
                // Dark +1, Scourge +1, Chill +1. All damage done +30%. At the start of your turn,
                // reduce the cost of the "Attack" cards in your hand by 1 until they are discarded.
                ReduceCardTypeCostUntilDiscarded(Enums.CardType.Attack, 1, ref _character, ref heroHand, ref cardDataList, _trait);
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
    }
}
