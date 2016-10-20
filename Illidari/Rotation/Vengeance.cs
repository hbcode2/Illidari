using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx.WoWInternals.WoWObjects;
using Styx;
using Styx.WoWInternals;
using Styx.CommonBot.Coroutines;

using HK = Illidari.Core.Managers.Hotkeys;
using S = Illidari.Core.Spell;
using SB = Illidari.Core.Helpers.Spell_Book;
using U = Illidari.Core.Unit;
using I = Illidari.Core.Item;
using T = Illidari.Core.Managers.TalentManager;
using L = Illidari.Core.Utilities.Log;
using C = Illidari.Core.Helpers.Common;
using M = Illidari.Main;
using System.Diagnostics;
using Buddy.Coroutines;
using System.Numerics;
using Illidari.Core.IllidariSettings;

namespace Illidari.Rotation
{
    class Vengeance
    {
        private static VengeanceSettings VS => VengeanceSettings.Instance;
        private static GeneralSettings GS => GeneralSettings.Instance;
        private static HotkeySettings HKS => HotkeySettings.Instance;
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static WoWUnit CurrentTarget { get { return StyxWoW.Me.CurrentTarget; } }
        //private static uint CurrentPain => StyxWoW.Me.GetPowerInfo(WoWPowerType.Pain).Current;
        private static int infernalStrikeRange
        {
            get
            {
                if (T.VengeanceAbyssalStrike) { return 40; }
                return 30;
            }
        }

        #region Precombat Buffing
        public static async Task<bool> PreCombatBuffing()
        {
            if (HK.manualOn || Me.Combat || !Me.IsAlive || (Me.OnTaxi))
                return true;
            // check to see if we should use a flask
            //var item = I.FindBestPrecombatFlask();
            //if (item != null)
            //{
            //    if (await I.UseItem(item, !Me.HasAura(SB.FlaskList)))
            //    {
            //        return true;
            //    }
            //}
            await Coroutine.Yield();
            return false;
        }
        #endregion

        #region Combat Buffing
        public static async Task<bool> CombatBuffing()
        {
            if (HK.manualOn || !Me.IsAlive || (!Me.GotTarget || !CurrentTarget.IsAlive) || (Me.OnTaxi))
                return true;
            if (!Me.IsAutoAttacking && CurrentTarget.IsValidCombatUnit()) { Lua.DoString("StartAttack()"); return true; }
            //
            // use potion - Change "true" to configurable
            var potion = I.GetItemByName("Draenic Agility Potion");
            if (potion != null)
            {
                return await I.UseItem(potion, true);
            }
            return false;

        }

        #endregion

        #region Pull Logic
        public static async Task<bool> Pull()
        {
            if (CurrentTarget.IsValidCombatUnit())
            {
                await C.EnsureMeleeRange(CurrentTarget);
                await C.FaceTarget(CurrentTarget);

                if (await S.Cast(SB.ThrowGlaive, C.CombatColor, CurrentTarget.Distance <= 30, "Pull - GlaiveTimer: " + glaiveTossTimer.ElapsedMilliseconds + "ms")) { glaiveTossTimer.Restart(); return true; }

                // use to engage if you have the charges to do so
                if (await S.CastGround(SB.InfernalStrike, C.CombatColor,
                    VS.VengeanceCombatInfernalStrikePull
                    && CurrentTarget.Distance <= infernalStrikeRange
                    && !CurrentTarget.IsWithinMeleeRangeOf(Me)
                    && !VS.VengeancePreferPullWithFelblade
                , "Pull"))
                { return true; }

                // need to change this to check to see if we want to pull with Fel Blade or Infernal Strike (or which we prefer)
                if (await S.Cast(SB.FelBlade, C.CombatColor, T.VengeanceFelblade
                    && !CurrentTarget.IsWithinMeleeRangeOf(Me)
                    && CurrentTarget.Distance <= 15, "Pull"))
                { return true; }

                // now use in case felblade was on cd, but don't check prefer
                if (await S.CastGround(SB.InfernalStrike, C.CombatColor,
                    CurrentTarget.Distance <= infernalStrikeRange
                    && VS.VengeanceCombatInfernalStrikePull
                    && !CurrentTarget.IsWithinMeleeRangeOf(Me)
                , "Pull"))
                { return true; }
            }
            return false;
        }
        #endregion

