using Styx;
using Styx.CommonBot.Coroutines;
using Styx.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

#region [Method] - Class Redundancy
using L = Illidari.Core.Utilities.Log;
using SB = Illidari.Core.Helpers.Spell_Book;
#endregion

namespace Illidari.Core
{
    class Unit
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static WoWUnit currentTarget { get { return StyxWoW.Me.CurrentTarget; } }

        #region Active Enemies
        public static IEnumerable<WoWUnit> activeEnemies(Vector3 fromLocation, double Range)
        {
            var Hostile = enemyCount;
            return Hostile != null ? Hostile.Where(x => x.DistanceSqr <= Range * Range) : null;
        }
        public static IEnumerable<WoWUnit> activeEnemiesToTaunt(Vector3 fromLocation, double Range)
        {
            var Hostile = enemiesToTaunt;
            return Hostile != null ? Hostile.Where(x => x.DistanceSqr <= Range * Range) : null;
        }
        private static List<WoWUnit> enemyCount { get; set; }
        private static List<WoWUnit> enemiesToTaunt { get; set; }
        public static void enemyAnnex(double Range)
        {
            enemyCount.Clear();
            foreach (var u in surroundingEnemies())
            {
                if (u == null || !u.IsValid)//<~ Use isUnitValid(u, Range) instead of the frist five of these first calls?
                    continue;
                if (!u.IsAlive || u.DistanceSqr > Range * Range)
                    continue;
                if (!u.Attackable || !u.CanSelect)
                    continue;
                if (u.IsFriendly)
                    continue;
                if (u.IsNonCombatPet && u.IsCritter)
                    continue;

                // make sure the unit is targeting something we care about.
                if (u.IsTargetingMeOrPet || u.IsTargetingMyPartyMember || u.IsTargetingMyRaidMember)
                {
                    enemyCount.Add(u);
                }
                
            }
        }
        public static void enemiesToTauntAnnex(double Range)
        {
            enemiesToTaunt.Clear();
            foreach (var u in surroundingEnemies())
            {
                if (u == null || !u.IsValid)//<~ Use isUnitValid(u, Range) instead of the frist five of these first calls?
                    continue;
                if (!u.IsAlive || u.DistanceSqr > Range * Range)
                    continue;
                if (!u.Attackable || !u.CanSelect)
                    continue;
                if (u.IsFriendly)
                    continue;
                if (u.IsNonCombatPet && u.IsCritter)
                    continue;
                if (!u.IsTargetingMeOrPet && (u.IsTargetingMyPartyMember || u.IsTargetingMyRaidMember))
                {
                    enemiesToTaunt.Add(u);
                }
            }
        }
        private static IEnumerable<WoWUnit> surroundingEnemies() { return ObjectManager.GetObjectsOfTypeFast<WoWUnit>(); }
        static Unit() { enemyCount = new List<WoWUnit>(); enemiesToTaunt = new List<WoWUnit>(); }
        #endregion

        #region Aura Detection
        public static bool auraExists(WoWUnit Unit, int auraID, bool isMyAura = false)
        {
            try
            {
                if (Unit == null || !Unit.IsValid)
                    return false;
                WoWAura Aura = isMyAura ? Unit.GetAllAuras().FirstOrDefault(A => A.SpellId == auraID && A.CreatorGuid == Me.Guid) : Unit.GetAllAuras().FirstOrDefault(A => A.SpellId == auraID);
                return Aura != null;
            }
            catch (Exception xException)
            {
                L.diagnosticsLog("Exception in auraExists(); ", xException);
                return false;
            }
        }
        public static uint auraStacks(WoWUnit Unit, int auraID, bool isMyAura = false)
        {
            try
            {
                if (Unit == null || !Unit.IsValid)
                    return 0;
                WoWAura Aura = isMyAura ? Unit.GetAllAuras().FirstOrDefault(A => A.SpellId == auraID && A.CreatorGuid == Me.Guid) : Unit.GetAllAuras().FirstOrDefault(A => A.SpellId == auraID);
                return Aura != null ? Aura.StackCount : 0;
            }
            catch (Exception xException)
            {
                L.diagnosticsLog("Exception in auraStacks(); ", xException);
                return 0;
            }
        }
        public static double auraTimeLeft(WoWUnit Unit, int auraID, bool isMyAura = false)
        {
            try
            {
                if (Unit == null || !Unit.IsValid)
                    return 0;
                WoWAura Aura = isMyAura ? Unit.GetAllAuras().FirstOrDefault(A => A.SpellId == auraID && A.CreatorGuid == Me.Guid) : Unit.GetAllAuras().FirstOrDefault(A => A.SpellId == auraID);
                return Aura != null ? Aura.TimeLeft.TotalMilliseconds : 0;
            }
            catch (Exception xException)
            {
                L.diagnosticsLog("Exception in auraExists(); ", xException);
                return 9999;
            }
        }
        #endregion

       

        #region Unit Evaluation


        //public static async Task<MoveResult> RangeCheck(int MobDistance, int DistanceNeeded)
        //{
        //    if (Me.CurrentTarget != null)
        //    {
        //        if (Me.CurrentTarget.Distance > MobDistance && !Me.IsMoving)
        //        {
        //            Navigator.MoveTo()
        //           return await CommonCoroutines.MoveTo(Me.CurrentTarget.Location., MyDistance);
        //        }
        //    }
        //    return MoveResult.Failed;
        //}
        #endregion
    }
}
