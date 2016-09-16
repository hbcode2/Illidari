using Styx;
using Styx.CommonBot.POI;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Styx.Common;
using System.Windows.Media;

using L = Illidari.Core.Utilities.Log;
using C = Illidari.Core.Helpers.Common;
using Styx.Helpers;
using Styx.Pathing;

namespace Illidari.Core
{
    /// <summary>
    /// <para>provides ability to find a safe spot nearby that is away from
    /// enemies.  properties can be modified to customize search</para>
    /// </summary>
    public class SafeArea
    {
        public enum Direction
        {
            None,
            Backwards,
            Frontwards
        }

        public const float ONE_DEGREE_AS_RADIAN = 0.0174532925f;

        /// <summary><para>Minimum distance away from character to find safe place</para></summary>
        public float MinScanDistance { get; set; }
        /// <summary><para>Maximum distance away from character to find safe place</para></summary>
        public float MaxScanDistance { get; set; }
        /// <summary><para>Increment added each repetition to MinScanDistance until MaxScanDistance reached.</para></summary>
        public float IncrementScanDistance { get; set; }
        /// <summary>Direction to favor for disengage</summary>
        public Direction PreferredDirection { get; set; }
        /// <summary><para>Number of evenly spaced checks around perimter.  36 would yield 36 checks around perimeter spaced 10 degrees apart.</para></summary>
        public int RaysToCheck { get; set; }
        /// <summary>Minimum distance from safe spot to nearest enemy</summary>
        public int MinSafeDistance { get; set; }
        /// <summary>Range to keep to LineOfSightMob if CheckRangeToLineOfSightMob</summary>
        public float RangeToLineOfSightMob { get; set; }
        /// <summary>Unit we are trying to get away from</summary>
        public WoWUnit MobToRunFrom { get; set; }
        /// <summary>Unit we need to be able to attack from destination</summary>
        public WoWUnit LineOfSightMob { get; set; }
        /// <summary>Require line of sight from origin to safe location</summary>
        public bool CheckLineOfSightToSafeLocation { get; set; }
        /// <summary>Require spell line of sight from safe location to mob</summary>
        public bool CheckSpellLineOfSightToMob { get; set; }
        /// <summary>Require safe location to be within 40 yds of current target</summary>
        public bool CheckRangeToLineOfSightMob { get; set; }
        /// <summary>Require direct line to destination.  Uses MeshTraceLine rather than CanFullyNavigate</summary>
        public bool DirectPathOnly { get; set; }

        /// <summary>Select best navigable point available</summary>
        public bool ChooseSafestAvailable { get; set; }

        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        public SafeArea()
        {
            MinScanDistance = 10;               // minimum distance to scan for safe spot
            MaxScanDistance = 30;               // maximum distance to scan for safe spot
            IncrementScanDistance = 10;             // distance increment for each iteration of safe spot check 
            RaysToCheck = 36;                   // number of checks within 360 degrees (36 = every 10 degrees)
            MinSafeDistance = 15;                       // radius of the minimal safe area at safe spot
            MobToRunFrom = NearestEnemyMobAttackingMe;  // trying to get away from this guy
            LineOfSightMob = Me.CurrentTarget;          // safe spot must have line of sight to this mob
            DirectPathOnly = false;                 // allow CanFullyNavigate to walk around obstacles 

            PreferredDirection = Direction.Backwards;

            CheckLineOfSightToSafeLocation = true;
            CheckSpellLineOfSightToMob = LineOfSightMob != null;

            RangeToLineOfSightMob = 0;
            CheckRangeToLineOfSightMob = false;
            /*
                        if (Me.Class == WoWClass.Priest || Me.Class == WoWClass.Warlock || Me.Class == WoWClass.Mage || Me.Class == WoWClass.Hunter)
                            RangeToLineOfSightMob = Styx.Helpers.CharacterSettings.Instance.PullDistance;
                        else if (SpellManager.HasSpell("Meditation"))
                            RangeToLineOfSightMob = Styx.Helpers.CharacterSettings.Instance.PullDistance;
                        else if (SpellManager.HasSpell("Starsurge"))
                            RangeToLineOfSightMob = Styx.Helpers.CharacterSettings.Instance.PullDistance;
                        else if (SpellManager.HasSpell("Thunderstorm"))
                            RangeToLineOfSightMob = Styx.Helpers.CharacterSettings.Instance.PullDistance;

                        CheckRangeToLineOfSightMob = Me.GotTarget() && RangeToLineOfSightMob > 0;
            */
            ChooseSafestAvailable = true;
        }

