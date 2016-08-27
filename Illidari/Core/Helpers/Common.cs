﻿using Buddy.Coroutines;
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
            if (target == null || Me.IsWithinMeleeRangeOf(target)) { return false; }
            L.infoLog("Getting in melee range", InfoColor);
            if (Me.IsWithinMeleeRangeOf(target) && Me.IsMoving) { return await CommonCoroutines.StopMoving(); }
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
            if (target == null || Me.IsSafelyFacing(target)) { return false; }

            if (!lastTimeFaced.IsRunning || (target.Guid == lastUnitGuid && lastTimeFaced.ElapsedMilliseconds > 500))
            L.infoLog("Not facing target; will attempt to", InfoColor);
            target.Face();
            lastUnitGuid = target.Guid;
            if (!lastTimeFaced.IsRunning) { lastTimeFaced.Start(); } else { lastTimeFaced.Restart(); }
            await Coroutine.Yield();
            return true;
        }

    }
}
