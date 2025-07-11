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
            "shamanpackleader"
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
                // When you play a Mage card, reduce the cost of the highest cost Healer card
                // in your hand by 1 until discarded. When you play a Healer card, reduce the
                // cost of the highest cost Mage card in your hand by 1 until discarded. (3 times/turn)
                // (4x if you have ???)
                int bonusActivations = _character.HaveTrait(myTraitList[3]) ? 1 : 0;
                Duality(ref _character,ref _castedCard, Enums.CardClass.Healer, 
                    Enums.CardClass.Mage, _trait, bonusActivations : bonusActivations);
            }
            else if(_trait == myTraitList[1])
            {
                // Upon picking up, if you have the "Astral Wolf" pet, corrupt it.
                if(IfCharacterHas(_character, CharacterHas.Item, "shamanhound", AppliesTo.ThisHero)
                    || IfCharacterHas(_character, CharacterHas.Item, "shamanhounda", AppliesTo.ThisHero)
                    || IfCharacterHas(_character, CharacterHas.Item, "shamanhoundb", AppliesTo.ThisHero))
                {
                    //_character.HeroItem.pet
                }
            }
            else if(_trait == myTraitList[2])
            {
                
            }
            else if(_trait == myTraitList[3])
            {
                
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
