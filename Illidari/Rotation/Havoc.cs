using Styx;
using Styx.CommonBot.Coroutines;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Illidari;

#region [Method] - Class Redundancy
using HK = Illidari.Core.Managers.Hotkeys;
using S = Illidari.Core.Spell;
using SB = Illidari.Core.Helpers.Spell_Book;
using U = Illidari.Core.Unit;
using I = Illidari.Core.Item;
using T = Illidari.Core.Managers.TalentManager;
using L = Illidari.Core.Utilities.Log;
using C = Illidari.Core.Helpers.Common;
using M = Illidari.Main;

#endregion

namespace Illidari.Rotation
{
    class Havoc
    {
        private static bool useOpenRotation = false;
        private static int openingRotationSkillsUsed = 0;
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static WoWUnit CurrentTarget { get { return StyxWoW.Me.CurrentTarget; } }
        //private static uint CurrentFury => StyxWoW.Me.GetPowerInfo(WoWPowerType.Fury).Current;

        #region Precombat Buffing
        public static async Task<bool> PreCombatBuffing()
        {
            if (HK.manualOn || Me.Combat || !Me.IsAlive || (Me.OnTaxi))
                return true;
            // check to see if we should use a flask
            if (await I.UseItem(I.FindBestPrecombatFlask(), M.IS.HavocUseAgilityFlask && !Me.HasAura(SB.FlaskList)))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Combat Buffing
        public static async Task<bool> CombatBuffing()
        {
            if (HK.manualOn || !Me.IsAlive || (!Me.GotTarget || !CurrentTarget.IsAlive) || (Me.OnTaxi))
                return true;
            if (!Me.IsAutoAttacking && CurrentTarget.IsValidCombatUnit()) { Lua.DoString("StartAttack()"); return true; }

            if (await I.UseItem(I.GetItemByName("Draenic Agility Potion"),
                M.IS.HavocUseAgilityPotionCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.Cooldown // use it on cooldown
                || (M.IS.HavocUseAgilityPotionCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.BossOnly && Me.CurrentTarget.IsBoss) // use if only is a boss
                || (M.IS.HavocUseAgilityPotionCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.EliteBoss && (Me.CurrentTarget.IsBoss || Me.CurrentTarget.Elite)) // use boss or eltie
            ))
            { return true; }

            return false;
        }

        #endregion

        #region Pull Logic
        public static async Task<bool> Pull()
        {
            if (CurrentTarget.IsValidCombatUnit())
            {
                if (!CurrentTarget.IsWithinMeleeRangeOf(Me) && M.IS.GeneralMovement)
                {
                    //L.infoLog("Tried to pull");
                    await C.EnsureMeleeRange(CurrentTarget);
                }

                if (M.IS.GeneralFacing)
                {
                    // check to see if we need to face target
                    await C.FaceTarget(CurrentTarget);
                }



                // throw a glaive first
                if (await S.Cast(SB.ThrowGlaive, C.CombatColor,
                    CurrentTarget.Distance <= 30))
                { return true; }

                // use to engage
                return await S.GCD(SB.FelRush, C.CombatColor, CurrentTarget.Distance <= 20
                    && M.IS.HavocFelRushOnPull);
            }

            return true;
        }
        #endregion

        #region Combat Rotation

        #region Rotation Selector
        public static async Task<bool> RotationSelector()
        {
            // we are playing manual, i am not alive, or I am mounted or on taxi, do not continue
            if (HK.manualOn || !Me.IsAlive || (Me.OnTaxi))
                return true;
            // if I 


            if (M.IS.GeneralFacing)
            {
                // check to see if we need to face target
                await C.FaceTarget(CurrentTarget);
            }

            if (M.IS.GeneralMovement)
            {
                // ensure we are in melee range and not too close
                await C.EnsureMeleeRange(CurrentTarget);
            }

            if (CurrentTarget.IsValidCombatUnit())
            {
                // see if we need any defensive cooldowns
                if (await SaveMe()) { return true; }

                if ((HK.HavocAoEOn || U.activeEnemies(Me.Location, 8f).Count() >= 3))
                {
                    // try to use aoe abilities
                    if (await AoE()) { return true; }
                    // we were supposed to aoe, but nothing to use for aoe (or jumped back out of melee), so use single target
                    if (await AoEDashing()) { return true; }
                }

                if (await OpeningRotationSingleTarget())
                {
                    return true;
                }

                // default to single target if nothing else
                return await SingleTarget();
            }
            return false;
        }
        #endregion

        #region Save Me
        public static async Task<bool> SaveMe()
        {

            // cache number of active enemies in combat and just use that for use/logging
            int enemiesInMelee = U.activeEnemies(Me.Location, 8f).Count();

            if (await I.UseItem(I.FindBestHealingPotion(), Me.HealthPercent < M.IS.HavocHealthPotionHp))
            {
                return true;
            }

            #region Blur
            if (M.IS.HavocBlurUnits > 0 && M.IS.HavocBlurHp > 0 && M.IS.HavocBlurOperator.ToUpper() == "AND")
            {
                if (await S.Buff(SB.Blur, C.DefensiveColor,
                    Me.HealthPercent < M.IS.HavocBlurHp
                    && enemiesInMelee >= M.IS.HavocBlurUnits,
                    string.Format($"HP:{Me.HealthPercent.ToString("F0")}<{M.IS.HavocBlurHp} AND Units:{enemiesInMelee}>={M.IS.HavocBlurUnits}")))
                { return true; }
            }
            else
            {
                if (await S.Buff(SB.Blur, C.DefensiveColor,
                    M.IS.HavocBlurUnits > 0
                && (enemiesInMelee >= M.IS.HavocBlurUnits),
                 string.Format($"Units:{enemiesInMelee}>={M.IS.HavocBlurUnits}")))
                {
                    return true;
                }
                if (await S.Buff(SB.Blur, C.DefensiveColor,
                    M.IS.HavocBlurHp > 0
                    && Me.HealthPercent < M.IS.HavocBlurHp,
                    string.Format($"HP:{Me.HealthPercent.ToString("F0")}<{M.IS.HavocBlurHp}")))
                {
                    return true;
                }

            }

            #endregion

            #region Darkness
            if (M.IS.HavocDarknessUnits > 0 && M.IS.HavocDarknessHp > 0 && M.IS.HavocDarknessOperator.ToUpper() == "AND")
            {
                if (await S.Buff(SB.Darkness, C.DefensiveColor,
                    Me.HealthPercent < M.IS.HavocDarknessHp
                    && enemiesInMelee >= M.IS.HavocDarknessUnits,
                    string.Format($"HP:{Me.HealthPercent.ToString("F0")}<{M.IS.HavocDarknessHp} AND Units:{enemiesInMelee}>={M.IS.HavocDarknessUnits}")))
                { return true; }
            }
            else
            {
                if (await S.Buff(SB.Darkness, C.DefensiveColor,
                    M.IS.HavocDarknessUnits > 0
                && (enemiesInMelee >= M.IS.HavocDarknessUnits),
                 string.Format($"Units:{enemiesInMelee}>={M.IS.HavocDarknessUnits}")))
                {
                    return true;
                }
                if (await S.Buff(SB.Darkness, C.DefensiveColor,
                    M.IS.HavocDarknessHp > 0
                    && Me.HealthPercent < M.IS.HavocDarknessHp,
                    string.Format($"HP:{Me.HealthPercent.ToString("F0")}<{M.IS.HavocDarknessHp}")))
                {
                    return true;
                }
            }


            #endregion

            #region Chaos Nova
            if (M.IS.HavocChaosNovaUnits > 0 && M.IS.HavocChaosNovaHp > 0 && M.IS.HavocChaosNovaOperator.ToUpper() == "AND")
            {
                if (await S.Buff(SB.ChaosNova, C.DefensiveColor,
                    Me.HealthPercent < M.IS.HavocChaosNovaHp
                    && enemiesInMelee >= M.IS.HavocChaosNovaUnits,
                    string.Format($"HP:{Me.HealthPercent.ToString("F0")}<{M.IS.HavocChaosNovaHp} AND Units:{enemiesInMelee}>={M.IS.HavocChaosNovaUnits}")))
                { return true; }
            }
            else
            {
                if (await S.Buff(SB.ChaosNova, C.DefensiveColor,
                    M.IS.HavocChaosNovaUnits > 0
                && (enemiesInMelee >= M.IS.HavocChaosNovaUnits),
                 string.Format($"Units:{enemiesInMelee}>={M.IS.HavocChaosNovaUnits}")))
                {
                    return true;
                }
                if (await S.Buff(SB.ChaosNova, C.DefensiveColor,
                    M.IS.HavocChaosNovaHp > 0
                    && Me.HealthPercent < M.IS.HavocChaosNovaHp,
                    string.Format($"HP:{Me.HealthPercent.ToString("F0")}<{M.IS.HavocChaosNovaHp}")))
                {
                    return true;
                }
            }

            #endregion

            return false;
        }
        #endregion

        #region Area of Effect
        public static async Task<bool> AoE()
        {
            // Vengeful retreat if we have a Fel Rush Available which will take us out of melee 
            // and this aoe should no longer hit and hopefully use Fel Rush again.
            #region Multi-Target Vengeful Retreat
            // use it if we have Prepared talent differently
            if (await S.GCD(SB.VengefulRetreat, C.CombatColor,
                M.IS.HavocVengefulReatreatAoe
                && !S.OnCooldown(SB.FelRush)                                   // we can cast fel rush
                && C.CurrentPower <= 85                                        // and Fury <= 85
                && T.HavocPrepared                                          // We took prepared so use 85 or less
                && (S.CooldownTimeLeft(SB.FelRush) < 500), "AoE"))          // and cooldown timer < 500ms (2nd one)
            { return true; }

            // use it different if we DO NOT have Prepared talent
            if (await S.GCD(SB.VengefulRetreat, C.CombatColor,
               M.IS.HavocVengefulReatreatAoe
               && !S.OnCooldown(SB.FelRush)                                       // we can cast fel rush
               && C.CurrentPower <= 70                                             // and Fury <= 70
               && !T.HavocPrepared                                              // We took prepared so use 85 or less
               && (S.CooldownTimeLeft(SB.FelRush) < 500), "AoE"))               // and cooldown timer < 500ms (2nd one)
            { return true; }
            #endregion

            if (await S.GCD(SB.EyeBeam, C.CombatColor, addLog: "AoE")) { return true; }
            if (await S.Cast(SB.ChaosStrike, C.CombatColor, T.HavocChaosCleave, "AoE")) { return true; }
            if (await S.GCD(SB.BladeDance, C.CombatColor, addLog: "AoE")) { return true; }
            if (await S.Cast(SB.ChaosStrike, C.CombatColor, C.CurrentPower >= 70, "AoE")) { return true; }
            if (await S.Cast(SB.DemonsBite, C.CombatColor, addLog: "AoE")) { return true; }
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor, !CurrentTarget.IsWithinMeleeRange, "AoE")) { return true; }

