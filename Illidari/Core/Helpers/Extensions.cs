using Styx;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Illidari
{
    static class Extensions
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        public static string AddSpaces(this string data)
        {
            Regex r = new Regex(@"(?!^)(?=[A-Z])");
            return r.Replace(data, " ");
        }

        public static bool HasAura(this Styx.WoWInternals.WoWObjects.LocalPlayer me, IEnumerable<int> auraList)
        {
            foreach (int aura in auraList)
            {
                if (me.HasAura(aura)) { return true; }
            }
            return false;
        }
        /// <summary>
        /// Returns a null-safe integer using an int.TryParse on the string.
        /// <para>return 0 if no good otherwise, will return the data</para>
        /// </summary>
        /// <param name="intData"></param>
        /// <returns></returns>
        public static uint SafeGetInt(this string intData)
        {
            try
            {
                uint safeInt = 0;
                if (uint.TryParse(intData, out safeInt))
                {
                    return safeInt;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
        public static bool HasTankWarglaivesEquipped (this LocalPlayer player)
        {
            WoWItem mh = player.Inventory.Equipped.MainHand;
            WoWItem oh = player.Inventory.Equipped.OffHand;
            if (mh != null && oh != null)
            {
                if (mh.Name == "Aldrachi Warblades" && oh.Name == "Aldrachi Warblades")
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        ///  Checks for the auras on a specified unit. Returns true if the unit has any aura in the auraNames list.
        /// </summary>
        /// <param name="unit"> The unit to check auras for. </param>
        /// <param name="auraNames"> Aura names to be checked. </param>
        /// <returns></returns>
        public static bool HasAnyAura(this WoWUnit unit, params string[] auraNames)
        {
            var auras = unit.GetAllAuras().Where(a => !a.IsPassive);
            var hashes = new HashSet<string>(auraNames);
            return auras.Any(a => hashes.Contains(a.Name));
        }
        public static bool HasAnyTempAura(this WoWUnit unit, params string[] auraNames)
        {
            var auras = unit.GetAllAuras().Where(a => !a.IsPassive);
            var hashes = new HashSet<string>(auraNames);
            return auras.Any(a => hashes.Contains(a.Name));
        }
        public static bool IsValidCombatUnit(this WoWUnit Unit)
        {
            return Unit != null && Unit.IsValid && Unit.IsAlive && Unit.Attackable;
        }
        //public static bool IsInMeleeRange(this WoWUnit Unit, LocalPlayer me)
        //{
        //    return Unit.Distance < calculateMeleeRange(Unit, me);
        //}
        private static float calculateMeleeRange(WoWUnit Unit, LocalPlayer me)
        {
            return !me.GotTarget ? 0f : Math.Max(5f, me.CombatReach + 1.3333334f + Unit.CombatReach);
        }


        public static bool TryRemove<TKey, TValue>(
          this ConcurrentDictionary<TKey, TValue> self, TKey key)
        {
            TValue ignored;
            return self.TryRemove(key, out ignored);
        }
        public static bool IsTrivial(this WoWUnit unit)
        {
            if (unit == null)
                return false;

            if (unit.Elite)
                return unit.Level <= 10;

            return unit.Level <= 10;
        }
        public static float MeleeDistance(this WoWUnit unit)
        {
            return Me.MeleeDistance(unit);
        }

        /// <summary>
        /// get melee distance between two units
        /// </summary>
        /// <param name="unit">unit</param>
        /// <param name="other">Me if null, otherwise second unit</param>
        /// <returns></returns>
        public static float MeleeDistance(this WoWUnit unit, WoWUnit atTarget = null)
        {
            // abort if mob null
            if (unit == null)
                return 0;

            // when called as SomeUnit.SpellDistance()
            // .. convert to SomeUnit.SpellDistance(Me)
            if (atTarget == null)
                atTarget = StyxWoW.Me;

            // when called as SomeUnit.SpellDistance(Me) then
            // .. convert to Me.SpellDistance(SomeUnit)
            if (atTarget.IsMe)
            {
                atTarget = unit;
                unit = StyxWoW.Me;
            }

            // pvp, then keep it close
            if (atTarget.IsPlayer && unit.IsPlayer)
                return 3.5f;

            // return Math.Max(5f, atTarget.CombatReach + 1.3333334f + unit.CombatReach);
            return Math.Max(5f, atTarget.CombatReach + 1.3333334f);
        }
        public static bool IsTrainingDummyBoss(this WoWUnit unit)
        {
            List<string> trainingDummyNames = new List<string>();
            trainingDummyNames.Add("Imprisoned Centurion");
            trainingDummyNames.Add("Imprisoned Weaver");

            return (trainingDummyNames.Contains(unit.SafeName));
        }
        public static bool IsTrainingDummyElite(this WoWUnit unit)
        {
            List<string> trainingDummyNames = new List<string>();
            trainingDummyNames.Add("Imprisoned Centurion");
            trainingDummyNames.Add("Imprisoned Weaver");

            return (trainingDummyNames.Contains(unit.SafeName));
        }
        public static WoWPlayer ToPlayer(this WoWUnit unit)
        {
            try
            {
                var player = (WoWPlayer)unit;
                if (player != null)
                {
                    return player;
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
    }
}