        /// <summary>
        /// Does minimal testing to see if a WoWUnit should be treated as an enemy.  Avoids 
        /// searching lists (such as TargetList)
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static bool IsEnemy(WoWUnit u)
        {
            if (u == null || !u.CanSelect || !u.Attackable || !u.IsAlive || u.IsNonCombatPet)
                return false;

            if (BotPoi.Current.Guid == u.Guid && BotPoi.Current.Type == PoiType.Kill)
                return true;

            if (u.IsCritter && u.ThreatInfo.ThreatValue == 0)
                return true;

            if (!u.IsPlayer)
                return u.IsHostile || u.Aggro || u.PetAggro;

            WoWPlayer p = u.ToPlayer();
            /* // not supported currently
                        if (Battlegrounds.IsInsideBattleground)
                            return p.BattlefieldArenaFaction != Me.BattlefieldArenaFaction;
            */
            return p.IsHostile; // || p.IsHorde != Me.IsHorde;
        }

        public static IEnumerable<WoWUnit> AllEnemyMobs
        {
            get
            {
                // return ObjectManager.ObjectList.Where(o => o is WoWUnit && IsEnemy(o.ToUnit())).Select(o => o.ToUnit()).ToList();
                return ObjectManager.ObjectList.Where(o => o is WoWUnit && o.ToUnit().IsValidCombatUnit()).Select(o => o.ToUnit()).ToList();

            }
        }

        public List<Vector3> AllEnemyMobLocationsToCheck
        {
            get
            {
                return (from u in AllEnemyMobs
                        where u != MobToRunFrom && u != LineOfSightMob && u.Distance2DSqr < (65 * 65)
                        select u.Location).ToList();
            }
        }

        public static IEnumerable<WoWUnit> AllEnemyMobsAttackingMe
        {
            get
            {
                return AllEnemyMobs
                    .Where(u => u.Combat
                        && (u.CurrentTargetGuid == Me.Guid || u.Aggro)
                        && !u.IsPet
                        && !u.IsTrivial()
                        );
            }
        }

        public static WoWUnit NearestEnemyMobAttackingMe
        {
            get
            {
                return AllEnemyMobsAttackingMe.OrderBy(uu => uu.Distance2DSqr).FirstOrDefault();
            }
        }

        public static Vector3 NearestMobLoc(Vector3 p, IEnumerable<Vector3> mobLocs)
        {
            if (!mobLocs.Any())
                return Vector3.Zero;

            return mobLocs.OrderBy(u => u.Distance2DSquared(p)).First();
        }

        /// <summary>
        /// locates safe point away from enemies
        /// </summary>
        /// <param name="minSafeDist">min distance to be safe</param>
        /// <returns></returns>
        public Vector3 FindLocation()
        {
            return FindLocation(Me.GetTraceLinePos());
        }

