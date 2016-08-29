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


                if (!CurrentTarget.IsWithinMeleeRangeOf(Me) && M.IS.GeneralMovement)
                {
                    //L.infoLog("Tried to pull");
                    if (await C.EnsureMeleeRange(CurrentTarget)) { return true; }
                }

                if (M.IS.GeneralFacing)
                {
                    // check to see if we need to face target
                    await C.FaceTarget(CurrentTarget);
                }

                // throw a glaive first
                if (await S.Cast(SB.ThrowGlaive, C.CombatColor, CurrentTarget.Distance <= 30)) { return true; }

                // use to engage if you have the charges to do so
                return await S.CastGround(SB.InfernalStrike, C.CombatColor, CurrentTarget.Distance <= infernalStrikeRange
                    && !CurrentTarget.IsWithinMeleeRangeOf(Me));
            }
            return true;
        }
        #endregion

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

            if (M.IS.VengeanceAllowTaunt)
            {
                await S.GCD(SB.Torment, C.CombatColor, 
                    !CurrentTarget.IsTargetingMeOrPet,
                    string.Format($"CT:{CurrentTarget.SafeName} not targeting me. Taunting!"));
            }

            if (await ActiveMitigation()) { return true; }

            if ((HK.AoEOn || U.activeEnemies(Me.Location, 8f).Count() >= 3))
            {
                if (await MultipleTarget())
                {
                    return true;
                }
            }

            // default to single target if nothing else
            return await SingleTarget();
        }

        public static async Task<bool> ActiveMitigation()
        {
            //L.infoLog(string.Format($"CP:{CurrentPain},DS:{M.IS.VengeanceAllowDemonSpikes},HP:{Me.HealthPercent},DSHP:{M.IS.VengeanceDemonSpikesHp}"),C.ItemColor);
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
            ))
            { return true; }

            if (await S.Cast(SB.FieryBrand, C.DefensiveColor, 
                M.IS.VengeanceAllowFieryBrand
                && Me.HealthPercent <= M.IS.VengeanceFieryBrandHp))
            { return true; }

            return false;
        }

        public static async Task<bool> SingleTarget()
        {
            if (await S.Cast(SB.SoulCleave, C.CombatColor,
                C.CurrentPower >= M.IS.VengeanceCombatSoulCleavePain, 
                string.Format($"ST: CP:{C.CurrentPower}>={M.IS.VengeanceCombatSoulCleavePain}")
            ))
            { return true; }
            if (await S.CastGround(SB.InfernalStrike, C.CombatColor, 
                S.MaxChargesAvailable(SB.InfernalStrike), 
                "ST Max Charges Available"))
            { return true; }
            if (await S.Cast(SB.ImmolationAura, C.CombatColor, true, "ST")) { return true; }
            if (await S.CastGround(SB.SigilOfFlame, C.CombatColor, true, "ST")) { return true; }
            if (await S.Cast(SB.Shear, C.CombatColor, true, "ST")) { return true; }


            return true;
        }

        public static async Task<bool> MultipleTarget()
        {
            if (await S.Cast(SB.SoulCleave, C.CombatColor,
                C.CurrentPower >= M.IS.VengeanceCombatSoulCleavePain, 
                string.Format($"ST: CP:{C.CurrentPower}>={M.IS.VengeanceCombatSoulCleavePain}")
            ))
            { return true; }
            if (await S.CastGround(SB.InfernalStrike, C.CombatColor, 
                S.MaxChargesAvailable(SB.InfernalStrike), 
                "AoE Max Charges Available"))
            { return true; }
            if (await S.Cast(SB.ImmolationAura, C.CombatColor, addLog: "AoE")) { return true; }
            if (await S.CastGround(SB.SigilOfFlame, C.CombatColor, addLog:"AoE")) { return true; }
            if (await S.Cast(SB.FieryBrand, C.CombatColor, T.VengeanceBurningAlive, addLog: "AoE has Burning Alive Talent"))
            if (await S.Cast(SB.Shear, C.CombatColor, addLog: "AoE")) { return true; }

            return false;
        }

    }
}