            return false;

        }
        public static async Task<bool> AoEDashing()
        {


            #region Multi-Target Fel Rush
            // use this in combination with vengeful retreat
            if (await S.GCD(SB.FelRush, C.CombatColor,
                M.IS.HavocFelRushAoe
                && T.HavocFelMastery                               // we took the talent for fel mastery
                && C.CurrentPower <= 70                            // make sure we have 70 or less Fury
                && CurrentTarget.Distance >= 10                 // make sure we aren't too close
                && CurrentTarget.Distance <= 30, "AoED"))               // make sure we aren't too far
            { return true; }

            // use to engage
            if (await S.GCD(SB.FelRush, C.CombatColor,
                M.IS.HavocFelRushAoe
                && !T.HavocFelMastery      // we took the talent for fel mastery
                && CurrentTarget.Distance >= 10                 // make sure we aren't too close
                && CurrentTarget.Distance <= 30, "AoED"))               // make sure we aren't too far
            { return true; }
            #endregion

            return false;
        }
        #endregion

        #region Single Target

        /// <summary>
        /// this is the opening rotation for single target. Once activated, it will continue to use these abilities until it has been fulfilled and then reset again.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> OpeningRotationSingleTarget()
        {

            if (M.IS.HavocFelRushOnPull && M.IS.HavocUseMetamorphosisCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.Cooldown
                && S.MaxChargesAvailable(SB.FelRush)           // make sure we have max charges saved up.
                && !Me.IsWithinMeleeRangeOf(CurrentTarget)         // make sure we are not in melee range.
                && !S.OnCooldown(SB.MetamorphosisSpell)
                && CurrentTarget.Distance < 25)
            {
                useOpenRotation = true;
            }
            if (useOpenRotation)
            {
                L.infoLog("Ready for Havoc Single Target Opener: " + openingRotationSkillsUsed, C.InfoColor);
                if (openingRotationSkillsUsed == 0)
                {
                    // we haven't used an ability yet for opener, let's fel rush
                    if (await S.GCD(SB.FelRush, C.CombatColor, true, "Opener 1"))
                    {
                        openingRotationSkillsUsed++;
                        return true;
                    }
                }
                if (openingRotationSkillsUsed == 1)
                {
                    if (T.HavocFelMastery)
                    {
                        await S.GCD(SB.FelRush, C.CombatColor, true, "Opener 2");
                    }
                    openingRotationSkillsUsed++;
                    return true;
                }
                if (openingRotationSkillsUsed == 2)
                {
                    if (await S.CastGround(SB.MetamorphosisSpell, C.CombatColor, true, "Opener 3"))
                    {
                        // if we did metamorphosis then that's our last ability we can now continue single target
                        useOpenRotation = false;
                        openingRotationSkillsUsed = 0;
                    }
                }
                return true;
            }
            return false;
        }
        public static async Task<bool> SingleTarget()
        {

            if (await S.CastGround(SB.MetamorphosisSpell, C.CombatColor,
                M.IS.HavocUseMetamorphosisCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.Cooldown
                || (M.IS.HavocUseMetamorphosisCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.BossOnly && CurrentTarget.IsBoss)
                || (M.IS.HavocUseMetamorphosisCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.EliteBoss && (CurrentTarget.IsBoss || CurrentTarget.Elite))
                ))
            { return true; }

            // chaos strike Fury dump
            if (await S.Cast(SB.ChaosStrike, C.CombatColor,
                C.CurrentPower >= 70
                && Me.IsWithinMeleeRangeOf(CurrentTarget), "ST"))
            { return true; }



            #region Single-Target Vengeful Retreat


            // use it if we have Prepared talent differently
            if (await S.GCD(SB.VengefulRetreat, C.CombatColor,
                M.IS.HavocVengefulReatreatSingleTarget
                && C.CurrentPower <= 85
                && !S.OnCooldown(SB.FelRush)
                && S.CooldownTimeLeft(SB.FelRush) < 500
                && T.HavocPrepared
                && Me.IsWithinMeleeRangeOf(CurrentTarget), "ST1"))
            { return true; }

            // use it if we DO NOT have Prepared talent without Fury check
            if (await S.GCD(SB.VengefulRetreat, C.CombatColor,
                M.IS.HavocVengefulReatreatAoe
                && !S.OnCooldown(SB.FelRush)
                && S.CooldownTimeLeft(SB.FelRush) < 500
                && !T.HavocPrepared
                && Me.IsWithinMeleeRangeOf(CurrentTarget), "ST2"))
            { return true; }

            #endregion

            #region Single-Target Fel Rush
            // use this in combination with vengeful retreat
            if (await S.Cast(SB.FelRush, C.CombatColor,
                M.IS.HavocFelRushSingleTarget
                && T.HavocFelMastery
                && Me.CurrentTarget.Distance >= 10
                && Me.CurrentTarget.Distance <= 30,
                "ST 1"))
            { return true; }

            // use to engage
            if (await S.Cast(SB.FelRush, C.CombatColor,
                M.IS.HavocFelRushSingleTarget
                && !Me.IsWithinMeleeRangeOf(CurrentTarget)
                && CurrentTarget.Distance >= 10
                && CurrentTarget.Distance <= 30,
                "ST 2"))
            { return true; }
            #endregion

            // only cast bite if we don't have DemonBlades.  DB removes the ability of Demon's bite
            if (await S.Cast(SB.DemonsBite, C.CombatColor,
                !T.HavocDemonBlades
                && Me.IsWithinMeleeRangeOf(CurrentTarget), "ST"))
            { return true; }

            // melee range with Demon Blades
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor,
                T.HavocDemonBlades
                && Me.IsWithinMeleeRangeOf(CurrentTarget)
                && C.CurrentPower < 70, "ST"))
            { return true; }

            // nothing else to do so let's throw a glaive
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor,
                T.HavocDemonBlades
                && !Me.IsWithinMeleeRangeOf(CurrentTarget)
                && C.CurrentPower < 70, "ST"))
            { return true; }

            return false;

        }
        #endregion

        #endregion

    }
}
