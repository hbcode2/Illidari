using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx;
using Styx.CommonBot;
using Styx.Common;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

using L = Illidari.Core.Utilities.Log;
using C = Illidari.Core.Helpers.Common;
namespace Illidari.Core.Managers
{
    public class TalentManager
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        public static event EventHandler specChanged;
        public static WoWSpec CurrentSpec
        {
            get;
            private set;
        }

        public static bool

        #region 99 level
            HavocFelMastery = false,
            HavocChaosCleave = false,
            HavocBlindFury = false,
            VengeanceAbyssalStrike = false,
            VengeanceAgonizingFlames = false,
            VengeanceRazorSpikes = false,
        #endregion

        #region 100 level
            HavocPrepared = false,
            HavocDemonBlades = false,
            HavocDemonicAppetite = false,
            VengeanceFeastOfSouls = false,
            VengeanceFallout = false,
            VengeanceBurningAlive = false,
        #endregion

        #region 102 level
            HavocFelblade = false,
            HavocFirstBlood = false,
            HavocBloodlet = false,
            VengeanceFelblade = false,
            VengeanceFlameCrash = false,
            VengeanceFelEruption = false,
        #endregion

        #region 104 level
            HavocNetherwalk = false,
            HavocDesperateInstints = false,
            HavocSoulRending = false,
            VengeanceFeedTheDemon = false,
            VengeanceFracture = false,
            VengeanceSoulRending = false,
        #endregion

        #region 106 level
            HavocMomentum = false,
            HavocFelEruption = false,
            HavocNemesis = false,
            VengeanceConcentratedSigils = false,
            VengeanceSigilOfChains = false,
            VengeanceQuickenedSigils = false,
        #endregion

        #region 108 level
            HavocMasterOfTheGlaive = false,
            HavocUnleasedPower = false,
            HavocDemonReborn = false,
            VengeanceFelDevastation = false,
            VengeanceBladeTurning = false,
            VengeanceSpiritBomb = false,
        #endregion

        #region 110 level
            HavocChaosBlades = false,
            HavocFelBarrage = false,
            HavocDemonic = false,
            VengeanceLastResort = false,
            VengeanceNetherBond = false,
            VengeanceSoulBarrier = false;
        #endregion

        private static void onSpecChanged(EventArgs args)
        {
            EventHandler handler = specChanged;
            if (handler != null)
            {
                handler(null, args);
            }
        }
        private static bool getTalent(int tier, int index)
        {
            // these are 0-based, and we are sending in actual tiers and index numbers.
            tier--;
            index--;
            return Me.GetLearnedTalent(tier).Index == index;
        }
        public static void initTalents()
        {
            setTalEvents();

            setTalents();

            printTalents();
        }

