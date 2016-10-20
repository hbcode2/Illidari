using Styx.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illidari.Core.IllidariSettings
{
    class VengeanceSettings : Settings
    {
        private static VengeanceSettings _instance;

        public VengeanceSettings()
            : base(System.IO.Path.Combine(CharacterSettingsDirectory, Main.CRName + "Routine-Vengeance.xml"))
        { }

        public static VengeanceSettings Instance
        {
            get { return _instance ?? (_instance = new VengeanceSettings()); }
        }

        #region Vengeance
        [Setting, DefaultValue(false)]
        [System.ComponentModel.Category("General")]
        [System.ComponentModel.DisplayName("Taunt")]
        [System.ComponentModel.Description("Allows taunting of current target (and if enabled in General Settings, \"Targeting\", auto target of any mob focused on your party/raid member).")]
        public bool VengeanceAllowTaunt { get; set; }

        #region Mitigation
        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Demon Spikes")]
        [System.ComponentModel.Description("Uses Demon Spikes based on the value supplied in Demon Spikes HP.")]
        public bool VengeanceAllowDemonSpikes { get; set; }

        [Setting, DefaultValue(90)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Demon Spikes HP")]
        [System.ComponentModel.Description("Uses Demon Spikes (if enabled) when HP is equal to or below supplied value.\r\n100 for On Cooldown; 0 to disable")]
        public int VengeanceDemonSpikesHp { get; set; }


        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Empowered Wards")]
        [System.ComponentModel.Description("Uses Empowered Wards when any unit in combat that is targeting you is casting a spell.")]
        public bool VengeanceEmpowerWards { get; set; }


        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Soul Cleave")]
        [System.ComponentModel.Description("Uses Soul Cleave based on the value supplied in Soul Cleave HP")]
        public bool VengeanceAllowSoulCleave { get; set; }

        [Setting, DefaultValue(80)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Soul Cleave HP")]
        [System.ComponentModel.Description("Uses Soul Cleave (if enabled) when HP is equal to or below supplied value.\r\n100 for On Cooldown; 0 to disable")]
        public int VengeanceSoulCleaveHp { get; set; }


        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Fiery Brand")]
        [System.ComponentModel.Description("Uses Fiery Brand based on the value supplied in Fiery Brand HP")]
        public bool VengeanceAllowFieryBrand { get; set; }

        [Setting, DefaultValue(70)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Fiery Brand HP")]
        [System.ComponentModel.Description("Uses Fiery Brand (if enabled) when HP is equal to or below supplied value.\r\n100 for On Cooldown; 0 to disable")]
        public int VengeanceFieryBrandHp { get; set; }

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Metamorphosis")]
        [System.ComponentModel.Description("Uses Metamorphosis based on the value supplied in Metamorphosis HP")]
        public bool VengeanceAllowMetamorphosis { get; set; }

        [Setting, DefaultValue(60)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Metamorphosis HP")]
        [System.ComponentModel.Description("Uses Metamorphosis (if enabled) when HP is equal to or below supplied value.\r\n100 for On Cooldown; 0 to disable")]
        public int VengeanceMetamorphosisHp { get; set; }

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Soul Barrier")]
        [System.ComponentModel.Description("Uses Soul Barrier based on the value supplied in Soul Barrier HP")]
        public bool VengeanceAllowSoulBarrier { get; set; }
        [Setting, DefaultValue(40)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Soul Barrier HP")]
        [System.ComponentModel.Description("Uses Soul Barrier (if enabled) when HP is equal to or below supplied value.\r\n100 for On Cooldown; 0 to disable")]
        public int VengeanceSoulBarrierHp { get; set; }

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Soul Carver")]
        [System.ComponentModel.Description("Uses Soul Carver based on the value supplied in Soul Carver HP")]
        public bool VengeanceAllowSoulCarver { get; set; }
        [Setting, DefaultValue(65)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Soul Carver HP")]
        [System.ComponentModel.Description("Uses Soul Carver (if enabled) when HP is equal to or below supplied value.\r\n100 for On Cooldown; 0 to disable")]
        public int VengeanceSoulCarverHp { get; set; }


        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Fel Devastation")]
        [System.ComponentModel.Description("Uses Fel Devastation based on the value supplied in Fel Devastation HP")]
        public bool VengeanceAllowFelDevastation { get; set; }
        [Setting, DefaultValue(60)]
        [System.ComponentModel.Category("Mitigation")]
        [System.ComponentModel.DisplayName("Fel Devastation HP")]
        [System.ComponentModel.Description("Uses Fel Devastation (if enabled) when HP is equal to or below supplied value.\r\n100 for On Cooldown; 0 to disable")]
        public int VengeanceFelDevastationHp { get; set; }

        #endregion

        #region Combat
        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Soul Cleave")]
        [System.ComponentModel.Description("Uses Soul Cleave as DPS when your Pain reaches the value supplied in Soul Cleave Pain")]
        public bool VengeanceCombatAllowSoulCleave { get; set; }
        [Setting, DefaultValue(80)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Soul Cleave Pain")]
        [System.ComponentModel.Description("Uses Soul Cleave (if enabled) when Pain is equal to or more than supplied value. 0 to disable")]
        public int VengeanceCombatSoulCleavePain { get; set; }

        [Setting, DefaultValue(false)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Prefer Pull with Felblade")]
        [System.ComponentModel.Description("Will attempt to pull with Felblade instead of Infernal Strike if you have the talent and if Infernal Strike is enabled.")]
        public bool VengeancePreferPullWithFelblade { get; set; }
        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Infernal Strike (single target)")]
        [System.ComponentModel.Description("Uses Infernal Strike while running the \"Single Target\" part of the routine.")]
        public bool VengeanceCombatInfernalStrikeSingleTarget { get; set; }
        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Infernal Strike (aoe)")]
        [System.ComponentModel.Description("Uses Infernal Strike while running the \"AoE\" part of the routine.")]
        public bool VengeanceCombatInfernalStrikeAoE { get; set; }
        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Infernal Strike (pull)")]
        [System.ComponentModel.Description("Uses Infernal Strike while running the \"Pull\" part of the routine.")]
        public bool VengeanceCombatInfernalStrikePull { get; set; }
        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Infernal Strike (gap closer)")]
        [System.ComponentModel.Description("Uses Infernal Strike while running the \"Gap Closer\" part of the routine.")]
        public bool VengeanceCombatInfernalStrikeGapCloser { get; set; }
        [Setting, DefaultValue(false)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Throw Glaive")]
        [System.ComponentModel.Description("Uses Throw Glaive based on the time set in Throw Glaive Timer.")]
        public bool VengeanceCombatThrowGlaive { get; set; }
        [Setting, DefaultValue(5)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Throw Glaive Timer")]
        [System.ComponentModel.Description("Uses Throw Glaive every X seconds to make sure you aren't using it too much.")]
        public int VengeanceCombatThrowGlaiveSeconds { get; set; }
        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Sigil of Chains")]
        [System.ComponentModel.Description("Uses Sigil of Chains during combat when unit count matches number of units within 8 yards of current target.")]
        public bool VengeanceCombatSigilOfChains { get; set; }
        [Setting, DefaultValue(3)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Sigil of Chains (units)")]
        [System.ComponentModel.Description("Number of units within 8 yards of current target to activate Sigil of Chains")]
        public int VengeanceCombatSigilOfChainsUnits { get; set; }
        #endregion  

        [Setting, DefaultValue(false)]
        [System.ComponentModel.Category("Interrupt")]
        [System.ComponentModel.DisplayName("Consume Magic")]
        [System.ComponentModel.Description("Interrupts casting using Consume Magic")]
        public bool VengeanceAllowInterruptConsumeMagic { get; set; }
        [Setting, DefaultValue(500)]
        [System.ComponentModel.Category("Interrupt")]
        [System.ComponentModel.DisplayName("Minimum cast time before interrupt")]
        [System.ComponentModel.Description("Minimum amount of time (in milliseconds) to wait until interrupting.\r\n0 to disable.")]
        public int VengeanceInterruptMinimumTime { get; set; }
        [Setting, DefaultValue(500)]
        [System.ComponentModel.Category("Interrupt")]
        [System.ComponentModel.DisplayName("Time left on cast before interrupt")]
        [System.ComponentModel.Description("How much time left on a cast before interrupting.\r\n0 to disable.")]
        public int VengeanceInterruptTimeLeft { get; set; }
        [Setting, DefaultValue(false)]
        [System.ComponentModel.Category("Interrupt")]
        [System.ComponentModel.DisplayName("Sigil of Silence")]
        [System.ComponentModel.Description("Interrupts casting using Sigil of Silence")]
        public bool VengeanceAllowInterruptSigilOfSilence { get; set; }
        [Setting, DefaultValue(false)]
        [System.ComponentModel.Category("Interrupt")]
        [System.ComponentModel.DisplayName("Sigil of Misery")]
        [System.ComponentModel.Description("Interrupts casting using Sigil of Misery")]
        public bool VengeanceAllowInterruptSigilOfMisery { get; set; }
        [Setting, DefaultValue(false)]
        [System.ComponentModel.Category("Stun")]
        [System.ComponentModel.DisplayName("Sigil of Misery")]
        [System.ComponentModel.Description("Allows stuns with Sigil of Misery based on Unit count supplied.")]
        public bool VengeanceAllowStunSigilOfMisery { get; set; }
        [Setting, DefaultValue(0)]
        [System.ComponentModel.Category("Stun")]
        [System.ComponentModel.DisplayName("Sigil of Misery Units")]
        [System.ComponentModel.Description("Will cast Sigil of Misery on Current Target when X units are within Melee range of you in combat.")]
        public int VengeanceStunSigilOfMiseryCount { get; set; }



        #endregion

       
    }
}
