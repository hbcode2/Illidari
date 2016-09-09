using Buddy.Coroutines;
using Styx;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region [Method] - Class Redundancy
using L = Illidari.Core.Utilities.Log;
using U = Illidari.Core.Unit;
#endregion

namespace Illidari.Core
{
    class Spell
    {
        public static int lastSpellCast;
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static WoWUnit currentTarget { get { return StyxWoW.Me.CurrentTarget; } }

        #region Cooldown Detection
        /// <summary>
        /// How much time (in milliseconds) is left on the cooldown of a spell
        /// </summary>
        /// <param name="Spell">The integer based spell id</param>
        /// <returns></returns>
        public static double CooldownTimeLeft(int Spell)
        {
            try
            {
                SpellFindResults Results;
                return SpellManager.FindSpell(Spell, out Results) ? (Results.Override != null ? Results.Override.CooldownTimeLeft.TotalMilliseconds : Results.Original.CooldownTimeLeft.TotalMilliseconds) : 9999;
            }
            catch (Exception xException)
            {
                L.diagnosticsLog("Exception in cooldownTimeLeft(); ", xException);
                return 0000;
            }
        }
        public static bool MaxChargesAvailable(int Spell)
        {
            SpellChargeInfo sci = GetSpellChargeInfo(Spell);
            if (sci != null)
            {
                return (sci.ChargesLeft == sci.MaxCharges);
            }
            return false;
        }
        public static SpellChargeInfo GetSpellChargeInfo(int Spell)
        {
            try
            {
                SpellFindResults Results;
                SpellManager.FindSpell(Spell, out Results);
                return (Results.Override != null ? Results.Override.GetChargeInfo() : Results.Original.GetChargeInfo());

            }
            catch (Exception xException)
            {
                L.diagnosticsLog("Exception in cooldownTimeLeft(); ", xException);
                return null;
            }
        }
        /// <summary>
        /// Determine from SPellFindResults if the spell is on cooldown or not.
        /// </summary>
        /// <param name="Spell"></param>
        /// <returns></returns>
        public static bool OnCooldown(int Spell)
        {
            try
            {
                SpellFindResults Results;
                if (SpellManager.FindSpell(Spell, out Results)) { return Results.Override != null ? Results.Override.Cooldown : Results.Original.Cooldown; }
                return false;
            }
            catch (Exception xException)
            {
                L.diagnosticsLog("Exception in cooldownTimeLeft(); ", xException);
                return false;
            }
        }
        #endregion

        #region Spell Casting
        /// <summary>
        /// Casts a spell on yourself
        /// </summary>
        /// <param name="Spell">The spell you want to cast.</param>
        /// <param name="reqs">The requirements to cast the spell.</param>
        /// <returns></returns>
        public static async Task<bool> Buff(int Spell, System.Windows.Media.Color newColor, bool reqs = true, string addLog = "")
        {
            if (!reqs)
                return false;
            if (!SpellManager.CanCast(WoWSpell.FromId(Spell), Me, false, false, false)) //Should we check for if we are moving? *Second false
                return false;
            if (!SpellManager.Cast(Spell, Me))
                return false;
            lastSpellCast = Spell;
            L.defensiveLog("~" + WoWSpell.FromId(Spell).Name + (String.IsNullOrEmpty(addLog) || !Main.IS.GeneralDebug ? "" : " - " + addLog), newColor);
            await CommonCoroutines.SleepForLagDuration();
            return true;
        }
        /// <summary>
        /// Specifies the current target when casting spell and sleeps for lag
        /// </summary>
        /// <param name="Spell"></param>
        /// <param name="reqs"></param>
        /// <returns></returns>
        public static async Task<bool> Cast(int Spell, System.Windows.Media.Color newColor, bool reqs = true, string addLog = "")
        {

            if (!currentTarget.IsValidCombatUnit())
                return false;
            if (!reqs)
            {
                //L.combatLog("Trying to cast: " + WoWSpell.FromId(Spell).Name + (String.IsNullOrEmpty(addLog) ? "" : " - " + addLog));
                return false;
            }

            if (!SpellManager.CanCast(WoWSpell.FromId(Spell), currentTarget, false, false, false)) //Should we check for if out currentTarget is moving? *Second false
                return false;
            if (!SpellManager.Cast(Spell, currentTarget))
                return false;
            lastSpellCast = Spell;
            L.combatLog("^" + WoWSpell.FromId(Spell).Name + (String.IsNullOrEmpty(addLog) || !Main.IS.GeneralDebug ? "" : " - " + addLog), newColor);
            await CommonCoroutines.SleepForLagDuration();
            return true;
        }