        private static void setTalents()
        {
            // don't allow any talents if < 99 level
            if (Me.Level < 99) { return; }

            if (Me.Specialization == WoWSpec.DemonHunterHavoc)
            {
                if (Me.Level >= 99)
                {
                    HavocFelMastery = getTalent(1, 1);
                    HavocChaosCleave = getTalent(1, 2);
                    HavocBlindFury = getTalent(1, 3);
                }
                if (Me.Level >= 100)
                {
                    HavocPrepared = getTalent(2, 1);
                    HavocDemonBlades = getTalent(2, 2);
                    HavocDemonicAppetite = getTalent(2, 3);
                }
                if (Me.Level >= 102)
                {
                    HavocFelblade = getTalent(3, 1);
                    HavocFirstBlood = getTalent(3, 2);
                    HavocBloodlet = getTalent(3, 3);
                }
                if (Me.Level >= 104)
                {
                    HavocNetherwalk = getTalent(4, 1);
                    HavocDesperateInstints = getTalent(4, 2);
                    HavocSoulRending = getTalent(4, 3);
                }
                if (Me.Level >= 106)
                {
                    HavocMomentum = getTalent(5, 1);
                    HavocFelEruption = getTalent(5, 2);
                    HavocNemesis = getTalent(5, 3);
                }
                if (Me.Level >= 108)
                {
                    HavocMasterOfTheGlaive = getTalent(6, 1);
                    HavocUnleasedPower = getTalent(6, 2);
                    HavocDemonReborn = getTalent(6, 3);
                }
                if (Me.Level >= 110)
                {
                    HavocChaosBlades = getTalent(7, 1);
                    HavocFelBarrage = getTalent(7, 2);
                    HavocDemonic = getTalent(7, 3);
                }
            }
            if (Me.Specialization == WoWSpec.DemonHunterVengeance)
            {
                if (Me.Level >= 99)
                {
                    VengeanceAbyssalStrike = getTalent(1, 1);
                    VengeanceAgonizingFlames = getTalent(1, 2);
                    VengeanceRazorSpikes = getTalent(1, 3);
                }
                if (Me.Level >= 100)
                {
                    VengeanceFeastOfSouls = getTalent(2, 1);
                    VengeanceFallout = getTalent(2, 2);
                    VengeanceBurningAlive = getTalent(2, 3);
                }
                if (Me.Level >= 102)
                {
                    VengeanceFelblade = getTalent(3, 1);
                    VengeanceFlameCrash = getTalent(3, 2);
                    VengeanceFelEruption = getTalent(3, 3);
                }
                if (Me.Level >= 104)
                {
                    VengeanceFeedTheDemon = getTalent(4, 1);
                    VengeanceFracture = getTalent(4, 2);
                    VengeanceSoulRending = getTalent(4, 3);
                }
                if (Me.Level >= 106)
                {
                    VengeanceConcentratedSigils = getTalent(5, 1);
                    VengeanceSigilOfChains = getTalent(5, 2);
                    VengeanceQuickenedSigils = getTalent(5, 3);
                }
                if (Me.Level >= 108)
                {
                    VengeanceFelDevastation = getTalent(6, 1);
                    VengeanceBladeTurning = getTalent(6, 2);
                    VengeanceSpiritBomb = getTalent(6, 3);
                }
                if (Me.Level >= 110)
                {
                    VengeanceLastResort = getTalent(7, 1);
                    VengeanceNetherBond = getTalent(7, 2);
                    VengeanceSoulBarrier = getTalent(7, 3);
                }
            }
        }

