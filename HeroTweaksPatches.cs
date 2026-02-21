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

    }
}