        /// <summary>
        /// Does not specify a target when casting a spell and sleeps for lag
        /// </summary>
        /// <param name="Spell">The spell you wish to cast.</param>
        /// <param name="reqs">The requirements to cast the spell.</param>
        /// <returns></returns>
        public static async Task<bool> GCD(int Spell, System.Windows.Media.Color newColor, bool reqs = true, string addLog = "")
        {

            if (!reqs)
            {
                //L.combatLog("Trying to cast: " + WoWSpell.FromId(Spell).Name + (String.IsNullOrEmpty(addLog) ? "" : " - " + addLog));
                return false;
            }
            //if (SpellManager.GlobalCooldown || !SpellManager.CanCast(Spell))
            //    return false;
            if (OnCooldown(Spell))
                return false;
            if (!SpellManager.Cast(Spell))
                return false;
            lastSpellCast = Spell;
            L.combatLog("*" + WoWSpell.FromId(Spell).Name + (String.IsNullOrEmpty(addLog) || !Main.IS.GeneralDebug ? "" : " - " + addLog), newColor);
            await CommonCoroutines.SleepForLagDuration();
            return true;
        }

        /// <summary>
        /// Does not specify a target when casting a spell and sleeps for lag
        /// </summary>
        /// <param name="Spell">The spell you wish to cast.</param>
        /// <param name="reqs">The requirements to cast the spell.</param>
        /// <returns></returns>
        public static async Task<bool> GcdOnTarget(int Spell, WoWUnit target, System.Windows.Media.Color newColor, bool reqs = true, string addLog = "")
        {

            if (!reqs)
            {
                //L.combatLog("Trying to cast: " + WoWSpell.FromId(Spell).Name + (String.IsNullOrEmpty(addLog) ? "" : " - " + addLog));
                return false;
            }
            //if (SpellManager.GlobalCooldown || !SpellManager.CanCast(Spell))
            //    return false;
            if (OnCooldown(Spell))
                return false;
            if (!SpellManager.Cast(Spell,target))
                return false;
            lastSpellCast = Spell;
            L.combatLog("*" + WoWSpell.FromId(Spell).Name + (String.IsNullOrEmpty(addLog) || !Main.IS.GeneralDebug ? "" : " - " + addLog), newColor);
            await CommonCoroutines.SleepForRandomReactionTime();
            return true;
        }
        private static List<SpellBlacklist> GroundSpellBlacklist = new List<SpellBlacklist>();
        /// <summary>
        /// Casts spell on the ground at current target's location.
        /// </summary>
        /// <param name="Spell">The spell you wish to cast.</param>
        /// <param name="reqs">The requirements to cast the spell.</param>
        /// <returns></returns>
        public static async Task<bool> CastGround(int Spell, WoWUnit target, System.Windows.Media.Color newColor, bool reqs = true, string addLog = "")
        {
            foreach (var item in GroundSpellBlacklist)
            {
                if (item.SpellId == Spell && item.IsBlacklisted()) { return false; }
            }
            if (!reqs) { return false; }
            //L.combatLog("Trying to cast: " + WoWSpell.FromId(Spell).Name + (String.IsNullOrEmpty(addLog) ? "" : " - " + addLog));
            if (!target.IsValidCombatUnit()) { return false; }
            if (!SpellManager.CanCast(WoWSpell.FromId(Spell), target, false, false, false)) { return false; }

            if (await GCD(Spell, newColor, true, "CastGround")
                && !await Coroutine.Wait(1000, () => Me.CurrentPendingCursorSpell != null))
            {
                AddSpellToBlacklist(Spell);
                L.diagnosticsLog("No Cursor Detected");
                return false;
            }
            lastSpellCast = Spell;
            if (SpellManager.ClickRemoteLocation(target.Location) == false) { AddSpellToBlacklist(Spell); }
            await CommonCoroutines.SleepForLagDuration();

            return true;
        }
        private static void AddSpellToBlacklist(int spellId)
        {
            var blSpell = GroundSpellBlacklist.FirstOrDefault(bl => bl.SpellId == spellId);
            if (blSpell != null) { blSpell.AddBlacklistCounter(); } else { GroundSpellBlacklist.Add(new SpellBlacklist(spellId)); }
        }
        /// <summary>
        /// Casts spell on the ground at current target's location.
        /// </summary>
        /// <param name="Spell">The spell you wish to cast.</param>
        /// <param name="reqs">The requirements to cast the spell.</param>
        /// <returns></returns>
        public static async Task<bool> CastGround(int Spell, System.Windows.Media.Color newColor, bool reqs = true, string addLog = "")
        {

            if (!reqs) { return false; }
            if (await CastGround(Spell, currentTarget, newColor, reqs, addLog)) { return true; }
            return false;
        }
        #endregion
    }
}
