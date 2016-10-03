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
using Buddy.Coroutines;
using System.Numerics;
using Styx.Helpers;
using Styx.Pathing;
using System.Diagnostics;

#endregion

namespace Illidari.Rotation
{
    class Havoc
    {
        private static Stopwatch openRotationTimeout = new Stopwatch();
        private static Stopwatch felRushAfterVengefulRetreat = new Stopwatch();
        public enum OpenerSteps { None, InitialFelRush, FelMasteryFelRush, FuryBuilder, FaceAwayFromTarget, VengefulRetreat, Nemesis, Metamorphosis, ChaosBlades }
        private static bool useOpenRotation = false;
        public static OpenerSteps openingRotationSkillsUsed = OpenerSteps.None;
        //private static Vector3 origSpot;
        //private static Vector3 safeSpot;
        //private static float needFacing;
        
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static WoWUnit CurrentTarget { get { return StyxWoW.Me.CurrentTarget; } }
        //private static uint CurrentFury => StyxWoW.Me.GetPowerInfo(WoWPowerType.Fury).Current;

        #region Precombat Buffing
        public static async Task<bool> PreCombatBuffing()
        {
            if (HK.manualOn || Me.Combat || !Me.IsAlive || (Me.OnTaxi))
                return true;
            // check to see if we should use a flask
            //if (await I.UseItem(I.FindBestPrecombatFlask(), M.IS.HavocUseAgilityFlask && !Me.HasAura(SB.FlaskList)))
            //{
            //    return true;
            //}
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

                if (await S.Cast(SB.FelBlade, C.CombatColor,
                    CurrentTarget.Distance <= 15))
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
            if (HK.manualOn || !Me.IsAlive || (Me.OnTaxi) || Me.IsCasting || Me.IsChanneling)
                return true;
            // if I 

            // i don't want facing to interfere with vengeful retreat opener.
            if (openingRotationSkillsUsed != OpenerSteps.FaceAwayFromTarget && openingRotationSkillsUsed != OpenerSteps.VengefulRetreat)
            {
                if (M.IS.GeneralFacing)
                {
                    // check to see if we need to face target
                    await C.FaceTarget(CurrentTarget);
                }
            }

            if (M.IS.GeneralMovement)
            {
                // ensure we are in melee range and not too close
                await C.EnsureMeleeRange(CurrentTarget);
            }

            if (CurrentTarget.IsValidCombatUnit())
            {

                if (await OpeningRotationSingleTarget())
                {
                    return true;
                }

                #region Offensive Hotkey
                if (HK.HavocOffensiveOn)
                {
                    // we have offensive cooldowns enabled, let's use them if they are available first.
                    if (await I.UseItem(I.GetItemByName("Draenic Agility Potion"), M.IS.HotkeyHavocOffensiveAgilityPotion))
                    {
                        return true;
                    }
                    if (await S.Cast(SB.MetamorphosisSpell, C.CombatColor, M.IS.HotkeyHavocOffensiveMetamorphosis))
                    {
                        return true;
                    }
                    if (await S.Cast(SB.FuryOfTheIllidari, C.CombatColor, M.IS.HotkeyHavocOffensiveFoTI))
                    {
                        return true;
                    }
                }
                #endregion
                // see if we need any defensive cooldowns
                if (await SaveMe()) { return true; }

                if ((HK.HavocAoEOn || U.activeEnemies(Me.Location, 8f).Count() >= 2))
                {
                    // try to use aoe abilities
                    if (await AoE(U.activeEnemies(Me.Location, 8f).Count())) { return true; }
                    // we were supposed to aoe, but nothing to use for aoe (or jumped back out of melee), so use single target
                    if (await AoEDashing(U.activeEnemies(Me.Location, 8f).Count())) { return true; }
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
        public static async Task<bool> AoE(int count)
        {
            // use metamorphosis in aoe mode
            if (await S.CastGround(SB.MetamorphosisSpell, C.CombatColor, UseMetamorphosisCD(true))) { return true; } 

            // initial Fel Rush with Fel Mastery talent, near the cap, almost 2 charges at > 3 targets or trigger momentum
            if (await S.GCD(SB.FelRush, C.CombatColor,
                M.IS.HavocFelRushAoe
                && Me.CurrentTarget.Distance <= 18
                && ((T.HavocFelMastery && !S.OnCooldown(SB.VengefulRetreat) && C.MissingPower <= 30 && count >= 3)
                    || (NeedMomentum())
                ), "AoE Initial", sleep: false))
            { return true; }

            #region Multi-Target Vengeful Retreat
            // use it if we have Prepared talent differently
            if (await S.GCD(SB.VengefulRetreat, C.CombatColor, M.IS.HavocVengefulReatreatAoe
                && C.MissingPower > 45
                && FelRushCheckCharge(true)
                && T.HavocPrepared
                && Me.IsWithinMeleeRangeOf(CurrentTarget), "AoE"))          // and cooldown timer < 500ms (2nd one)
            { return true; }

            // use it different if we DO NOT have Prepared talent
            if (await S.GCD(SB.VengefulRetreat, C.CombatColor,
                M.IS.HavocVengefulReatreatAoe
                && FelRushCheckCharge(true)
                && !T.HavocPrepared
                && Me.IsWithinMeleeRangeOf(CurrentTarget), "AoE"))               // and cooldown timer < 500ms (2nd one)
            { return true; }
            #endregion

            // Fel Rush if we almost hitting max charges with 3 or more
            if (await S.GCD(SB.FelRush, C.CombatColor,
                M.IS.HavocFelRushAoe
                && Me.CurrentTarget.Distance <= 18 && count >= 3 && FelRushAlmostMaxCharge(), "AoE Max Charges", sleep: false))
            { return true; }

            if (await S.Cast(SB.FuryOfTheIllidari, C.CombatColor, UseFuryOfIllidariCD(count >= 3) && CanUseAbilityWithMomentum())) { return true; }
            if (await S.Cast(SB.FelBarrage, C.CombatColor, count >= 3 && CanUseAbilityWithMomentum() && T.HavocFelBarrage && S.GetCharges(SB.FelBarrage) >= 4 && CurrentTarget.Distance <= 30, "AoE")) { return true; }
            if (await S.GCD(SB.EyeBeam, C.CombatColor, count >= 3 && CanUseAbilityWithMomentum(), "AoE")) { return true; }
            if (await S.GCD(SB.BladeDance, C.CombatColor, count>=3, "AoE")) { return true; }
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor, count >= 3 && CanUseAbilityWithMomentum() && T.HavocBloodlet && CurrentTarget.Distance <= 30, "AoE")) { return true; }
            if (await S.Cast(SB.ChaosStrike, C.CombatColor, T.HavocChaosCleave && count <= 3, "AoE")) { return true; }
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor, count >= 3 && CurrentTarget.Distance <= 30, "AoE")) { return true; }
            if (await S.Cast(SB.ChaosStrike, C.CombatColor, (C.CurrentPower >= 70 && !T.HavocDemonBlades) || (C.CurrentPower >= 60 && T.HavocDemonBlades), "AoE")) { return true; }
            if (await S.Cast(SB.DemonsBite, C.CombatColor, !T.HavocDemonBlades, "AoE")) { return true; }