        public static async Task<bool> RotationSelector()
        {
            // we are playing manual, i am not alive, or I am mounted or on taxi, do not continue
            if (HK.manualOn || !Me.IsAlive || (Me.OnTaxi))
                return true;

            // face taret, ensure melee, interrupt and taunt do not need to return as they are off of global cooldown.
            await C.FaceTarget(CurrentTarget);
            await C.EnsureMeleeRange(CurrentTarget);
            await InterruptTarget();
            if (await S.GCD(SB.Torment, C.CombatColor, !CurrentTarget.IsTargetingMeOrPet && VS.VengeanceAllowTaunt,
                string.Format($"CT:{CurrentTarget.SafeName} not targeting me. Taunting!")))
            { return true; }

            if (CurrentTarget.IsValidCombatUnit())
            {
                if (await ActiveMitigation()) { return true; }

                if (await GapCloser()) { return true; }

                if ((HK.VengeanceAoEOn || U.activeEnemies(Me.Location, 8f).Count() >= 3))
                {
                    if (await MultipleTarget())
                    {
                        return true;
                    }
                }

                // default to single target if nothing else
                return await SingleTarget();
            }
            return false;
        }

        public static async Task<bool> GapCloser()
        {
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor,
                !Me.IsWithinMeleeRangeOf(CurrentTarget)
                && CurrentTarget.Distance <= 30, "GapCloser - GlaiveTimer: " + glaiveTossTimer.ElapsedMilliseconds + "ms"))
            {
                glaiveTossTimer.Restart();
                return true;
            }
            if (await S.Cast(SB.FelBlade, C.CombatColor,
               T.VengeanceFelblade
               && !CurrentTarget.IsWithinMeleeRangeOf(Me)
               && CurrentTarget.MeleeDistance() <= 15,
               "ST Gap Closer"))
            { return true; }

            if (await S.CastGround(SB.InfernalStrike, C.CombatColor,
                VS.VengeanceCombatInfernalStrikeGapCloser
                && !CurrentTarget.IsWithinMeleeRangeOf(Me)
                && CurrentTarget.MeleeDistance() <= infernalStrikeRange
                && (S.GetSpellChargeInfo(SB.InfernalStrike).ChargesLeft > 0),
                "ST Gap Closer"))
            { return true; }

