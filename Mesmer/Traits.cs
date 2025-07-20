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
            "keepertimelord",
            "keeperillusionist"
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
                string cardID = _trait;
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

                _character.HeroItem.ScrollCombatText("traits_" + _trait, Enums.CombatScrollEffectType.Trait);
                MatchManager.Instance.ItemTraitActivated();
            }
            else if(_trait == myTraitList[1])
            {
                if(!CanIncrementTraitActivations(_trait))
                    return;

                if(_castedCard.CardType != Enums.CardType.Defense)
                    return;

                // Replace the Accelerate cards of Chronomancer for Eldritch Restart cards. (handled above)
                // When you play a "Defense" card that costs Energy, refund 1 Energy and gain 1 Regeneration. (3 times/turn)
                IncrementTraitActivations(_trait);
                WhenYouPlayXGainY(Enums.CardType.Defense, "regeneration", 1, _castedCard, ref _character, _trait);
                _character.ModifyEnergy(1, showScrollCombatText: true);
                EffectsManager.Instance.PlayEffectAC("energy", isHero: true, _character.HeroItem.CharImageT, flip: false);
            }
            else if(_trait == myTraitList[2])
            {
                // Replace the Accelerate cards of Chronomancer for Instability cards. (handled above)
                // At the start of your turn, reduce the cost of the "Mind Spell" cards in your hand by 1 until they are discarded.
                ReduceCardTypeCostUntilDiscarded(Enums.CardType.Mind_Spell, 1, ref _character, ref heroHand, ref cardDataList, _trait);
            }
            else if(_trait == myTraitList[3])
            {
                // Upgrade Eldritch Restart cards and Instability cards into their corrupted versions. (handled above)
                // At the start of your turn, shuffle 1 healer card from your vanish pile into your deck.
                if(MatchManager.Instance.CountHeroVanish() < 1)
                    return;

                List<string> vanished = MatchManager.Instance.GetHeroVanish(_character.HeroIndex);
                Dictionary<int, string> validVanished = new Dictionary<int, string>();

                for(int i = 0; i < vanished.Count; i++)
                {
                    CardData cardData = MatchManager.Instance.GetCardData(vanished[i]);
                    if(cardData.CardClass == Enums.CardClass.Healer)
                        validVanished.Add(i, vanished[i]);
                }

                if(validVanished.Count < 1) return;

                int randomIndexValid = SafeRandomInt(0, validVanished.Count);
                int j = 0;
                int randomIndexVanished = 0;
                foreach(KeyValuePair<int, string> keyValuePair in validVanished)
                {
                    if(j == randomIndexValid)
                    {
                        randomIndexVanished = keyValuePair.Key;
                        break;
                    }
                    j++;
                }

                // Okay, now what?
                int randomIndexDeck = SafeRandomInt(0, MatchManager.Instance.CountHeroDeck());
                MatchManager.Instance.GetHeroDeck(_character.HeroIndex)[_character.HeroIndex].Insert(randomIndexDeck, validVanished[randomIndexVanished]);
                MatchManager.Instance.GetHeroVanish(_character.HeroIndex).RemoveAt(randomIndexVanished);
                //MatchManager.Instance.DrawDeckPile(MatchManager.Instance.CountHeroDeck() + 1);
                _character.HeroItem.ScrollCombatText("traits_" + _trait, Enums.CombatScrollEffectType.Trait);
                MatchManager.Instance.ItemTraitActivated();
            }
            else if(_trait == myTraitList[4])
            {
                // When you play a "Skill" card, gain 1 Inspire and 1 Stealth.
                if(_castedCard.CardType != Enums.CardType.Skill)
                    return;

                WhenYouPlayXGainY(Enums.CardType.Skill, "inspire", 1, _castedCard, ref _character, _trait);
                WhenYouPlayXGainY(Enums.CardType.Skill, "stealth", 1, _castedCard, ref _character, _trait);
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
