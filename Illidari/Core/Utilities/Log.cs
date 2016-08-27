using Styx;
using Styx.Common;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Illidari.Core.Utilities
{
    class Log
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static WoWUnit CurrentTarget { get { return StyxWoW.Me.CurrentTarget; } }
        private static uint CurrentPower
        {
            get
            {
                if (Me.Specialization == WoWSpec.DemonHunterHavoc)
                {
                   return Me.GetPowerInfo(WoWPowerType.Fury).Current;
                }
                return Me.GetPowerInfo(WoWPowerType.Pain).Current;
            }
        }
        private static string CurrentPowerDescription
        {
            get
            {
                if (Me.Specialization == WoWSpec.DemonHunterHavoc)
                {
                    return "Fury";
                }
                return "Pain";
            }
        }


        //public static string lastDebugMSG;
        public static string lastInformationMSG;
        public static string lastItemUseLogMSG;
        public static string lastDefensiveLogMSG;

        #region [Method] - Combat Log
        public static string lastCombatMSG;
        public static void combatLog(string Message, System.Windows.Media.Color logColor, params object[] args)
        {
            if (Message == lastCombatMSG)
                return;
            if (Main.IS.GeneralDebug)
            {
                if (CurrentTarget != null)
                {
                    Logging.Write(logColor, "[Illidari]: {0}" + string.Format($" on {CurrentTarget.SafeName} @ {CurrentTarget.HealthPercent.ToString("F2")}% at {CurrentTarget.Distance.ToString("F2")} yds with {CurrentPower} {CurrentPowerDescription}"), Message, args);
                }
                else
                {
                    Logging.Write(logColor, "[Illidari]: {0}" + string.Format($" on [no target] with {CurrentPower} {CurrentPowerDescription}"), Message, args);
                }
            }
            else
            {
                Logging.Write(logColor, "[Illidari]: {0}", Message, args);
            }
            lastCombatMSG = Message;
        }
        #endregion

        #region ItemUse Log
        public static void useItemLog(string Message, System.Windows.Media.Color logColor, params object[] args)
        {
            if (Message == lastItemUseLogMSG) { return; }

            Logging.Write(logColor, "[Illidari]: {0}", Message, args);
            lastItemUseLogMSG = Message;
        }
        #endregion

        #region Defensive Log
        public static void defensiveLog(string Message, System.Windows.Media.Color logColor, params object[] args)
        {
            if (Message == lastDefensiveLogMSG) { return; }

            Logging.Write(logColor, "[Illidari]: {0}", Message, args);
            lastDefensiveLogMSG = Message;
        }
        #endregion



        #region
        public static void pullMobLog(string Message, System.Windows.Media.Color logColor, params object[] args)
        {
            if (Message == lastInformationMSG) { return; }

            Logging.Write(logColor, "[Illidari]: {0}", Message, args);
            lastInformationMSG = Message;
        }
        #endregion

        #region Information Log

        public static void infoLog(string Message, System.Windows.Media.Color logColor, params object[] args)
        {
            if (Message == lastInformationMSG || !Main.IS.GeneralDebug) { return; }

            Logging.Write(logColor, "[Illidari]: {0}", Message, args);
            lastInformationMSG = Message;
        }

        #endregion

        #region [Method] - Diagnostics Log
        public static void diagnosticsLog(string Message, params object[] args)
        {
            if (Message == null)
                return;
            Logging.WriteDiagnostic(Colors.Firebrick, "[Error] {0}", String.Format(Message, args));
        }
        #endregion
    }
}