            return false;

        }
        public static async Task<bool> AoEDashing(int count)
        {


            #region Multi-Target Fel Rush
            // use this in combination with vengeful retreat
            if (await S.GCD(SB.FelRush, C.CombatColor,
                M.IS.HavocFelRushAoe
                && !S.OnCooldown(SB.VengefulRetreat)
                && T.HavocFelMastery
                && Me.CurrentTarget.Distance <= 18, "AoED", sleep: false))
            { return true; }

            // use to engage
            if (await S.GCD(SB.FelRush, C.CombatColor,
                M.IS.HavocFelRushAoe
                && !S.OnCooldown(SB.VengefulRetreat)
                && T.HavocFelMastery
                && Me.CurrentTarget.Distance <= 18, "AoED", sleep: false))
            { return true; }
            #endregion

            return false;
        }
        #endregion

        #region Single Target

        #region Opening Rotation
        /// <summary>
        /// this is the opening rotation for single target. Once activated, it will continue to use these abilities until it has been fulfilled and then reset again.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> OpeningRotationSingleTarget()
        {
            //L.debugLog("UseRotation:" + (!useOpenRotation).ToString() + ", FelRushOnPull:" + (M.IS.HavocFelRushOnPull).ToString() + ", UseMetaCd:" + UseMetamorphosisCD(false).ToString() + ", FRMaxCharges:" + S.MaxChargesAvailable(SB.FelRush).ToString() + ", FRChargesNoFelMastery:" + (S.GetSpellChargeInfo(SB.FelRush).ChargesLeft >= 1 && !T.HavocFelMastery).ToString() + ", MetaCD:" + (!S.OnCooldown(SB.MetamorphosisSpell)).ToString());

            if ((useOpenRotation && (openRotationTimeout.ElapsedMilliseconds > 5000 && openingRotationSkillsUsed != OpenerSteps.FuryBuilder) || openRotationTimeout.ElapsedMilliseconds > 10000 && openingRotationSkillsUsed == OpenerSteps.FuryBuilder))
            {
                useOpenRotation = false;
                openingRotationSkillsUsed = OpenerSteps.None;
                L.infoLog("Opener Rotation timed out.", C.InfoColor);
                return false;
            }

            

            if (!useOpenRotation && (M.IS.HavocFelRushOnPull && UseMetamorphosisCD(false)
                && (S.MaxChargesAvailable(SB.FelRush)           // make sure we have max charges saved up.
                    || (S.GetSpellChargeInfo(SB.FelRush).ChargesLeft >= 1 && !T.HavocFelMastery) // also check for 1 charge if no fel mastery.
                )
                && !S.OnCooldown(SB.MetamorphosisSpell)         // make sure metamorphosis is not on cd.
                && CurrentTarget.Distance < 25))                 // make sure we aren't too far away.
            {
                L.infoLog("Ready for Havoc Single Target Opener: " + openingRotationSkillsUsed, C.HavocOpenerColor);
                useOpenRotation = true;
                openingRotationSkillsUsed = OpenerSteps.InitialFelRush;
                openRotationTimeout.Restart();
                return true;
            }

            if (useOpenRotation)
            {
                //L.debugLog(openingRotationSkillsUsed.ToString());
                if (openingRotationSkillsUsed == OpenerSteps.InitialFelRush)
                {

                    // we haven't used an ability yet for opener, let's fel rush
                    if (await S.GCD(SB.FelRush, C.HavocOpenerColor, CurrentTarget.Distance <= 15 && Me.IsSafelyFacing(CurrentTarget), "Opener 1"))
                    {
                        openingRotationSkillsUsed = OpenerSteps.FelMasteryFelRush;
                        openRotationTimeout.Restart();
                        return true;
                    }
                }

                if (openingRotationSkillsUsed == OpenerSteps.FelMasteryFelRush)
                {
                    //L.debugLog("Made it to FelMasteryFelRush : HasFelMastery:" + T.HavocFelMastery + ", HasMomentum: " + Me.HasAnyAura("Momentum"));
                    if (await S.GCD(SB.FelRush, C.HavocOpenerColor,
                        T.HavocFelMastery && NeedMomentum(), "Opener 2 - Fel Mastery 2nd Fel Rush"))
                    {
                        openingRotationSkillsUsed = OpenerSteps.FuryBuilder;
                        openRotationTimeout.Restart();
                        return true;
                    }
                    // if we don't have fel mastery talent, just bump the rotation. 
                    // really should have it, but just in case someone thinks they know better ;] 
                    if (!T.HavocFelMastery)
                    {
                        L.combatLog("Skipping Opener 2; No Fel Mastery talent", C.HavocOpenerColor);
                        openingRotationSkillsUsed = OpenerSteps.FuryBuilder;
                        openRotationTimeout.Restart();
                        return true;
                    }
                }

                if (openingRotationSkillsUsed == OpenerSteps.FuryBuilder)
                {
                    if (await S.GCD(SB.DemonsBite, C.HavocOpenerColor, !T.HavocDemonBlades, "Opener 3, building Fury"))
                    {
                        return true;
                    }


                    // if we have demon blades, we want to passively get <=20 missing fury.
                    if (T.HavocDemonBlades && C.MissingPower > 20)
                    {
                        L.combatLog("Opener 3, passively building fury", C.HavocOpenerColor);
                    }
                    if (C.MissingPower <= 20)
                    {
                        openingRotationSkillsUsed = OpenerSteps.FaceAwayFromTarget;
                        return true;
                    }
                }

                // make sure we have max charges saved up.
                if (openingRotationSkillsUsed == OpenerSteps.FaceAwayFromTarget)
                {
                    // we don't want to be safely facing
                    if (Me.IsSafelyFacing(CurrentTarget.Location))
                    {
                        await FaceAwayFromTarget();
                        openingRotationSkillsUsed = OpenerSteps.VengefulRetreat;
                    }
                }
                //if (openingRotationSkillsUsed == OpenerSteps.VengefulRetreat && !Me.IsSafelyFacing(CurrentTarget))
                //{
                //    // wait until you are safely facing.
                //    return true;
                //}

                if (openingRotationSkillsUsed == OpenerSteps.VengefulRetreat && !Me.IsSafelyFacing(CurrentTarget.Location))
                {
                    if (await S.Cast(SB.VengefulRetreat, C.HavocOpenerColor, T.HavocPrepared || T.HavocMomentum, "Opener 4"))
                    {
                        openingRotationSkillsUsed = OpenerSteps.Nemesis;
                        return true;
                    }
                }

                if (openingRotationSkillsUsed == OpenerSteps.Nemesis)
                {

                    if (await S.Cast(SB.Nemesis, C.HavocOpenerColor, T.HavocNemesis, "Opener 5"))
                    {
                        openingRotationSkillsUsed = OpenerSteps.Metamorphosis;
                        return true;
                    }
                    if (!T.HavocNemesis)
                    {
                        L.combatLog("Skip Opener 5 - Nemesis.  No talent selected", C.HavocOpenerColor);
                        openingRotationSkillsUsed = OpenerSteps.Metamorphosis;
                        return true;
                    }


                }
                if (openingRotationSkillsUsed == OpenerSteps.Metamorphosis)
                {
                    // if in melee, need to cast on yourself.  If in ranged, cast on target.
                    if (await S.CastGround(SB.MetamorphosisSpell, C.HavocOpenerColor, !Me.IsWithinMeleeRangeOf(CurrentTarget), "Opener 6 - Cast on Target"))
                    {
                        openingRotationSkillsUsed = OpenerSteps.ChaosBlades;
                        return true;
                    }
                    if (await S.CastGroundOnMe(SB.MetamorphosisSpell, C.HavocOpenerColor, Me.IsWithinMeleeRangeOf(CurrentTarget), "Opener 6 - Cast on Me"))
                    {
                        // if we did metamorphosis then that's our last ability we can now continue single target
                        openingRotationSkillsUsed = OpenerSteps.ChaosBlades;
                        return true;
                    }
                }
                if (openingRotationSkillsUsed == OpenerSteps.ChaosBlades)
                {
                    if (await S.Cast(SB.ChaosBlades, C.HavocOpenerColor, T.HavocChaosBlades, "Opener 7"))
                    {
                        useOpenRotation = false;
                        openingRotationSkillsUsed = OpenerSteps.None;
                    }
                    if (!T.HavocChaosBlades)
                    {
                        useOpenRotation = false;
                        openingRotationSkillsUsed = OpenerSteps.None;
                        return true;
                    }
                }
                await S.Cast(SB.DemonsBite, C.HavocOpenerColor, !T.HavocDemonBlades && Me.IsWithinMeleeRangeOf(CurrentTarget), "Opener, Filler");
            }
            return useOpenRotation;
        }
        public static async Task FaceAwayFromTarget()
        {
            //Core.SafeArea sa = new Core.SafeArea();
            //sa.MinScanDistance = 25;
            //sa.MaxScanDistance = sa.MinScanDistance;
            //sa.RaysToCheck = 36;
            //sa.LineOfSightMob = CurrentTarget;
            //sa.CheckLineOfSightToSafeLocation = true;
            //sa.CheckSpellLineOfSightToMob = false;
            //sa.DirectPathOnly = true;

            //safeSpot = sa.FindLocation();
            //if (safeSpot != Vector3.Zero)
            //{
            //    origSpot = new Vector3(Me.Location.X, Me.Location.Y, Me.Location.Z);
            //    needFacing = Styx.Helpers.WoWMathHelper.CalculateNeededFacing(safeSpot, origSpot);
            //    needFacing = WoWMathHelper.NormalizeRadian(needFacing);
            //    float rotation = WoWMathHelper.NormalizeRadian(Math.Abs(needFacing - Me.RenderFacing));
            //}

            //Me.SetFacing(needFacing);
            //await Coroutine.Sleep(500);
            Me.SetFacing(WoWMathHelper.CalculatePointBehind(Me.Location, 36, 5f));
            await Coroutine.Yield();
        }