        public static void setTalEvents()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                //Lua.Events.AttachEvent("PLAYER_LEVEL_UP", playerLeveledUp);
                //Lua.Events.AttachEvent("CHARACTER_POINTS_CHANGED", initializeTalents);
                //Lua.Events.AttachEvent("ACTIVE_TALENT_GROUP_CHANGED", initializeTalents);
                //Lua.Events.AttachEvent("PLAYER_SPECIALIZATION_CHANGED", talentSpecChanged);
                //Lua.Events.AttachEvent("LEARNED_SPELL_IN_TAB", initializeTalents);
                Lua.Events.AttachEvent("PLAYER_TALENT_UPDATE", initializeTalents);
            }

        }

        private static void talentSpecChanged(object sender, LuaEventArgs args)
        {
            //TODO: Remove after all 3 Spec supported
            setTalents();
            Logging.Write("Spec changed to {0}.- {1}", Me.Specialization, args.EventName);
            printTalents();
            //onSpecChanged(args);
        }
        private static void initializeTalents(object sender, LuaEventArgs args)
        {
            Logging.Write("Event triggered: " + args.EventName);
            setTalents();
            printTalents();
        }
        private static void playerLeveledUp(object sender, LuaEventArgs args)
        {
            Logging.Write(String.Format($"Player leveled up!  Now level {args.Args[0]}"));
            setTalents();
        }
        private static void printTalents()
        {
            Logging.Write("Selected talents for {0}", Me.Specialization.ToString().AddSpaces());
            if (Me.Specialization == WoWSpec.DemonHunterHavoc)
            {
                // tier 1
                if (HavocFelMastery) { printTalent("Fel Mastery", 1); }
                else if (HavocChaosCleave) { printTalent("Chaos Cleave", 1); }
                else if (HavocBlindFury) { printTalent("Blind Fury", 1); }

                // tier 2
                if (HavocPrepared) { printTalent("Prepared", 2); }
                else if (HavocDemonBlades) { printTalent("Demon Blades", 2); }
                else if (HavocDemonicAppetite) { printTalent("Demonic Appetite", 2); }

                // tier 3
                if (HavocFelblade) { printTalent("Felblade", 3); }
                else if (HavocFirstBlood) { printTalent("First Blood", 3); }
                else if (HavocBloodlet) { printTalent("Bloodlet", 3); }

                // tier 4
                if (HavocNetherwalk) { printTalent("Netherwalk", 4); }
                else if (HavocDesperateInstints) { printTalent("Desperate Instincts", 4); }
                else if (HavocSoulRending) { printTalent("Soul Rending", 4); }

                // tier 5
                if (HavocMomentum) { printTalent("Momentum", 5); }
                else if (HavocFelEruption) { printTalent("Fel Eruption", 5); }
                else if (HavocNemesis) { printTalent("Nemesis", 5); }

                // tier 6
                if (HavocMasterOfTheGlaive) { printTalent("Master of the Glaive", 6); }
                else if (HavocUnleasedPower) { printTalent("Unleashed Power", 6); }
                else if (HavocDemonReborn) { printTalent("Demon Reborn", 6); }

                // tier 7
                if (HavocChaosBlades) { printTalent("Chaos Blades", 7); }
                else if (HavocFelBarrage) { printTalent("Fel Barrage", 7); }
                else if (HavocDemonic) { printTalent("Demonic", 7); }
            }

            if (Me.Specialization == WoWSpec.DemonHunterVengeance)
            {
                // tier 1
                if (VengeanceAbyssalStrike) { printTalent("Abyssal Strike", 1); }
                else if (VengeanceAgonizingFlames) { printTalent("Agonizing Flames", 1); }
                else if (VengeanceRazorSpikes) { printTalent("Razor Spikes", 1); }

                // tier 2
                if (VengeanceFeastOfSouls) { printTalent("Feast of Souls", 2); }
                else if (VengeanceFallout) { printTalent("Fallout", 2); }
                else if (VengeanceBurningAlive) { printTalent("Burning Alive", 2); }

                // tier 3
                if (VengeanceFelblade) { printTalent("Felblade", 3); }
                else if (VengeanceFlameCrash) { printTalent("Flame Crash", 3); }
                else if (VengeanceFelEruption) { printTalent("Fe Eruption", 3); }

                // tier 4
                if (VengeanceFeedTheDemon) { printTalent("Feed the Demon", 4); }
                else if (VengeanceFracture) { printTalent("Fracture", 4); }
                else if (VengeanceSoulRending) { printTalent("SoulRending", 4); }

                // tier 5
                if (VengeanceConcentratedSigils) { printTalent("Concentrated Sigils", 5); }
                else if (VengeanceSigilOfChains) { printTalent("Sigil of Chains", 5); }
                else if (VengeanceQuickenedSigils) { printTalent("Quickened Sigils", 5); }

                // tier 6
                if (VengeanceFelDevastation) { printTalent("Fel Devastation", 6); }
                else if (VengeanceBladeTurning) { printTalent("Blade Turning", 6); }
                else if (VengeanceSpiritBomb) { printTalent("Spirit Bomb", 6); }

                // tier7
                if (VengeanceLastResort) { printTalent("Last Resort", 7); }
                else if (VengeanceNetherBond) { printTalent("Nether Bond", 7); }
                else if (VengeanceSoulBarrier) { printTalent("Soul Barrier", 7); }

            }
        }
        private static void printTalent(string name, int tier)
        {
            L.infoLog("Tier {0}: {1}", C.InfoColor, tier, name);
        }

        private static void updateTalentManager(object sender, LuaEventArgs args)
        {
            L.infoLog("------------------", C.InfoColor);
            L.infoLog("Talents changed...", C.InfoColor);
            initTalents();
            printTalents();
        }
    }
}
