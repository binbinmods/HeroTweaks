using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using static HeroTweaks.CustomFunctions;
// using Obeliskial_Essentials;
using System.IO;
using static UnityEngine.Mathf;
using UnityEngine.TextCore.LowLevel;
using static HeroTweaks.Plugin;
using System.Collections.ObjectModel;
using UnityEngine;
using System.Text;

namespace HeroTweaks
{
    public class HeroTweaksFunctions
    {

        public static List<string> GetModifiedTraits()
        {
            List<string> modifiedTraits = new List<string>();

            // Get the names of all files in the trait folder (less the .json extension)
            return modifiedTraits;
        }
        public static string SpriteText(string sprite)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string text = sprite.ToLower().Replace(" ", "");
            switch (text)
            {
                case "block":
                case "card":
                    stringBuilder.Append("<space=.2>");
                    break;
                case "piercing":
                    stringBuilder.Append("<space=.4>");
                    break;
                case "bleed":
                    stringBuilder.Append("<space=.1>");
                    break;
                case "bless":
                    stringBuilder.Append("<space=.1>");
                    break;
                default:
                    stringBuilder.Append("<space=.3>");
                    break;
            }
            stringBuilder.Append(" <space=-.2>");
            stringBuilder.Append("<size=+.1><sprite name=");
            stringBuilder.Append(text);
            stringBuilder.Append("></size>");
            switch (text)
            {
                case "bleed":
                    stringBuilder.Append("<space=-.4>");
                    break;
                case "card":
                    stringBuilder.Append("<space=-.2>");
                    break;
                case "powerful":
                case "fury":
                    stringBuilder.Append("<space=-.1>");
                    break;
                default:
                    stringBuilder.Append("<space=-.2>");
                    break;
                case "reinforce":
                case "fire":
                    break;
            }
            return stringBuilder.ToString();
        }

        public static void UpdateMaxMadnessChargesByItem(ref AuraCurseData __result, Character characterOfInterest, string itemID)
        {
            if (__result == null)
            {
                LogDebug("null AuraCurse");
                return;
            }
            // if(itemID == "ringoffire")
            // {
            //     LogDebug("UpdateChargesByItem: " + itemID );
            //     LogDebug($"Team have: {itemID} {AtOManager.Instance.TeamHaveItem(itemID)} ");
            //     LogDebug($"Character have: {itemID} {IfCharacterHas(characterOfInterest, CharacterHas.Item, itemID, AppliesTo.Global)} ");
            // }


            AppliesTo appliesTo = __result.IsAura ? AppliesTo.Heroes : AppliesTo.Monsters;

            if (IfCharacterHas(characterOfInterest, CharacterHas.Item, itemID + "rare", appliesTo))
            {
                LogDebug("UpdateChargesByItem: " + itemID + "rare");
                ItemData itemData = Globals.Instance.GetItemData(itemID + "rare");
                if (itemData == null)
                    return;

                if (__result.MaxCharges != -1)
                {
                    __result.MaxCharges += itemData.AuracurseCustomModValue1;
                }
                if (__result.MaxMadnessCharges != -1)
                {
                    __result.MaxMadnessCharges += itemData.AuracurseCustomModValue1;
                }
            }
            else if (IfCharacterHas(characterOfInterest, CharacterHas.Item, itemID, appliesTo))
            {
                LogDebug("UpdateChargesByItem: " + itemID);
                ItemData itemData = Globals.Instance.GetItemData(itemID);
                if (itemData == null)
                    return;

                if (__result.MaxCharges != -1)
                {
                    __result.MaxCharges += itemData.AuracurseCustomModValue1;
                }
                if (__result.MaxMadnessCharges != -1)
                {
                    __result.MaxMadnessCharges += itemData.AuracurseCustomModValue1;
                }

                LogDebug($"UpdateChargesByItem: {itemID} - post update max charges {__result.MaxMadnessCharges}");

            }

        }

        public static bool IsSpiderCard(CardData card)
        {
            return card.Id.StartsWith("spider") || card.Id.StartsWith("mentalsc") || card.Id.StartsWith("templelur") || card.Id.StartsWith("hatch");
        }

        public static void binbinenergyspike(Enums.EventActivation theEvent, Character character, Character target, int auxInt, string auxString, CardData castedCard, string trait)
        {
            if (theEvent == Enums.EventActivation.Damage && IsLivingNPC(target) && IsLivingHero(character) && character.HaveTrait("binbinenergyspike"))
            {
                int burn = target.GetAuraCharges("burn");
                int shadowDamage = Mathf.RoundToInt(burn * 0.15f);
                int bleed = target.GetAuraCharges("bleed");
                int fireDamage = Mathf.RoundToInt(bleed * 0.15f);

                CharacterItem characterItem = GameUtils.GetCharacterItem(target);

                CastResolutionForCombatText cast = new CastResolutionForCombatText
                {
                    damage = shadowDamage,
                    damageType = Enums.DamageType.Shadow
                };
                target.IndirectDamage(Enums.DamageType.Shadow, shadowDamage, character);
                characterItem.ScrollCombatTextDamageNew(cast);

                CastResolutionForCombatText cast2 = new CastResolutionForCombatText
                {
                    damage = fireDamage,
                    damageType = Enums.DamageType.Fire
                };
                target.IndirectDamage(Enums.DamageType.Fire, fireDamage, character);
                characterItem.ScrollCombatTextDamageNew(cast2);


                characterItem.ScrollCombatText(Texts.Instance.GetText("traits_energyspike"), Enums.CombatScrollEffectType.Trait);
                EffectsManager.Instance.PlayEffectAC("hellfire", isHero: true, characterItem.CharImageT, flip: false);
            }
        }

        public static void binbinwebweaver(Character source)
        {
            ApplyAuraCurseToAll("poison", 1, AppliesTo.Monsters, source, useCharacterMods: true);
            ApplyAuraCurseToAll("insane", 1, AppliesTo.Monsters, source, useCharacterMods: true);
            source?.HeroItem?.ScrollCombatText(Texts.Instance.GetText("traits_Webweaver"), Enums.CombatScrollEffectType.Trait);

        }

        public static void binbinmindcollapse(Character source)
        {
            AddCardToHand("binbininducesleep", 1);
            source?.HeroItem?.ScrollCombatText(Texts.Instance.GetText("traits_mindcollapse"), Enums.CombatScrollEffectType.Trait);

        }

        public static void binbinrevealingpresence(Character source)
        {
            Character randomNPC = GetRandomCharacter(MatchManager.Instance.GetTeamNPC());
            if (randomNPC != null)
            {
                randomNPC.SetAuraTrait(source, "revealed", 1);
                source?.HeroItem?.ScrollCombatText(Texts.Instance.GetText("traits_revealingpresence"), Enums.CombatScrollEffectType.Trait);
            }
        }

    }
}