        #endregion

        public static async Task<bool> SingleTarget()
        {
            // we only want to use the single target rotation after the open rotation is finished.
            if (await S.CastGround(SB.MetamorphosisSpell, C.CombatColor, UseMetamorphosisCD(false))) { return true; }

            #region Single-Target Vengeful Retreat


            // use it if we have Prepared talent differently
            if (await S.GCD(SB.VengefulRetreat, C.CombatColor,
                M.IS.HavocVengefulReatreatSingleTarget
                && C.MissingPower > 30
                && (FelRushCheckCharge(true))
                && T.HavocPrepared
                && Me.IsWithinMeleeRangeOf(CurrentTarget), "ST1", sleep: false))
            { felRushAfterVengefulRetreat.Restart(); C.fallingTimeout.Restart(); return true; }

            // use it if we DO NOT have Prepared talent without Fury check
            if (await S.GCD(SB.VengefulRetreat, C.CombatColor,
                M.IS.HavocVengefulReatreatAoe
                && (FelRushCheckCharge(true))
                && !T.HavocPrepared
                && Me.IsWithinMeleeRangeOf(CurrentTarget), "ST2", sleep: false))
            { felRushAfterVengefulRetreat.Restart();  C.fallingTimeout.Restart(); return true; }

            #endregion



            #region Single-Target Fel Rush
            // use this in combination with vengeful retreat
            if (await S.Cast(SB.FelRush, C.CombatColor, M.IS.HavocFelRushSingleTarget && S.OnCooldown(SB.VengefulRetreat) 
                && felRushAfterVengefulRetreat.ElapsedMilliseconds <= 2000
                && T.HavocFelMastery 
                && Me.CurrentTarget.Distance <= 18 
                && C.MissingPower <= 30, 
            "ST 1"))
            { felRushAfterVengefulRetreat.Reset(); return true; }

            // use to engage
            if (await S.Cast(SB.FelRush, C.CombatColor, M.IS.HavocFelRushSingleTarget 
                && !Me.IsWithinMeleeRangeOf(CurrentTarget) && CurrentTarget.Distance <= 18, "ST 2"))
            { felRushAfterVengefulRetreat.Reset(); return true; }
            #endregion

            // if we have used vengeful retreat, don't do anything else until we fel rush
            if (felRushAfterVengefulRetreat.IsRunning && felRushAfterVengefulRetreat.ElapsedMilliseconds <= 2000) { return true; }

            // fel rush, almost max charges and needs momentum
            if (await S.Cast(SB.FelRush, C.CombatColor, M.IS.HavocFelRushSingleTarget && Me.IsWithinMeleeRangeOf(CurrentTarget)
                && FelRushAlmostMaxCharge()
                && NeedMomentum(true), "ST - Fel Rush for Momentum"))
            { return true; }
            if (await S.Cast(SB.EyeBeam, C.CombatColor, T.HavocDemonic && CurrentTarget.Distance <= 10, "ST - Demonic talent")) { return true; }
            if (await S.Cast(SB.FelEruption, C.CombatColor, T.HavocFelEruption && CurrentTarget.Distance <= 20, "ST")) { return true; }
            L.debugLog(CanUseAbilityWithMomentum().ToString() + ", " + Me.IsWithinMeleeRangeOf(CurrentTarget).ToString() + ", " + UseFuryOfIllidariCD(false).ToString());
            if (await S.Cast(SB.FuryOfTheIllidari, C.CombatColor, CanUseAbilityWithMomentum() && Me.IsWithinMeleeRangeOf(CurrentTarget)
                    && UseFuryOfIllidariCD(false), "ST"))
            { return true; }
            if (await S.Cast(SB.BladeDance, C.CombatColor, CanUseAbilityWithMomentum() && T.HavocFirstBlood && Me.IsWithinMeleeRangeOf(CurrentTarget), "ST")) { return true; }
            if (await S.Cast(SB.FelBlade, C.CombatColor, C.MissingPower <= 30 && CurrentTarget.Distance <= 15, "ST")) { return true; }
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor, CanUseAbilityWithMomentum() && T.HavocBloodlet && CurrentTarget.Distance <= 30, "ST")) { return true; }
            //if (await S.Cast(SB.FelBarrage, C.CombatColor, NeedMomentum() && T.HavocFelBarrage && S.GetCharges(SB.FelBarrage) >= 5, "ST")) { return true; }
            if (await S.Cast(SB.ChaosStrike, C.CombatColor, C.MissingPower <= 30 && Me.HasAnyTempAura("Metamorphosis") && Me.IsWithinMeleeRangeOf(CurrentTarget), "ST")) { return true; }
            if (await S.Cast(SB.EyeBeam, C.CombatColor, Me.HasAura(SB.AuraAnguishOfTheDeceiver) && CurrentTarget.Distance <= 10, "ST - Has Eye of the Deceiver Trait")) { return true; }
            if (await S.Cast(SB.ChaosStrike, C.CombatColor, C.MissingPower <= 30 && Me.IsWithinMeleeRangeOf(CurrentTarget), "ST")) { return true; }
            if (await S.Cast(SB.FelBarrage, C.CombatColor, CanUseAbilityWithMomentum() && T.HavocFelBarrage && S.GetCharges(SB.FelBarrage) >= 4 && CurrentTarget.Distance <= 30, "ST")) { return true; }
            if (await S.Cast(SB.DemonsBite, C.CombatColor, !T.HavocDemonBlades && Me.IsWithinMeleeRangeOf(CurrentTarget) && !S.MaxChargesAvailable(SB.ThrowGlaive), "ST")) { return true; }

            // melee range with Demon Blades
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor,
                Me.IsWithinMeleeRangeOf(CurrentTarget), "ST"))
            { return true; }

            // nothing else to do so let's throw a glaive
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor,
                !Me.IsWithinMeleeRangeOf(CurrentTarget), "ST"))
            { return true; }
            return false;

        }
        #endregion

        /// <summary>
        /// If you want to use an ability with momentum
        /// <para>If you did not take momentum, it will return true.</para>
        /// <para>If you did take momentum and have it active, it will return true so you can use your ability</para>
        /// <para>If you did take momentum and do not have it active, it will return false</para>
        /// </summary>
        /// <returns></returns>
        static bool CanUseAbilityWithMomentum()
        {
            if (!T.HavocMomentum) { return true; }
            if (T.HavocMomentum && Me.HasAura(208628)) { return true; }
            return false;
        }
        /// <summary>
        /// If your ability causes momentum buff, this checks if you need it.
        /// <para>If you did not take momentum, it will return false.</para>
        /// <para>If you did take momentum and do not have it active, it will return true so you can use the momentum buff generating ability.</para>
        /// <para>If you did take momentum and have the buff active, it will return false.</para>
        /// </summary>
        /// <param name="defaultValue">Set the default value if you do not have the Momentum talent.</param>
        /// <returns></returns>
        static bool NeedMomentum(bool defaultValue = false)
        {
            if (!T.HavocMomentum) { return defaultValue; }
            if (T.HavocMomentum && !Me.HasAnyTempAura("Momentum")) { return true; }
            return false;
        }
        static bool FelRushAlmostMaxCharge()
        {
            return (S.GetSpellChargeInfo(SB.FelRush).ChargesLeft >= 1 && S.GetSpellChargeInfo(SB.FelRush).TimeUntilNextCharge.TotalMilliseconds < 500);
        }
        static bool FelRushCheckCharge(bool needForVengeful)
        {
            // if we are already almost at max charges, go for it
            if (FelRushAlmostMaxCharge()) { return true; }

            SpellChargeInfo sci = S.GetSpellChargeInfo(SB.FelRush);
            // if we have no charges, but almost 1 charge available, go for it
            if (sci.ChargesLeft == 0) { return (sci.TimeUntilNextCharge.TotalMilliseconds < 500); }

            if (sci.ChargesLeft == 1)
            {
                if (needForVengeful) { return true; }
                // if we have 1 charge, but the cooldown time fo vengeful retreat is less than next charge time, go for it
                if (sci.TimeUntilNextCharge.TotalMilliseconds <= S.CooldownTimeLeft(SB.VengefulRetreat)) { return true; }
            }
            // don't go for it.
            return false;
        }
        static bool UseFuryOfIllidariCD(bool aoe)
        {
            return (M.IS.HavocUseFuryOfTheIllidariCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.AoE && aoe)
                    || (M.IS.HavocUseFuryOfTheIllidariCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.BossOnly && CurrentTarget.IsBoss)
                    || (M.IS.HavocUseFuryOfTheIllidariCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.EliteBoss && (CurrentTarget.IsBoss || CurrentTarget.Elite))
                    || (M.IS.HavocUseFuryOfTheIllidariCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.Cooldown);
        }
        static bool UseMetamorphosisCD(bool aoe)
        {
            return (M.IS.HavocUseMetamorphosisCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.AoE && aoe)
                || M.IS.HavocUseMetamorphosisCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.Cooldown
                || (M.IS.HavocUseMetamorphosisCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.BossOnly && (CurrentTarget.IsBoss || CurrentTarget.IsTrainingDummyBoss()))
                || (M.IS.HavocUseMetamorphosisCooldown == Core.IllidariSettings.IllidariSettings.CooldownTypes.EliteBoss && ((CurrentTarget.IsBoss || CurrentTarget.IsTrainingDummyBoss()) || (CurrentTarget.Elite || CurrentTarget.IsTrainingDummyElite()))
            );
        }

       
        #endregion

    }

}
