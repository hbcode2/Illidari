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

namespace Illidari.Rotation
{
    class Vengeance
    {
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
            if (await I.UseItem(I.FindBestPrecombatFlask(), !Me.HasAura(SB.FlaskList)))
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
            //
            // use potion - Change "true" to configurable
            return await I.UseItem(I.GetItemByName("Draenic Agility Potion"), true);

        }

        #endregion

        #region Pull Logic
        public static async Task<bool> Pull()
        {
            if (CurrentTarget.IsValidCombatUnit())
            {
                await C.EnsureMeleeRange(CurrentTarget);
                await C.FaceTarget(CurrentTarget);

                if (await S.Cast(SB.ThrowGlaive, C.CombatColor, CurrentTarget.Distance <= 30)) { glaiveTossTimer.Restart(); return true; }

                // use to engage if you have the charges to do so
                if (await S.CastGround(SB.InfernalStrike, C.CombatColor,
                    M.IS.VengeanceCombatInfernalStrikePull
                    && CurrentTarget.Distance <= infernalStrikeRange
                    && !CurrentTarget.IsWithinMeleeRangeOf(Me)
                    && !M.IS.VengeancePreferPullWithFelblade
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
                    && M.IS.VengeanceCombatInfernalStrikePull
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
            if (await S.GCD(SB.Torment, C.CombatColor, !CurrentTarget.IsTargetingMeOrPet && M.IS.VengeanceAllowTaunt,
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
                CurrentTarget.Distance > 8
                && CurrentTarget.Distance <= 30))
            {
                glaiveTossTimer.Restart();
                return true;
            }
            if (await S.Cast(SB.FelBlade, C.CombatColor,
               T.VengeanceFelblade
               && !CurrentTarget.IsWithinMeleeRangeOf(Me)
               && CurrentTarget.Distance <= 15
               && CurrentTarget.Distance > 8,
               "ST Gap Closer"))
            { return true; }

            if (await S.CastGround(SB.InfernalStrike, C.CombatColor,
                M.IS.VengeanceCombatInfernalStrikeGapCloser
                && !CurrentTarget.IsWithinMeleeRangeOf(Me)
                && CurrentTarget.Distance > 8
                && CurrentTarget.Distance <= infernalStrikeRange
                && (S.GetSpellChargeInfo(SB.InfernalStrike).ChargesLeft > 0),
                "ST Gap Closer"))
            { return true; }

            return false;
        }

        public static async Task<bool> ActiveMitigation()
        {
            WoWUnit stunTarget = GetStunTarget(CurrentTarget, 8f);
            if (await S.CastGround(SB.SigilOfMisery, stunTarget, C.DefensiveColor, stunTarget != null
                && M.IS.VengeanceAllowStunSigilOfMisery))
            { return true; }

            #region Defensive Hotkey
            // demon spikes if force defensive is on
            if (await S.Cast(SB.DemonSpikes, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && M.IS.HotkeyVengeanceDefensiveDemonSpikes
                && !Me.HasAura(SB.AuraDemonSpikes),
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveDemonSpikes:{M.IS.HotkeyVengeanceDefensiveDemonSpikes.ToString()}")
            ))
            { return true; }

            if (await S.Cast(SB.MetamorphosisSpell, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && M.IS.HotkeyVengeanceDefensiveMetamorphosis,
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveMetamorphosis:{M.IS.HotkeyVengeanceDefensiveMetamorphosis.ToString()}")
            ))
            { return true; }

            if (await S.Cast(SB.SoulBarrier, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && M.IS.HotkeyVengeanceDefensiveSoulBarrier,
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveSoulBarrier:{M.IS.HotkeyVengeanceDefensiveSoulBarrier.ToString()}")
            ))
            { return true; }

            if (await S.Cast(SB.FelDevastation, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && M.IS.HotkeyVengeanceDefensiveFelDevastation,
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveFelDevastation:{M.IS.HotkeyVengeanceDefensiveFelDevastation.ToString()}")))

            if (Me.HasTankWarglaivesEquipped())
            {
                if (await S.Cast(SB.SoulCarver, C.DefensiveColor,
                    HK.VengeanceDefensiveOn
                    && M.IS.HotkeyVengeanceDefensiveSoulCarver
                    && Me.IsWithinMeleeRangeOf(CurrentTarget) // must be within melee range or won't work.
                    && Me.IsSafelyFacing(CurrentTarget.Location),
                    string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveSoulCarver:{M.IS.HotkeyVengeanceDefensiveSoulCarver.ToString()}")
                ))
                { return true; }
            }

            if (await S.Cast(SB.SoulCleave, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && M.IS.HotkeyVengeanceDefensiveSoulCleave
                && CurrentTarget.IsWithinMeleeRangeOf(Me),
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveSoulCleave:{M.IS.HotkeyVengeanceDefensiveSoulCleave.ToString()}")
            ))
            { return true; }

            if (await S.Cast(SB.EmpowerWards, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && M.IS.HotkeyVengeanceDefensiveEmpowerWards,
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveEmpowerWards:{M.IS.HotkeyVengeanceDefensiveEmpowerWards.ToString()}")
            ))
            { return true; }

            if (await S.Cast(SB.FieryBrand, C.DefensiveColor,
                HK.VengeanceDefensiveOn
                && M.IS.HotkeyVengeanceDefensiveEmpowerWards,
                string.Format($"AM: HK.VengeanceDefensiveOn:{HK.VengeanceDefensiveOn.ToString()}, HotkeyVengeanceDefensiveEmpowerWards:{M.IS.HotkeyVengeanceDefensiveEmpowerWards.ToString()}")))
            { return true; }

            #endregion

            if (await S.Cast(SB.MetamorphosisSpell, C.DefensiveColor,
                M.IS.VengeanceAllowMetamorphosis
                && Me.HealthPercent <= M.IS.VengeanceMetamorphosisHp,
                string.Format($"AM: HP:{Me.HealthPercent.ToString("F0")}<={M.IS.VengeanceMetamorphosisHp}")
            ))
            { return true; }

            // cast Demon Spikes if we have 
            if (await S.Cast(SB.DemonSpikes, C.DefensiveColor,
            M.IS.VengeanceAllowDemonSpikes
            && C.CurrentPower >= 20
            && Me.HealthPercent <= M.IS.VengeanceDemonSpikesHp
            && !Me.HasAura(SB.AuraDemonSpikes)
            && U.activeEnemies(Me.Location, 8f).Any(),
            string.Format($"AM: HP:{Me.HealthPercent.ToString("F0")}<={M.IS.VengeanceDemonSpikesHp}")
        ))
            { return true; }

            if (await S.Cast(SB.SoulBarrier, C.DefensiveColor, T.VengeanceSoulBarrier
                && M.IS.VengeanceAllowSoulBarrier
                && Me.HealthPercent <= M.IS.VengeanceSoulBarrierHp,
                string.Format($"AM: HP:{Me.HealthPercent.ToString("F0")}<={M.IS.VengeanceSoulBarrierHp}")
            ))
            { return true; }

            // make sure we have tank weapons equipped (for lower level stuff)
            if (Me.HasTankWarglaivesEquipped())
            {
                if (await S.Cast(SB.SoulCarver, C.DefensiveColor,
                    M.IS.VengeanceAllowSoulCarver
                    && Me.HealthPercent <= M.IS.VengeanceSoulCarverHp,
                    string.Format($"AM: HP:{Me.HealthPercent.ToString("F0")}<={M.IS.VengeanceSoulCarverHp}")
                ))
                { return true; }
            }

            if (await S.Cast(SB.SoulCleave, C.DefensiveColor,
                M.IS.VengeanceAllowSoulCleave
                && C.CurrentPower >= 30
                && Me.CurrentHealth <= M.IS.VengeanceSoulCleaveHp
                && CurrentTarget.IsWithinMeleeRangeOf(Me),
                string.Format($"AM: HP:{Me.HealthPercent.ToString("F0")}<={M.IS.VengeanceSoulCleaveHp}")
            ))
            { return true; }

            if (await S.Cast(SB.EmpowerWards, C.DefensiveColor,
                M.IS.VengeanceEmpowerWards
                && U.activeEnemies(Me.Location, 50f).Where(u =>
                    u.IsTargetingMeOrPet && u.IsCasting)
                .Any()
            )) { return true; }

            if (await S.Cast(SB.FieryBrand, C.DefensiveColor,
                M.IS.VengeanceAllowFieryBrand
                && Me.HealthPercent <= M.IS.VengeanceFieryBrandHp))
            { return true; }

            return false;
        }
        private static Stopwatch glaiveTossTimer = new Stopwatch();
        public static async Task<bool> SingleTarget()
        {
            if (await S.Cast(SB.ThrowGlaive, C.CombatColor,
                !glaiveTossTimer.IsRunning && M.IS.VengeanceCombatThrowGlaive
                || (glaiveTossTimer.IsRunning && M.IS.VengeanceCombatThrowGlaive && glaiveTossTimer.ElapsedMilliseconds > M.IS.VengeanceCombatThrowGlaiveSeconds), "ST"))
            {
                glaiveTossTimer.Restart();
                return true;
            }

            if (await S.Cast(SB.SoulCleave, C.CombatColor,
                C.CurrentPower >= M.IS.VengeanceCombatSoulCleavePain
                && CurrentTarget.IsWithinMeleeRangeOf(Me),
                string.Format($"ST: CP:{C.CurrentPower}>={M.IS.VengeanceCombatSoulCleavePain}")
            ))
            { return true; }

            // cast infernal strike in melee only if we have max chargets
            // it is off of the gcd, so can be cast any time.
            if (await S.CastGroundOnMe(SB.InfernalStrike, C.CombatColor,
                M.IS.VengeanceCombatInfernalStrikeSingleTarget
                && Me.IsWithinMeleeRangeOf(CurrentTarget)
                && S.MaxChargesAvailable(SB.InfernalStrike)
                && CurrentTarget.IsWithinMeleeRangeOf(Me),
                "ST Max Charges Available"))
            { return true; }

            if (await S.Cast(SB.ImmolationAura, C.CombatColor, CurrentTarget.IsWithinMeleeRangeOf(Me), "ST")) { return true; }
            if (await S.Cast(SB.FelBlade, C.CombatColor, T.VengeanceFelblade, "ST")) { return true; }
            if (await S.Cast(SB.FelEruption, C.CombatColor, T.VengeanceFelEruption && CurrentTarget.IsWithinMeleeRangeOf(Me), "ST")) { return true; }
            if (await S.Cast(SB.SpiritBomb, C.CombatColor, T.VengeanceSpiritBomb && !CurrentTarget.HasAura(SB.AuraFrailty), "ST")) { return true; }
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
                !glaiveTossTimer.IsRunning && M.IS.VengeanceCombatThrowGlaive
                || (glaiveTossTimer.IsRunning && M.IS.VengeanceCombatThrowGlaive && glaiveTossTimer.ElapsedMilliseconds > M.IS.VengeanceCombatThrowGlaiveSeconds), "AoE"))
            {
                glaiveTossTimer.Restart();
                return true;
            }

            if (await S.Cast(SB.SoulCleave, C.CombatColor,
                C.CurrentPower >= M.IS.VengeanceCombatSoulCleavePain,
                string.Format($"AoE: CP:{C.CurrentPower}>={M.IS.VengeanceCombatSoulCleavePain}")
            ))
            { return true; }

            if (await S.CastGroundOnMe(SB.InfernalStrike, C.CombatColor,
                M.IS.VengeanceCombatInfernalStrikeAoE
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
            if (await S.Cast(SB.FieryBrand, C.CombatColor, T.VengeanceBurningAlive, addLog: "AoE has Burning Alive Talent")) { return true; }
            if (await S.Cast(SB.Shear, C.CombatColor, addLog: "AoE")) { return true; }

            return false;
        }

        #region Interrupt and Stun
        public static async Task<bool> InterruptTarget()
        {
            if (!M.IS.VengeanceAllowInterrupt) { return false; }
            // use consume magic at 20 yards first
            //WoWUnit interruptTarget = GetInterruptTarget(20f);
            //L.debugLog(string.Format($"Interrupt target 20yd: {CurrentTarget.SafeName}"));
            if (await S.GCD(SB.ConsumeMagic, C.DefensiveColor,
                M.IS.VengeanceAllowInterruptConsumeMagic
                && CurrentTarget.IsValidCombatUnit()
                && (CurrentTarget.IsCasting || CurrentTarget.IsCastingHealingSpell),
                string.Format($"Interrupt: {CurrentTarget.SafeName}, casting: {CurrentTarget.CastingSpell?.Name}")
            ))
            { return true; }


            //interruptTarget = GetInterruptTarget(30f);
            //if (interruptTarget != null)
            //{
            //    L.debugLog(string.Format($"Interrupt target 30yd: {interruptTarget.SafeName} casting: {interruptTarget.CastingSpell?.Name}"));
            //    // now look for sigil of silence
            if (await S.CastGround(SB.SigilOfSilence, C.DefensiveColor, M.IS.VengeanceAllowInterruptSigilOfSilence,
                string.Format($"Interrupt: {CurrentTarget.SafeName}, casting: {CurrentTarget.CastingSpell?.Name}")))
            { return true; }

            // now look for sigil of misery
            if (await S.CastGround(SB.SigilOfMisery, C.DefensiveColor, M.IS.VengeanceAllowInterruptSigilOfMisery,
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
            if (units != null && units.Count() >= M.IS.VengeanceStunSigilOfMiseryCount)
            {
                return unit;
            }

            return null;
        }

        #endregion  
    }
}
