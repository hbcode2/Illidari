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
using C = Illidari.Core.Helpers.Common;
using System.Diagnostics;
using System.Reflection;
#endregion

namespace Illidari
{
    public class Main : CombatRoutine
    {
        public static Core.IllidariSettings.IllidariSettings IS;
        private static readonly Version version = new Version(08, 31, 2016);
        public override string Name { get { return string.Format($"{CRName} v{version}"); } }
        public override WoWClass Class { get { return WoWClass.DemonHunter; } }
        public static string CRName { get { return "Illidari"; } }
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        
        #region Implementations
        public override Composite PreCombatBuffBehavior
        {
            get
            {
               // if (DateTime.Now.Second % 5 == 0)
                    //L.debugLog("Calling PreCombatBuffBehavior");
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
                //if (DateTime.Now.Second % 5 == 0)
                   // L.debugLog("Calling CombatBuffBehavior");
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
               // if (DateTime.Now.Second % 5 == 0)
                    //L.debugLog("Calling CombatBehavior");
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
                //if (DateTime.Now.Second % 5 == 0)
                    //L.debugLog("Calling PullBehavior");
                if (Me.Specialization == WoWSpec.DemonHunterHavoc)
                {
                    return new ActionRunCoroutine(ctx => H.Pull());
                }
                return new ActionRunCoroutine(ctx => V.Pull());
            }
        }
        public override Composite RestBehavior
        {
            get
            {
                //if (DateTime.Now.Second % 5 == 0)
                   // L.debugLog("Calling RestBehavior");
                return base.RestBehavior;
            }
        }
        public override Composite MoveToTargetBehavior
        {
            get
            {
                //if (DateTime.Now.Second % 5 == 0)
                   // L.debugLog("Calling MoveToTargetBehavior");
                return new ActionRunCoroutine(ctx => C.EnsureMeleeRange(Me.CurrentTarget));
            }
        }
        public override Composite PullBuffBehavior
        {
            get
            {
                //if (DateTime.Now.Second % 5 == 0)
                   // L.debugLog("Calling PullBuffBehavior");
                return base.PullBuffBehavior;
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
            
            IS = new Core.IllidariSettings.IllidariSettings();

            TM.initTalents();
            Type type = IS.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "SettingsPath") { continue; }
                if (property.PropertyType == typeof(List<uint>))
                {
                    List<uint> uintList = (List<uint>)property.GetValue(IS, null);
                    foreach (var uintItem in uintList)
                    {
                        L.debugLog(string.Format($"{property.Name}: {uintItem}"));
                    }
                }
                else
                {
                    L.debugLog(string.Format($"{property.Name}: {property.GetValue(IS, null)}"));
                }

            }

        }
        public override bool WantButton { get { return true; } }

        public override CapabilityFlags SupportedCapabilities
        {
            get
            {
                return CapabilityFlags.Aoe | CapabilityFlags.DefensiveCooldowns | CapabilityFlags.Facing | CapabilityFlags.GapCloser | CapabilityFlags.Interrupting | CapabilityFlags.Movement | CapabilityFlags.OffensiveCooldowns | CapabilityFlags.Targeting | CapabilityFlags.Taunting;
            }
        }

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
            
            // cache a few things like my fury/pain
            C.Cache();
            
            // grab all enemies within 50 yards to find a valid target
            U.enemyAnnex(50f);
            
            // get a taunt target, if any
            if (GetTauntTarget()) { return; }

            // get a new target, if any
            if (GetNewTarget()) { return; }
           

        }
        
        private bool GetTauntTarget()
        {
            if (Me.Specialization == WoWSpec.DemonHunterVengeance && IS.VengeanceAllowTaunt)
            {
                U.enemiesToTauntAnnex(50f);

                if (U.activeEnemiesToTaunt(Me.Location, 40f).Any())
                {
                    IEnumerable<WoWUnit> tauntEnemies = U.activeEnemiesToTaunt(Me.Location, 40f);
                    if (tauntEnemies != null)
                    {
                        var tauntEnemy = tauntEnemies.OrderBy(u => u.Distance).FirstOrDefault();
                        if (tauntEnemy != null)
                        {
                            L.pullMobLog(string.Format($"Switch taunt target to {tauntEnemy.SafeName} at {tauntEnemy.Distance.ToString("F0")} yds @ {tauntEnemy.HealthPercent.ToString("F0")}% HP"), Core.Helpers.Common.TargetColor);
                            tauntEnemy.Target();
                            return true;
                        }

                    }
                }
            }
            return false;
        }

        private bool GetNewTarget()
        {
            if (!Me.CurrentTarget.IsValidCombatUnit())
            {
                //L.debugLog("Need a new target");
                var newUnits = U.activeEnemies(Me.Location, 50f).OrderBy(u => u.Distance).ThenBy(u => u.HealthPercent);
                L.debugLog("Number of new units: " + newUnits.Count());
                if (newUnits != null)
                {
                    var newUnit = newUnits.FirstOrDefault();
                    if (newUnit != null)
                    {
                        L.pullMobLog(string.Format($"Switch target to {newUnit.SafeName} at {newUnit.Distance.ToString("F0")} yds @ {newUnit.HealthPercent.ToString("F0")}% HP"), Core.Helpers.Common.TargetColor);
                        newUnit.Target();
                        return true;
                    }
                    //L.infoLog(string.Format($"New Target: {Me.CurrentTarget.SafeName}.{Me.CurrentTarget.Guid}"));
                }
            }
            return false;
        }

        #endregion

        #endregion
    }
}
