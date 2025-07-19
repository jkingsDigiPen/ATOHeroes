using HarmonyLib;
using Obeliskial_Content;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static Mesmer.CustomFunctions;
using static Mesmer.Plugin;

namespace Mesmer
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs
        public static string[] myTraitList = 
        {
            "keeperaccelerate",
            "keeperrestart",
            "keeperinstability",
            "keepertimelord"
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
                // At the start of combat, shuffle 1 Accelerate card into each hero deck.
                string cardID = myTraitList[0];
                for (int i = 0; i < teamHero.Length; i++)
                {
                    if (_character.HaveTrait(myTraitList[1]))
                    {
                        cardID = ((!_character.HaveTrait(myTraitList[3])) ? myTraitList[1] : myTraitList[1] + "rare");
                        break;
                    }

                    if (_character.HaveTrait(myTraitList[2]))
                    {
                        cardID = ((!_character.HaveTrait(myTraitList[3])) ? myTraitList[2] : myTraitList[2] + "rare");
                        break;
                    }
                }

                for (int j = 0; j < teamHero.Length; j++)
                {
                    if (teamHero[j] != null && teamHero[j].HeroData != null && teamHero[j].Alive)
                    {
                        string text = MatchManager.Instance.CreateCardInDictionary(cardID);
                        MatchManager.Instance.GetCardData(text);
                        MatchManager.Instance.GenerateNewCard(1, text, createCard: false, Enums.CardPlace.RandomDeck, null, null, teamHero[j].HeroIndex);
                    }
                }

                _character.HeroItem.ScrollCombatText("Chronomancer", Enums.CombatScrollEffectType.Trait);
                MatchManager.Instance.ItemTraitActivated();
            }
            else if(_trait == myTraitList[1])
            {
               // Replace the Accelerate cards of Chronomancer for Eldritch Restart cards. (handled above)
            }
            else if(_trait == myTraitList[2])
            {
                // Replace the Accelerate cards of Chronomancer for Instability cards. (handled above)
            }
            else if(_trait == myTraitList[3])
            {
                 // Upgrade Eldritch Restart cards and Instability cards into their corrupted versions. (handled above)
            }
            else if(_trait == myTraitList[4])
            {
                
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
