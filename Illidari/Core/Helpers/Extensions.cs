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

    }
}