            return false;
        }

        public static async Task<bool> ActiveMitigation()
        {
            WoWUnit stunTarget = GetStunTarget(CurrentTarget, 8f);
            if (await S.CastGround(SB.SigilOfMisery, stunTarget, C.DefensiveColor, stunTarget != null
                && VS.VengeanceAllowStunSigilOfMisery))
            { return true; }

            #region Defensive Hotkey
            // demon spikes if force defensive is on
            if (await S.Cast(SB.DemonSpikes, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && HKS.HotkeyVengeanceDefensiveDemonSpikes
                && !Me.HasAura(SB.AuraDemonSpikes),
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveDemonSpikes:{HKS.HotkeyVengeanceDefensiveDemonSpikes.ToString()}")
            ))
            { return true; }

            if (await S.Cast(SB.MetamorphosisSpell, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && HKS.HotkeyVengeanceDefensiveMetamorphosis,
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveMetamorphosis:{HKS.HotkeyVengeanceDefensiveMetamorphosis.ToString()}")
            ))
            { return true; }

            if (await S.Cast(SB.SoulBarrier, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && HKS.HotkeyVengeanceDefensiveSoulBarrier,
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveSoulBarrier:{HKS.HotkeyVengeanceDefensiveSoulBarrier.ToString()}")
            ))
            { return true; }

            if (await S.Cast(SB.FelDevastation, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && HKS.HotkeyVengeanceDefensiveFelDevastation,
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveFelDevastation:{HKS.HotkeyVengeanceDefensiveFelDevastation.ToString()}")))

            if (Me.HasTankWarglaivesEquipped())
            {
                if (await S.Cast(SB.SoulCarver, C.DefensiveColor,
                    HK.VengeanceDefensiveOn
                    && HKS.HotkeyVengeanceDefensiveSoulCarver
                    && Me.IsWithinMeleeRangeOf(CurrentTarget) // must be within melee range or won't work.
                    && Me.IsSafelyFacing(CurrentTarget.Location),
                    string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveSoulCarver:{HKS.HotkeyVengeanceDefensiveSoulCarver.ToString()}")
                ))
                { return true; }
            }

            if (await S.Cast(SB.SoulCleave, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && HKS.HotkeyVengeanceDefensiveSoulCleave
                && CurrentTarget.IsWithinMeleeRangeOf(Me),
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveSoulCleave:{HKS.HotkeyVengeanceDefensiveSoulCleave.ToString()}")
            ))
            { return true; }

            if (await S.Cast(SB.EmpowerWards, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && HKS.HotkeyVengeanceDefensiveEmpowerWards,
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveEmpowerWards:{HKS.HotkeyVengeanceDefensiveEmpowerWards.ToString()}")
            ))
            { return true; }

            if (await S.Cast(SB.FieryBrand, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && HKS.HotkeyVengeanceDefensiveEmpowerWards,
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveEmpowerWards:{HKS.HotkeyVengeanceDefensiveEmpowerWards.ToString()}")))
            { return true; }

            #endregion

            if (await S.Cast(SB.MetamorphosisSpell, C.DefensiveColor,
                VS.VengeanceAllowMetamorphosis
                && Me.HealthPercent <= VS.VengeanceMetamorphosisHp,
                string.Format($"AM: HP:{Me.HealthPercent.ToString("F0")}<={VS.VengeanceMetamorphosisHp}")
            ))
            { return true; }

            // cast Demon Spikes if we have 
            if (await S.Cast(SB.DemonSpikes, C.DefensiveColor,
            VS.VengeanceAllowDemonSpikes
            && C.CurrentPower >= 20
            && Me.HealthPercent <= VS.VengeanceDemonSpikesHp
            && !Me.HasAura(SB.AuraDemonSpikes)
            && U.activeEnemies(Me.Location, 8f).Any(),
            string.Format($"AM: HP:{Me.HealthPercent.ToString("F0")}<={VS.VengeanceDemonSpikesHp}")
        ))
            { return true; }

            if (await S.Cast(SB.SoulBarrier, C.DefensiveColor, T.VengeanceSoulBarrier
                && VS.VengeanceAllowSoulBarrier
                && Me.HealthPercent <= VS.VengeanceSoulBarrierHp,
                string.Format($"AM: HP:{Me.HealthPercent.ToString("F0")}<={VS.VengeanceSoulBarrierHp}")
            ))
            { return true; }

            // make sure we have tank weapons equipped (for lower level stuff)

            if (await S.Cast(SB.SoulCarver, C.DefensiveColor,
                VS.VengeanceAllowSoulCarver
                && Me.HealthPercent <= VS.VengeanceSoulCarverHp,
                string.Format($"AM: HP:{Me.HealthPercent.ToString("F0")}<={VS.VengeanceSoulCarverHp}")
            ))
            { return true; }

            if (await S.Cast(SB.SoulCleave, C.DefensiveColor,
                VS.VengeanceAllowSoulCleave
                && C.CurrentPower >= 30
                && Me.CurrentHealth <= VS.VengeanceSoulCleaveHp
                && CurrentTarget.IsWithinMeleeRangeOf(Me),
                string.Format($"AM: HP:{Me.HealthPercent.ToString("F0")}<={VS.VengeanceSoulCleaveHp}")
            ))
            { return true; }

            if (await S.Cast(SB.EmpowerWards, C.DefensiveColor,
                VS.VengeanceEmpowerWards
                && U.activeEnemies(Me.Location, 50f).Where(u =>
                    u.IsTargetingMeOrPet && u.IsCasting)
                .Any()
            )) { return true; }

            if (await S.Cast(SB.FieryBrand, C.DefensiveColor,
                VS.VengeanceAllowFieryBrand
                && Me.HealthPercent <= VS.VengeanceFieryBrandHp))
            { return true; }

            return false;
        }
        private static Stopwatch glaiveTossTimer = new Stopwatch();
        public static async Task<bool> SingleTarget()
        {
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor,
                (!glaiveTossTimer.IsRunning && VS.VengeanceCombatThrowGlaive)
                || (glaiveTossTimer.IsRunning && VS.VengeanceCombatThrowGlaive && glaiveTossTimer.ElapsedMilliseconds > VS.VengeanceCombatThrowGlaiveSeconds), "ST - GlaiveTimer: " + glaiveTossTimer.ElapsedMilliseconds + "ms"))
            {
                glaiveTossTimer.Restart();
                return true;
            }

            if (await S.Cast(SB.SoulCleave, C.CombatColor,
                C.CurrentPower >= VS.VengeanceCombatSoulCleavePain
                && ((T.VengeanceSpiritBomb && CurrentTarget.HasAura(SB.AuraFrailty)  && CurrentTarget.GetAuraById(SB.AuraFrailty).TimeLeft.TotalMilliseconds > 5000) 
                    ||  (!T.VengeanceSpiritBomb))
                && CurrentTarget.IsWithinMeleeRangeOf(Me),
                string.Format($"ST: CP:{C.CurrentPower}>={VS.VengeanceCombatSoulCleavePain}")
            ))
            { return true; }

            // cast infernal strike in melee only if we have max chargets
            // it is off of the gcd, so can be cast any time.
            if (await S.CastGroundOnMe(SB.InfernalStrike, C.CombatColor,
                VS.VengeanceCombatInfernalStrikeSingleTarget
                && Me.IsWithinMeleeRangeOf(CurrentTarget)
                && S.MaxChargesAvailable(SB.InfernalStrike)
                && CurrentTarget.IsWithinMeleeRangeOf(Me),
                "ST Max Charges Available"))
            { return true; }

            
            if (await S.Cast(SB.ImmolationAura, C.CombatColor, CurrentTarget.IsWithinMeleeRangeOf(Me), "ST")) { return true; }
            if (await S.Cast(SB.FelBlade, C.CombatColor, T.VengeanceFelblade, "ST")) { return true; }
            if (await S.Cast(SB.FelEruption, C.CombatColor, T.VengeanceFelEruption && CurrentTarget.IsWithinMeleeRangeOf(Me), "ST")) { return true; }
            if (await S.Cast(SB.SpiritBomb, C.CombatColor, T.VengeanceSpiritBomb && Me.HasAura(SB.AuraSoulFragments) && (!CurrentTarget.HasAura(SB.AuraFrailty) || (CurrentTarget.HasAura(SB.AuraFrailty) && CurrentTarget.GetAuraById(SB.AuraFrailty).TimeLeft.TotalMilliseconds <= 3000)), "ST")) { return true; }
            if (await S.Cast(SB.Shear, C.CombatColor, T.VengeanceBladeTurning && Me.HasAura(SB.AuraBladeTurning) && CurrentTarget.IsWithinMeleeRangeOf(Me), "ST")) { return true; }
            if (await S.Cast(SB.Fracture, C.CombatColor, T.VengeanceFracture && CurrentTarget.IsWithinMeleeRangeOf(Me), "ST")) { return true; }
            if (await S.Cast(SB.SigilOfFlameTalented, C.CombatColor, T.VengeanceConcentratedSigils && CurrentTarget.IsWithinMeleeRangeOf(Me), "ST - Contentrated Sigils")) { return true; }
            if (await S.CastGround(SB.SigilOfFlame, C.CombatColor, !T.VengeanceConcentratedSigils && !Me.IsWithinMeleeRangeOf(CurrentTarget), "ST - Not in Melee; Cast on target")) { return true; }
            if (await S.CastGroundOnMe(SB.SigilOfFlame, C.CombatColor, !T.VengeanceConcentratedSigils && Me.IsWithinMeleeRangeOf(CurrentTarget), "ST - In Melee; Cast on self")) { return true; }
            if (await S.Cast(SB.Shear, C.CombatColor, CurrentTarget.IsWithinMeleeRangeOf(Me), "ST")) { return true; }



            return true;
        }

        public static async Task<bool> MultipleTarget()
        {
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor,
                !glaiveTossTimer.IsRunning && VS.VengeanceCombatThrowGlaive
                || (glaiveTossTimer.IsRunning && VS.VengeanceCombatThrowGlaive && glaiveTossTimer.ElapsedMilliseconds > VS.VengeanceCombatThrowGlaiveSeconds), "AoE - GlaiveTimer: " + glaiveTossTimer.ElapsedMilliseconds + "ms"))
            {
                glaiveTossTimer.Restart();
                return true;
            }

            if (await S.Cast(SB.SoulCleave, C.CombatColor,
                C.CurrentPower >= VS.VengeanceCombatSoulCleavePain,
                string.Format($"AoE: CP:{C.CurrentPower}>={VS.VengeanceCombatSoulCleavePain}")
            ))
            { return true; }

            if (await S.CastGroundOnMe(SB.InfernalStrike, C.CombatColor,
                VS.VengeanceCombatInfernalStrikeAoE
                && Me.IsWithinMeleeRangeOf(CurrentTarget)
                && S.MaxChargesAvailable(SB.InfernalStrike),
                "AoE Max Charges Available"))
            { return true; }

            if (await S.Cast(SB.FelDevastation, C.CombatColor, T.VengeanceFelDevastation, addLog: "AoE Fel Devastation")) { return true; }
            if (await S.Cast(SB.ImmolationAura, C.CombatColor, addLog: "AoE")) { return true; }
            if (await S.Cast(SB.SpiritBomb, C.CombatColor, T.VengeanceSpiritBomb && !CurrentTarget.HasAura(SB.AuraFrailty), "AoE")) { return true; }
            if (await S.Cast(SB.FelBlade, C.CombatColor, T.VengeanceFelblade, "AoE")) { return true; }
            if (await S.Cast(SB.Shear, C.CombatColor, T.VengeanceBladeTurning && Me.HasAura(SB.AuraBladeTurning) && CurrentTarget.IsWithinMeleeRangeOf(Me), "ST")) { return true; }
            if (await S.Cast(SB.SigilOfFlameTalented, C.CombatColor, CurrentTarget.IsWithinMeleeRangeOf(Me) && T.VengeanceConcentratedSigils, "AoE - Contentrated Sigils")) { return true; }
            if (await S.CastGround(SB.SigilOfFlame, C.CombatColor, !T.VengeanceConcentratedSigils && !Me.IsWithinMeleeRangeOf(CurrentTarget), "AoE - Not in Melee; Cast on target")) { return true; }
            if (await S.CastGroundOnMe(SB.SigilOfFlame, C.CombatColor, !T.VengeanceConcentratedSigils && Me.IsWithinMeleeRangeOf(CurrentTarget), "AoE - In Melee; Cast on self")) { return true; }
            if (await S.CastGround(SB.SigilOfChains, C.CombatColor, T.VengeanceSigilOfChains && VS.VengeanceCombatSigilOfChains && CurrentTarget.NearbyTargets()?.Count() >= VS.VengeanceCombatSigilOfChainsUnits))
            if (await S.Cast(SB.FieryBrand, C.CombatColor, T.VengeanceBurningAlive, addLog: "AoE has Burning Alive Talent")) { return true; }
            if (await S.Cast(SB.Shear, C.CombatColor, addLog: "AoE")) { return true; }

            return false;
        }

        #region Interrupt and Stun
        public static async Task<bool> InterruptTarget()
        {
            if (!M.VengeanceAllowInterrupt) { return false; }
            // use consume magic at 20 yards first
            //WoWUnit interruptTarget = GetInterruptTarget(20f);
            //L.debugLog(string.Format($"Interrupt target 20yd: {CurrentTarget.SafeName}"));
            if (await S.GCD(SB.ConsumeMagic, C.DefensiveColor,
                VS.VengeanceAllowInterruptConsumeMagic
                && CurrentTarget.IsValidCombatUnit()
                && CurrentTarget.ShouldInterrupt(VS.VengeanceInterruptMinimumTime, VS.VengeanceInterruptTimeLeft),
                string.Format($"Interrupt: {CurrentTarget.SafeName}, casting: {CurrentTarget.CastingSpell?.Name}")
            ))
            { return true; }


            //interruptTarget = GetInterruptTarget(30f);
            //if (interruptTarget != null)
            //{
            //    L.debugLog(string.Format($"Interrupt target 30yd: {interruptTarget.SafeName} casting: {interruptTarget.CastingSpell?.Name}"));
            //    // now look for sigil of silence
            if (await S.CastGround(SB.SigilOfSilence, C.DefensiveColor, VS.VengeanceAllowInterruptSigilOfSilence 
                && CurrentTarget.ShouldInterrupt(VS.VengeanceInterruptMinimumTime, VS.VengeanceInterruptTimeLeft),
                string.Format($"Interrupt: {CurrentTarget.SafeName}, casting: {CurrentTarget.CastingSpell?.Name}")))
            { return true; }

            // now look for sigil of misery
            if (await S.CastGround(SB.SigilOfMisery, C.DefensiveColor, VS.VengeanceAllowInterruptSigilOfMisery
                && CurrentTarget.ShouldInterrupt(VS.VengeanceInterruptMinimumTime, VS.VengeanceInterruptTimeLeft),
                string.Format($"Interrupt: {CurrentTarget.SafeName}, casting: {CurrentTarget.CastingSpell?.Name}")))
            { return true; }

            //}

            return false;
        }
        //private static WoWUnit GetInterruptTarget(double range)
        //{

        //    var units = U.activeEnemies(Me.Location, range); // get all enemies within 20 yards
        //    if (units != null)
        //    {
        //        var interruptTarget = units.Where(u => (u.IsCasting || u.IsCastingHealingSpell) && u.CanInterruptCurrentSpellCast).OrderBy(d => d.Distance).FirstOrDefault();
        //        if (interruptTarget != null)
        //        {
        //            return interruptTarget;
        //        }
        //    }

        //    return null;
        //}

        private static WoWUnit GetStunTarget(WoWUnit unit, double range)
        {
            var units = U.activeEnemies(unit.Location, 8f);
            if (units != null && units.Count() >= VS.VengeanceStunSigilOfMiseryCount)
            {
                return unit;
            }

            return null;
        }

        #endregion  
    }
}
