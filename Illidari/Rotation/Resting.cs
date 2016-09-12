using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Styx.WoWInternals.WoWObjects;

using I = Illidari.Core.Item;
using M = Illidari.Main;
using C = Illidari.Core.Helpers.Common;
using Styx;
using System.Diagnostics;
using Styx.CommonBot.Coroutines;

namespace Illidari.Rotation
{
    public class Resting
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        private static Stopwatch minimumRestTime = new Stopwatch();
        public static async Task<bool> RestBehavior()
        {
            while (Me.HealthPercent < M.IS.GeneralRestingRestHp || minimumRestTime.ElapsedMilliseconds >= (M.IS.GeneralRestingRestHpSecondsTimeout * 1000))
            {
                if (Me.IsCasting || Me.IsChanneling || C.IsEatingOrDrinking) { return false; }
                WoWItem bandage = I.FindBestBandage();
                if (bandage != null)
                {
                    await I.UseItem(bandage, M.IS.GeneralRestingUseBandages);
                    await Coroutine.Sleep(500);
                }

                WoWItem food = I.FindBestFood();
                if (food != null)
                {
                    await I.UseItem(food, true);
                    await Coroutine.Sleep(500);
                }
            }
            return false;
        }

    }
}
