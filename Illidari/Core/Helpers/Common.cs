using Buddy.Coroutines;
using Styx;
using Styx.CommonBot.Coroutines;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using L = Illidari.Core.Utilities.Log;
using HK = Illidari.Core.Managers.Hotkeys;

namespace Illidari.Core.Helpers
{
    public class Common
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static System.Windows.Media.Color CombatColor { get { return System.Windows.Media.Colors.Fuchsia; } }
        public static System.Windows.Media.Color InfoColor { get { return System.Windows.Media.Colors.WhiteSmoke; } }
        public static System.Windows.Media.Color TargetColor { get { return System.Windows.Media.Colors.Teal; } }
        public static System.Windows.Media.Color DefensiveColor { get { return System.Windows.Media.Colors.LightGreen; } }
        public static System.Windows.Media.Color ItemColor { get { return System.Windows.Media.Colors.Gold; } }
        public static async Task<bool> EnsureMeleeRange(WoWUnit target)
        {
            if (target == null || Me.IsWithinMeleeRangeOf(target) || !target.IsValidCombatUnit() || HK.RotationOnlyOn || !Main.IS.GeneralMovement) { return false; }
            L.infoLog("Getting in melee range", InfoColor);
            if (Me.IsWithinMeleeRangeOf(target)) { return await CommonCoroutines.StopMoving(); }
            if (!Me.IsWithinMeleeRangeOf(target))
            {
                await CommonCoroutines.MoveTo(target.RelativeLocation, target.SafeName);
            }
           return true;
        }

        private static WoWGuid lastUnitGuid = WoWGuid.Empty;
        private static Stopwatch lastTimeFaced = new Stopwatch();
        public static async Task<bool> FaceTarget(WoWUnit target)
        {
            if (target == null || Me.IsSafelyFacing(target) || !target.IsValidCombatUnit() || HK.RotationOnlyOn || !Main.IS.GeneralFacing) { return false; }

            if (!lastTimeFaced.IsRunning || (target.Guid == lastUnitGuid && lastTimeFaced.ElapsedMilliseconds > 500))
            L.infoLog("Not facing target; will attempt to", InfoColor);
            target.Face();
            lastUnitGuid = target.Guid;
            if (Me.IsWithinMeleeRangeOf(target) && Me.IsMoving && Main.IS.GeneralMovement && !Managers.Hotkeys.RotationOnlyOn) { return await CommonCoroutines.StopMoving(); }
            if (!lastTimeFaced.IsRunning) { lastTimeFaced.Start(); } else { lastTimeFaced.Restart(); }
            await Coroutine.Yield();
            return true;
        }
        public static bool IsEatingOrDrinking
        {
            get
            {
                return Me.HasAnyAura("Drink", "Food", "Refreshment", "Cannibalize");
            }
        }
        #region Cache
        public static uint CurrentPower;
        public static bool shouldBarrier;
        public static bool shouldBlock;

        public static void Cache()
        {
            //shouldBlock = (!auraExists(Me, SB.auraDemoralizingShout, true) || !auraExists(Me, SB.auraShieldWall, true) || !auraExists(Me, SB.auraLastStand) || !auraExists(Me, SB.auraEnragedRegeneration, true) || !auraExists(Me, SB.auraShieldBlock, true));
            //shouldBarrier = WoWSpell.FromId(SB.spellShieldBlock).GetChargeInfo().ChargesLeft < 1 && WoWSpell.FromId(SB.spellShieldBlock).GetChargeInfo().TimeUntilNextCharge.TotalMilliseconds < 9000;
            CurrentPower = luaGetPower();
        }


        public static uint luaGetPower()
        {
            try
            {
                using (StyxWoW.Memory.AcquireFrame()) { return Lua.GetReturnVal<uint>("return UnitPower(\"player\");", 0); }
            }
            catch (Exception xException)
            {
                L.diagnosticsLog("Exception in playerPower(); ", xException);
                return Me.CurrentPower;
            }
        }
        #endregion
    }
}
