using Buddy.Coroutines;
using Styx;
using Styx.CommonBot.Coroutines;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using L = Illidari.Core.Utilities.Log;

namespace Illidari.Rotation
{
    class Death
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static async Task<bool> DeathBehavor()
        {

            if (Me.IsDead)
            {
                L.infoLog(DateTime.Now.ToString("mm:ss:ffffff") + " RepopMe()", Core.Helpers.Common.InfoColor);
                Lua.DoString("RepopMe()");
            }

            if (Me.IsGhost)
            {
                if (Battlegrounds.IsInsideBattleground || Me.CurrentMap.Name == "Ashran")
                {
                    L.infoLog(DateTime.Now.ToString("mm:ss:ffffff") + " Waiting for spirit rez", Core.Helpers.Common.InfoColor);
                    await Coroutine.Sleep(1500);
                }
                else
                {
                    L.infoLog(DateTime.Now.ToString("mm:ss:ffffff") + " Move to corpse", Core.Helpers.Common.InfoColor);
                    await CommonCoroutines.MoveTo(Me.CorpsePoint);
                }
            }
            return false;
        }
    }
}
