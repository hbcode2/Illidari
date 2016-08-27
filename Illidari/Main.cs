using CommonBehaviors.Actions;
using Styx;
using Styx.Common;
using Styx.CommonBot.Routines;
using Styx.TreeSharp;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

#region Class Redundancy
using HK = Illidari.Core.Managers.Hotkeys;
using H = Illidari.Rotation.Havoc;
using V = Illidari.Rotation.Vengeance;
using U = Illidari.Core.Unit;
using TM = Illidari.Core.Managers.TalentManager;
using D = Illidari.Rotation.Death;
using L = Illidari.Core.Utilities.Log;
using System.Diagnostics;
#endregion

namespace Illidari
{
    public class Main : CombatRoutine
    {
        public static Illidari.Core.IllidariSettings.IllidariSettings IS = new Core.IllidariSettings.IllidariSettings();
        private static readonly Version version = new Version(08, 26, 2016);
        public override string Name { get { return string.Format($"{CRName} v{version}"); } }
        public override WoWClass Class { get { return WoWClass.DemonHunter; } }
        public static string CRName { get { return "Illidari"; } }
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        #region Implementations
        public override Composite PreCombatBuffBehavior
        {
            get
            {
                if (Me.Specialization == WoWSpec.DemonHunterHavoc)
                {
                    return new ActionRunCoroutine(ctx => H.PreCombatBuffing());
                }
                return new ActionRunCoroutine(ctx => V.PreCombatBuffing());
            }
        }
        public override Composite CombatBuffBehavior
        {
            get
            {
                if (Me.Specialization == WoWSpec.DemonHunterHavoc)
                {
                    return new ActionRunCoroutine(ctx => H.CombatBuffing());
                }
                return new ActionRunCoroutine(ctx => V.CombatBuffing());
            }
        }
        public override Composite CombatBehavior
        {
            get
            {
                if (Me.Specialization == WoWSpec.DemonHunterHavoc)
                {
                    return new ActionRunCoroutine(ctx => H.RotationSelector());
                }

                return new ActionRunCoroutine(ctx => V.RotationSelector());

            }
        }
        public override Composite PullBehavior
        {
            get
            {
                if (Me.Specialization == WoWSpec.DemonHunterHavoc)
                {
                    return new ActionRunCoroutine(ctx => H.Pull());
                }
                return new ActionRunCoroutine(ctx => V.Pull());
            }
        }
        public override Composite DeathBehavior { get { return new ActionRunCoroutine(ctx => D.DeathBehavor()); } }
        public override bool NeedDeath { get { return Me.IsDead; } }
        public override bool NeedRest
        {
            get
            {
                if (Me.Specialization == WoWSpec.DemonHunterHavoc)
                {
                    return Me.HealthPercent < 50;
                }
                if (Me.Specialization == WoWSpec.DemonHunterVengeance)
                {
                    return Me.HealthPercent < 50;
                }
                return false;
            }
        }
        public override bool NeedCombatBuffs { get { return base.NeedCombatBuffs; } }

        #region Hidden Overrides
        public override void Initialize()
        {
            Logging.Write(Colors.Fuchsia, "Hello {0}", Me.SafeName);
            Logging.Write("");
            Logging.Write(Colors.Fuchsia, "For optimal performance, please use: Enyo");
            Logging.Write("");
            Logging.Write(Colors.Fuchsia, "Current Version:");
            Logging.Write(Colors.Fuchsia, "-- v" + version + " --");
            Logging.Write(Colors.Fuchsia, "-- by SpeshulK926 --");
            Logging.Write(Colors.Fuchsia, "-- A Demon Hunter's Combat Routine --");
            HK.registerHotkeys();
            TM.initTalents();

        }
        public override bool WantButton { get { return true; } }
        public override void OnButtonPress() { IllidariSettingsForm setForm = new IllidariSettingsForm(); setForm.Show(); }
        public override void ShutDown() { HK.removeHotkeys(); }
        #endregion

        #region Pulse
        //private static Stopwatch _check = new Stopwatch();
        public override void Pulse()
        {

            if (!StyxWoW.IsInGame || !StyxWoW.IsInWorld || Me == null || !Me.IsValid || !Me.IsAlive || Me.Mounted)
                return;
            if (!Me.Combat)
                return;
            //U.Cache();
            
            U.enemyAnnex(50f);
            if (Me.Specialization == WoWSpec.DemonHunterVengeance && IS.VengeanceAllowTaunt)
            {
                U.enemiesToTauntAnnex(50f);

                if (U.activeEnemiesToTaunt(Me.Location, 40f).Any())
                {
                    WoWUnit tauntEnemy = U.activeEnemiesToTaunt(Me.Location, 40f).OrderBy(u => u.Distance).FirstOrDefault();
                    if (tauntEnemy != null)
                    {
                        L.pullMobLog(string.Format($"Switch taunt target to {tauntEnemy.SafeName} at {tauntEnemy.Distance.ToString("F0")} yds @ {tauntEnemy.HealthPercent.ToString("F0")}% HP"), Core.Helpers.Common.TargetColor);
                        tauntEnemy.Target();
                        return;
                    }
                }
            }
            if (!Me.CurrentTarget.IsValidCombatUnit())
            {
                //L.infoLog("Need a new target");
                var newUnits = U.activeEnemies(Me.Location, 50f).OrderBy(u => u.Distance).ThenBy(u => u.HealthPercent);
                //L.infoLog("Number of new units: " + newUnits.Count());
                var newUnit = newUnits.FirstOrDefault();
                L.pullMobLog(string.Format($"Switch target to {newUnit.SafeName} at {newUnit.Distance.ToString("F0")} yds @ {newUnit.HealthPercent.ToString("F0")}% HP"), Core.Helpers.Common.TargetColor);
                if (newUnit != null) newUnit.Target();
                //L.infoLog(string.Format($"New Target: {Me.CurrentTarget.SafeName}.{Me.CurrentTarget.Guid}"));
            }

        }
        #endregion

        #endregion
    }
}