        public Vector3 FindLocation(Vector3 ptOrigin)
        {
            DateTime startFind = DateTime.UtcNow;
            int countPointsChecked = 0;
            int countFailDiff = 0;
            int countFailTrace = 0;
            int countFailToPointNav = 0;
            int countFailRange = 0;
            int countFailSafe = 0;
            int countFailToPointLoS = 0;
            int countFailToMobLoS = 0;
            TimeSpan spanTrace = TimeSpan.Zero;
            TimeSpan spanNav = TimeSpan.Zero;
            double furthestNearMobDistSqr = 0f;
            Vector3 ptFurthest = Vector3.Zero;
            float facingFurthest = 0f;

            bool reallyCheckRangeToLineOfSightMob = CheckRangeToLineOfSightMob && Me.GotTarget;
            Vector3 ptAdjOrigin = ptOrigin;
            // ptAdjOrigin.Z += 1f;   // comment out origin adjustment since using GetTraceLinePos()

            Vector3 ptDestination = new Vector3();
            List<Vector3> mobLocations = new List<Vector3>();
            float arcIncrement = ((float)Math.PI * 2) / RaysToCheck;

            mobLocations = AllEnemyMobLocationsToCheck;
            double minSafeDistSqr = MinSafeDistance * MinSafeDistance;
            
            float baseDestinationFacing;
            if (PreferredDirection == Direction.None && MobToRunFrom != null)
                baseDestinationFacing = Styx.Helpers.WoWMathHelper.CalculateNeededFacing(MobToRunFrom.Location, Me.Location);
            else if (PreferredDirection == Direction.Frontwards)
                baseDestinationFacing = Me.RenderFacing;
            else // if (PreferredDirection == Disengage.Direction.Backwards)
                baseDestinationFacing = Me.RenderFacing + (float)Math.PI;

            L.debugLog("SafeArea: facing {0:F0} degrees, looking for safespot towards {1:F0} degrees", C.DefensiveColor,
                WoWMathHelper.RadiansToDegrees(Me.RenderFacing),
                WoWMathHelper.RadiansToDegrees(baseDestinationFacing)
                );

            for (int arcIndex = 0; arcIndex < RaysToCheck; arcIndex++)
            {
                // rather than tracing around the circle, toggle between clockwise and counter clockwise for each test
                // .. so we favor a position furthest away from mob
                float checkFacing = baseDestinationFacing;
                if ((arcIndex & 1) == 0)
                    checkFacing += arcIncrement * (arcIndex >> 1);
                else
                    checkFacing -= arcIncrement * ((arcIndex >> 1) + 1);

                checkFacing = WoWMathHelper.NormalizeRadian(checkFacing);
                for (float distFromOrigin = MinScanDistance; distFromOrigin <= MaxScanDistance; distFromOrigin += IncrementScanDistance)
                {
                    countPointsChecked++;

                    ptDestination = ptOrigin.RayCast(checkFacing, distFromOrigin);

                    L.debugLog("SafeArea: checking {0:F1} degrees at {1:F1} yds", WoWMathHelper.RadiansToDegrees(checkFacing), distFromOrigin);

                    DateTime start = DateTime.UtcNow;
                    bool failTrace = MeshTraceline(Me.Location, ptDestination);
                    spanTrace += DateTime.UtcNow - start;

                    bool failNav;
                    if (DirectPathOnly)
                    {
                        failNav = failTrace;
                        spanNav = spanTrace;
                    }
                    else
                    {
                        start = DateTime.UtcNow;
                        failNav = false;
                        //failNav = !Navigator.CanNavigateFully(Me.Location, ptDestination);
                        spanNav += DateTime.UtcNow - start;
                    }

                    if (failTrace)
                        countFailTrace++;

                    if (failTrace != failNav)
                        countFailDiff++;

                    if (failNav)
                    {
                        // L.debugLog( Color.Cyan, "Safe Location failed navigation check for degrees={0:F1} dist={1:F1}", RadiansToDegrees(checkFacing), distFromOrigin);
                        countFailToPointNav++;
                        continue;
                    }

                    Vector3 ptNearest = NearestMobLoc(ptDestination, mobLocations);
                    if (ptNearest == Vector3.Zero)
                    {
                        if (furthestNearMobDistSqr < minSafeDistSqr)
                        {
                            furthestNearMobDistSqr = minSafeDistSqr;
                            ptFurthest = ptDestination;     // set best available if others fail
                            facingFurthest = checkFacing;
                        }
                    }
                    else
                    {
                        double mobDistSqr = ptDestination.Distance2DSquared(ptNearest);
                        if (furthestNearMobDistSqr < mobDistSqr)
                        {
                            furthestNearMobDistSqr = mobDistSqr;
                            ptFurthest = ptDestination;     // set best available if others fail
                            facingFurthest = checkFacing;
                        }
                        if (mobDistSqr <= minSafeDistSqr)
                        {
                            countFailSafe++;
                            continue;
                        }
                    }

                    if (reallyCheckRangeToLineOfSightMob && RangeToLineOfSightMob < ptDestination.Distance(LineOfSightMob.Location) - LineOfSightMob.MeleeDistance())
                    {
                        countFailRange++;
                        continue;
                    }

                    if (CheckLineOfSightToSafeLocation)
                    {
                        Vector3 ptAdjDest = ptDestination;
                        ptAdjDest.Z += 1f;
                        if (!Styx.WoWInternals.World.GameWorld.IsInLineOfSight(ptAdjOrigin, ptAdjDest))
                        {
                            // L.debugLog( Color.Cyan, "Mob-free location failed line of sight check for degrees={0:F1} dist={1:F1}", degreesFrom, distFromOrigin);
                            countFailToPointLoS++;
                            continue;
                        }
                    }

                    if (CheckSpellLineOfSightToMob && LineOfSightMob != null)
                    {
                        if (!Styx.WoWInternals.World.GameWorld.IsInLineOfSpellSight(ptDestination, LineOfSightMob.GetTraceLinePos()))
                        {
                            if (!Styx.WoWInternals.World.GameWorld.IsInLineOfSight(ptDestination, LineOfSightMob.GetTraceLinePos()))
                            {
                                // L.debugLog( Color.Cyan, "Mob-free location failed line of sight check for degrees={0:F1} dist={1:F1}", degreesFrom, distFromOrigin);
                                countFailToMobLoS++;
                                continue;
                            }
                        }
                    }

                    L.debugLog("SafeArea: Found mob-free location ({0:F1} yd radius) at degrees={1:F1} dist={2:F1} on point check# {3} at {4}, {5}, {6}", MinSafeDistance, WoWMathHelper.RadiansToDegrees(checkFacing), distFromOrigin, countPointsChecked, ptDestination.X, ptDestination.Y, ptDestination.Z);
                    L.debugLog("SafeArea: processing took {0:F0} ms", (DateTime.UtcNow - startFind).TotalMilliseconds);
                    L.debugLog("SafeArea: meshtrace took {0:F0} ms / fullynav took {1:F0} ms", spanTrace.TotalMilliseconds, spanNav.TotalMilliseconds);
                    L.debugLog("SafeArea: stats for ({0:F1} yd radius) found within {1:F1} yds ({2} checked, {3} nav, {4} not safe, {5} range, {6} pt los, {7} mob los, {8} mesh trace)", MinSafeDistance, MaxScanDistance, countPointsChecked, countFailToPointNav, countFailSafe, countFailRange, countFailToPointLoS, countFailToMobLoS, countFailTrace);
                    return ptDestination;
                }
            }

            L.debugLog("SafeArea: No mob-free location ({0:F1} yd radius) found within {1:F1} yds ({2} checked, {3} nav, {4} not safe, {5} range, {6} pt los, {7} mob los, {8} mesh trace)", MinSafeDistance, MaxScanDistance, countPointsChecked, countFailToPointNav, countFailSafe, countFailRange, countFailToPointLoS, countFailToMobLoS, countFailTrace);
            if (ChooseSafestAvailable && ptFurthest != Vector3.Zero)
            {
                L.debugLog("SafeArea: choosing best available spot in {0:F1} yd radius where closest mob is {1:F1} yds", MinSafeDistance, Math.Sqrt(furthestNearMobDistSqr));
                L.debugLog("SafeArea: processing took {0:F0} ms", (DateTime.UtcNow - startFind).TotalMilliseconds);
                L.debugLog("SafeArea: meshtrace took {0:F0} ms / fullynav took {1:F0} ms", spanTrace.TotalMilliseconds, spanNav.TotalMilliseconds);
                return ChooseSafestAvailable ? ptFurthest : Vector3.Zero;
            }

            L.debugLog("SafeArea: processing took {0:F0} ms", (DateTime.UtcNow - startFind).TotalMilliseconds);
            L.debugLog("SafeArea: meshtrace took {0:F0} ms / fullynav took {1:F0} ms", spanTrace.TotalMilliseconds, spanNav.TotalMilliseconds);
            return Vector3.Zero;
        }

