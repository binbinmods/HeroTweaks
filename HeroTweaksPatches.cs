using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
// using static Obeliskial_Essentials.Essentials;
using System;
using static HeroTweaks.Plugin;
using static HeroTweaks.CustomFunctions;
using static HeroTweaks.HeroTweaksFunctions;
using System.Collections.Generic;
using static Functions;
using UnityEngine;
// using Photon.Pun;
using TMPro;
using System.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using System.Text;
// using Unity.TextMeshPro;

// Make sure your namespace is the same everywhere
namespace HeroTweaks
{

    [HarmonyPatch] // DO NOT REMOVE/CHANGE - This tells your plugin that this is part of the mod

    public class HeroTweaksPatches
    {
        public static bool devMode = false; //DevMode.Value;
        public static bool bSelectingPerk = false;
        public static bool IsHost()
        {
            return GameManager.Instance.IsMultiplayer() && NetworkManager.Instance.IsMaster();
        }




        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetEvent))]
        public static void SetEventPostfix(
            Character __instance,
            Enums.EventActivation theEvent,
            Character target = null,
            int auxInt = 0,
            string auxString = "")
        {
            string traitOfInterest = "broodmother";
            if (theEvent == Enums.EventActivation.Killed && AtOManager.Instance.TeamHaveTrait(traitOfInterest) && IsLivingHero(__instance))
            {

                __instance.DoTraitFunction(traitOfInterest);
            }

            traitOfInterest = "spiderqueen";
            if (theEvent == Enums.EventActivation.BeginRound && MatchManager.Instance.GameRound() >= 2 && __instance.HaveTrait(traitOfInterest) && IsLivingHero(__instance))
            {
                __instance.DoTraitFunction("broodmother");
            }

            traitOfInterest = "loadedgun";
            CardData castedCard = __instance.CardCasted;
            if (theEvent == Enums.EventActivation.FinishCast && castedCard.HasCardType(Enums.CardType.Ranged_Attack) && __instance.HaveTrait(traitOfInterest) && IsLivingHero(__instance))
            {

                __instance.SetAuraTrait(__instance, "burn", 1);
            }

            traitOfInterest = "webweaver";
            if (theEvent == Enums.EventActivation.FinishCast && IsSpiderCard(castedCard) && __instance.HaveTrait(traitOfInterest) && IsLivingHero(__instance))
            {
                binbinwebweaver(__instance);
            }

            traitOfInterest = "binbinmindcollapse";
            if (theEvent == Enums.EventActivation.FinishCast && __instance.HaveTrait(traitOfInterest) && IsLivingHero(__instance) && castedCard != null && MatchManager.Instance.energyJustWastedByHero >= 3)
            {
                binbinmindcollapse(__instance);
            }

            traitOfInterest = "binbinrevealingpresence";
            if (theEvent == Enums.EventActivation.FinishCast && __instance.HaveTrait(traitOfInterest) && IsLivingHero(__instance) && castedCard != null && MatchManager.Instance.energyJustWastedByHero >= 3)
            {
                binbinrevealingpresence(__instance);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.DoTrait))]
        public static bool DoTraitPrefix(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard)
        {

            if (_trait == "binbinenergyspike")
            {
                binbinenergyspike(_theEvent, _character, _target, _auxInt, _auxString, _castedCard, _trait);
                return false;
            }
            return true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.blessed))]
        public static bool blessed(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            character.SetAura(character, Globals.Instance.GetAuraCurseData("bless"), 2, fromTrait: true, useCharacterMods: false);
            if (character.HeroItem != null)
            {
                character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Blessed"), Enums.CombatScrollEffectType.Trait);
                EffectsManager.Instance.PlayEffectAC("bless", isHero: true, character.HeroItem.CharImageT, flip: false);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.butcher))]
        public static bool butcher(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            if (target == null || target.IsHero || !MatchManager.Instance.AnyNPCAlive())
            {
                return false;
            }
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            for (int i = 0; i < teamHero.Length; i++)
            {
                if (teamHero[i] == null || !(teamHero[i].HeroData != null) || !teamHero[i].Alive)
                {
                    continue;
                }
                if (!character.HaveTrait("threestarchef"))
                {
                    switch (MatchManager.Instance.GetRandomIntRange(0, 3, "trait"))
                    {
                        case 0:
                            {
                                string text3 = MatchManager.Instance.CreateCardInDictionary("premiummeat");
                                MatchManager.Instance.GetCardData(text3);
                                MatchManager.Instance.GenerateNewCard(1, text3, createCard: false, Enums.CardPlace.RandomDeck, null, null, teamHero[i].HeroIndex);
                                break;
                            }
                        case 1:
                            {
                                string text2 = MatchManager.Instance.CreateCardInDictionary("meat");
                                MatchManager.Instance.GetCardData(text2);
                                MatchManager.Instance.GenerateNewCard(1, text2, createCard: false, Enums.CardPlace.RandomDeck, null, null, teamHero[i].HeroIndex);
                                break;
                            }
                        default:
                            {
                                string text = MatchManager.Instance.CreateCardInDictionary("spoiledmeat");
                                MatchManager.Instance.GetCardData(text);
                                MatchManager.Instance.GenerateNewCard(1, text, createCard: false, Enums.CardPlace.RandomDeck, null, null, teamHero[i].HeroIndex);
                                break;
                            }
                    }
                }
                else
                {
                    switch (MatchManager.Instance.GetRandomIntRange(0, 3, "trait"))
                    {
                        case 0:
                            {
                                string text6 = MatchManager.Instance.CreateCardInDictionary("premiummeat");
                                MatchManager.Instance.GetCardData(text6);
                                MatchManager.Instance.GenerateNewCard(1, text6, createCard: false, Enums.CardPlace.RandomDeck, null, null, teamHero[i].HeroIndex);
                                break;
                            }
                        case 1:
                            {
                                string text5 = MatchManager.Instance.CreateCardInDictionary("meat");
                                MatchManager.Instance.GetCardData(text5);
                                MatchManager.Instance.GenerateNewCard(1, text5, createCard: false, Enums.CardPlace.RandomDeck, null, null, teamHero[i].HeroIndex);
                                break;
                            }
                        default:
                            {
                                string text4 = MatchManager.Instance.CreateCardInDictionary("gourmetmeat");
                                MatchManager.Instance.GetCardData(text4);
                                MatchManager.Instance.GenerateNewCard(1, text4, createCard: false, Enums.CardPlace.RandomDeck, null, null, teamHero[i].HeroIndex);
                                break;
                            }
                    }
                }
            }
            character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Butcher"), Enums.CombatScrollEffectType.Trait);
            MatchManager.Instance.ItemTraitActivated();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.countermeasures))]
        public static bool countermeasures(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            if (!(MatchManager.Instance != null))
            {
                return false;
            }
            TraitData traitData = Globals.Instance.GetTraitData("countermeasures");
            if (character != null && character.Alive && character.HeroItem != null)
            {
                // if (!MatchManager.Instance.activatedTraitsRound.ContainsKey("countermeasures"))
                // {
                //     MatchManager.Instance.activatedTraitsRound.Add("countermeasures", 1);
                // }
                // else
                // {
                //     MatchManager.Instance.activatedTraitsRound["countermeasures"]++;
                // }
                MatchManager.Instance.SetTraitInfoText();
                character.SetAuraTrait(character, "thorns", 2);
                character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Countermeasures") + Functions.TextChargesLeft(MatchManager.Instance.activatedTraitsRound["countermeasures"], traitData.TimesPerRound), Enums.CombatScrollEffectType.Trait);
                EffectsManager.Instance.PlayEffectAC("thorns", isHero: true, character.HeroItem.CharImageT, flip: false);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.crimsonripple))]

        public static bool crimsonripple(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            if (!(auxString.ToLower() == "bleed") || target.IsHero)
            {
                return false;
            }
            if (CanIncrementTraitActivations("crimsonripple") && castedCard != null && castedCard.CardType == Enums.CardType.Skill)
            {
                ApplyAuraCurseToAll("bleed", 2, AppliesTo.Monsters, character, useCharacterMods: true);
                MatchManager.Instance.SetTraitInfoText();
                IncrementTraitActivations("crimsonripple");
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.darkmercy))]
        public static bool darkmercy(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {

            bool flag = ((target is Hero || (target != null && !target.Alive)) ? true : false);
            if (!flag && character != null && character.Alive && auxString.Equals("dark", StringComparison.OrdinalIgnoreCase))
            {
                Hero lowestHealthHero = GetLowestHealthHero(theEvent, character, target, auxInt, auxString, castedCard, trait);
                if (lowestHealthHero != null && lowestHealthHero.Alive)
                {
                    TraitHealHero(ref character, ref lowestHealthHero, auxInt * 2, trait);
                }
                GameUtils.GetCharacterItem(target).ScrollCombatText(Texts.Instance.GetText("traits_darkmercy"), Enums.CombatScrollEffectType.Trait);
            }
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.envenom))]
        public static bool envenom(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            if (target != null && target.Alive)
            {
                target.SetAuraTrait(character, "poison", 1);
                character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Envenom"), Enums.CombatScrollEffectType.Trait);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.marksmanship))]
        public static bool marksmanship(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            if (MatchManager.Instance != null && castedCard != null && castedCard.GetCardTypes().Contains(Enums.CardType.Ranged_Attack))
            {
                character.SetAuraTrait(character, "sharp", 3);
                if (character.HeroItem != null)
                {
                    character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Marksmanship"), Enums.CombatScrollEffectType.Trait);
                    EffectsManager.Instance.PlayEffectAC("sharp", isHero: true, character.HeroItem.CharImageT, flip: false);
                }
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.spellsinger))]
        public static bool spellsinger(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            if (!(MatchManager.Instance != null) || !(castedCard != null))
            {
                return false;
            }
            int num = Globals.Instance.GetTraitData("spellsinger").TimesPerTurn - 1;
            if (character.HaveTrait("spellsinger") && character.HaveTrait("ragnarok"))
            {
                num += 2;
            }
            if ((MatchManager.Instance.activatedTraits != null && MatchManager.Instance.activatedTraits.ContainsKey("spellsinger") && MatchManager.Instance.activatedTraits["spellsinger"] > num) || !castedCard.GetCardTypes().Contains(Enums.CardType.Song))
            {
                return false;
            }
            List<string> list = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Enum.GetName(typeof(Enums.HeroClass), Enums.HeroClass.Mage));
            stringBuilder.Append("_");
            stringBuilder.Append(Enum.GetName(typeof(Enums.CardType), Enums.CardType.Fire_Spell));
            for (int i = 0; i < Globals.Instance.CardListByClassType[stringBuilder.ToString()].Count; i++)
            {
                list.Add(Globals.Instance.CardListByClassType[stringBuilder.ToString()][i]);
            }
            stringBuilder.Clear();
            stringBuilder.Append(Enum.GetName(typeof(Enums.HeroClass), Enums.HeroClass.Mage));
            stringBuilder.Append("_");
            stringBuilder.Append(Enum.GetName(typeof(Enums.CardType), Enums.CardType.Lightning_Spell));
            for (int j = 0; j < Globals.Instance.CardListByClassType[stringBuilder.ToString()].Count; j++)
            {
                list.Add(Globals.Instance.CardListByClassType[stringBuilder.ToString()][j]);
            }
            stringBuilder.Clear();
            stringBuilder.Append(Enum.GetName(typeof(Enums.HeroClass), Enums.HeroClass.Mage));
            stringBuilder.Append("_");
            stringBuilder.Append(Enum.GetName(typeof(Enums.CardType), Enums.CardType.Cold_Spell));
            for (int k = 0; k < Globals.Instance.CardListByClassType[stringBuilder.ToString()].Count; k++)
            {
                list.Add(Globals.Instance.CardListByClassType[stringBuilder.ToString()][k]);
            }
            int num2 = MatchManager.Instance.energyJustWastedByHero;
            if (character.HaveTrait("spellsinger") || character.HaveTrait("ragnarok"))
            {
                if (character.EffectCharges("stanzai") > 0)
                {
                    num2++;
                }
                else if (character.EffectCharges("stanzaii") > 0)
                {
                    num2 += 2;
                }
                else if (character.EffectCharges("stanzaiii") > 0)
                {
                    num2 += 3;
                }
            }
            if (num2 > 10)
            {
                num2 = 10;
            }
            bool flag = false;
            string id = "";
            int num3 = 0;
            while (!flag && num3 < 500)
            {
                int randomIntRange = MatchManager.Instance.GetRandomIntRange(0, list.Count, "trait");
                id = list[randomIntRange];
                if (Globals.Instance.GetCardData(id, instantiate: false).EnergyCostOriginal == num2)
                {
                    flag = true;
                    break;
                }
                num3++;
            }
            string text = MatchManager.Instance.CreateCardInDictionary(id);
            CardData cardData = MatchManager.Instance.GetCardData(text);
            cardData.Vanish = true;
            cardData.EnergyReductionToZeroPermanent = true;
            MatchManager.Instance.GenerateNewCard(1, text, createCard: false, Enums.CardPlace.Hand);
            if (character.HeroItem != null)
            {
                if (!MatchManager.Instance.activatedTraits.ContainsKey("spellsinger"))
                {
                    MatchManager.Instance.activatedTraits.Add("spellsinger", 1);
                }
                else
                {
                    MatchManager.Instance.activatedTraits["spellsinger"]++;
                }
                MatchManager.Instance.SetTraitInfoText();
                character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Spell Singer") + Functions.TextChargesLeft(MatchManager.Instance.activatedTraits["spellsinger"], num + 1), Enums.CombatScrollEffectType.Trait);
            }
            MatchManager.Instance.ItemTraitActivated();
            MatchManager.Instance.CreateLogCardModification(cardData.InternalId, MatchManager.Instance.GetHero(character.HeroIndex));
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.temperate))]
        public static bool temperate(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {

            if (character.EffectCharges("Insulate") > 1)
            {
                character.SetAuraTrait(character, "infuse", 1);
            }
            else
            {
                character.SetAuraTrait(character, "insulate", 1);
            }
            if (character.HeroItem != null)
            {
                character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Temperate"), Enums.CombatScrollEffectType.Trait);
                EffectsManager.Instance.PlayEffectAC("insulate", isHero: true, character.HeroItem.CharImageT, flip: false);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.versatile))]
        public static bool versatile(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            if (MatchManager.Instance != null && castedCard != null)
            {
                bool flag = false;
                _ = castedCard.InternalId;
                if (castedCard.GetCardTypes().Contains(Enums.CardType.Fire_Spell))
                {
                    flag = true;
                    character.SetAuraTrait(character, "powerful", 1);
                    character.SetAuraTrait(character, "furnacelightning", 1);
                }
                if (castedCard.GetCardTypes().Contains(Enums.CardType.Cold_Spell))
                {
                    flag = true;
                    character.SetAuraTrait(character, "block", 6);
                    character.SetAuraTrait(character, "furnace", 1);
                }
                if (castedCard.GetCardTypes().Contains(Enums.CardType.Lightning_Spell))
                {
                    flag = true;
                    character.HealCurses(1);
                    character.SetAuraTrait(character, "furnacecold", 1);
                }
                if (flag)
                {
                    character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Versatile"), Enums.CombatScrollEffectType.Trait);
                }
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.warmaiden))]
        public static bool warmaiden(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            if (character != null && character.Alive)
            {
                int num = character.EffectCharges("powerful");
                if (num > 0)
                {
                    character.SetAuraTrait(character, "stanzai", 1);
                }
                else
                {
                    character.SetAuraTrait(character, "powerful", 2);
                }
                if (character.HeroItem != null)
                {
                    character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Warmaiden"), Enums.CombatScrollEffectType.Trait);
                    EffectsManager.Instance.PlayEffectAC("powerful", isHero: true, character.HeroItem.CharImageT, flip: false);
                }
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), nameof(Trait.webweaver))]
        public static bool webweaver(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            if (!(MatchManager.Instance != null) || MatchManager.Instance.GetCurrentRound() != 1)
            {
                return false;
            }
            NPC[] teamNPC = MatchManager.Instance.GetTeamNPC();
            for (int i = 0; i < teamNPC.Length; i++)
            {
                if (teamNPC[i] != null && teamNPC[i].Alive)
                {
                    // teamNPC[i].SetAuraTrait(character, "insane", 6);
                    // teamNPC[i].SetAuraTrait(character, "poison", 6);
                    teamNPC[i].SetAuraTrait(character, "shackle", 1);
                    EffectsManager.Instance.PlayEffectAC("poisonneuro", isHero: true, teamNPC[i].NPCItem.CharImageT, flip: false);
                }
            }
            character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_Webweaver"), Enums.CombatScrollEffectType.Trait);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            switch (_acId)
            {
                // trait2a:

                // trait2b:

                // trait 4a;

                // trait 4b:

                case "zealotry":
                    traitOfInterest = "zealotry";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, AppliesTo.ThisHero))
                    {
                        __result.AuraDamageIncreasedPercentPerStack = 3.0f;
                    }
                    break;

            }
        }



    }
}