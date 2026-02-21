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

namespace HeroTweaks
{
    public class HeroTweaksFunctions
    {


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


    }
}

