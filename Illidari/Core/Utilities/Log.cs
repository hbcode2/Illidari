using Illidari.Core.IllidariSettings;
using Styx;
using Styx.Common;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Concurrent;
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
            if (GeneralSettings.Instance.GeneralDebug)
            {
                if (CurrentTarget != null)
                {
                    Logging.Write(logColor, "[Illidari]: {0}" + string.Format($" on {CurrentTarget.SafeName} @ {CurrentTarget.HealthPercent.ToString("F2")}% at {CurrentTarget.Distance.ToString("F2")} yds with {Helpers.Common.CurrentPower} {CurrentPowerDescription}"), Message, args);
                }
                else
                {
                    Logging.Write(logColor, "[Illidari]: {0}" + string.Format($" on [no target] with {Helpers.Common.CurrentPower} {CurrentPowerDescription}"), Message, args);
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
            if (Message == lastInformationMSG || !GeneralSettings.Instance.GeneralDebug) { return; }

            Logging.Write(logColor, "[Illidari]: {0}", Message, args);
            lastInformationMSG = Message;
        }

        protected static ConcurrentDictionary<string, DateTime> m_messages = new ConcurrentDictionary<string, DateTime>();
        public static void debugLog(string Message, params object[] args)
        {
            // check to see if we already have a message like this recently.  If so, then we will move on.
            bool alreadyHasEntry = m_messages.Any(p => p.Key.Equals(Message, StringComparison.OrdinalIgnoreCase) && ((DateTime.Now - p.Value).TotalMilliseconds < 1000));
            if (alreadyHasEntry) { return; }

            DateTime dt = DateTime.Now;
            // we don't already have an entry, so let's add the entry to the log window and set the concurrent dictionary.
            m_messages.AddOrUpdate(Message, dt, (key, rsc) => dt);

            Logging.WriteQuiet("[Illidari:Debug]: " + Message, args);

            TryRemoveOldEntries();
        }

        private static void TryRemoveOldEntries()
        {
            foreach (var m in m_messages)
            {
                // check to see if the value can be removed.
                // we will remove if more than double the time has passed since it's been updated.
                if ((DateTime.Now - m.Value).TotalMilliseconds >= (2000))
                {
                    m_messages.TryRemove(m.Key);
                }
            }
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