        /// <summary>
        /// determines if there is an obstruction in a straight line from source point to destination.
        /// </summary>
        /// <param name="src">origin</param>
        /// <param name="dest">destination</param>
        /// <returns>true if obstruction or cannot determine, false if safe to walk</returns>
        public static bool MeshTraceline(Vector3 src, Vector3 dest)
        {
            Vector3 hit;
            bool? pathObstructed = MeshTraceline(src, dest, out hit);
            return pathObstructed == null || (bool)pathObstructed;
        }
        /// <summary>
        /// Checks if obstruction exists in walkable ray on the surface of the mesh from <c>Vector3Src</c> to <c>Vector3Dest</c>.
        /// Return value indicates whether a wall (disjointed polygon edge) was encountered
        /// </summary>
        /// <param name="Vector3Src"></param>
        /// <param name="Vector3Dest"></param>
        /// <param name="hitLocation">
        /// The point where a wall (disjointed polygon edge) was encountered if any, otherwise Vector3.Empty.
        /// The hit calculation is done in 2d so the Z coord will not be accurate; It is an interpolation between <c>Vector3Src</c>'s and <c>Vector3Dest</c>'s Z coords
        /// </param>
        /// <returns>Returns null if a result cannot be determined e.g <c>Vector3Dest</c> is not on mesh, True if a wall (disjointed polygon edge) is encountered otherwise false</returns>
        public static bool? MeshTraceline(Vector3 Vector3Src, Vector3 Vector3Dest, out Vector3 hitLocation)
        {
            hitLocation = Vector3.Zero;
            return null;

        }
        }
    }
