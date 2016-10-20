using Styx.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illidari.Core.IllidariSettings
{
    class HavocSettings : Settings
    {
        private static HavocSettings _instance;

        public HavocSettings()
            : base(System.IO.Path.Combine(CharacterSettingsDirectory, Main.CRName + "Routine-Havoc.xml"))
        { }

        public static HavocSettings Instance
        {
            get { return _instance ?? (_instance = new HavocSettings()); }
        }

        #region Havoc

        #region Potion / Flask

        //[Setting]
        //[System.ComponentModel.Category("Defensive")]
        //[System.ComponentModel.DisplayName("Potion List")]
        //[System.ComponentModel.Description("Use Potion when health is < HP supplied.")]
        //public List<uint> HavocHealthPotionList { get; set; }
        
        [Setting, DefaultValue(40)]
        [System.ComponentModel.Category("Defensive")]
        [System.ComponentModel.DisplayName("Potion HP")]
        [System.ComponentModel.Description("Use Potion when health is < HP supplied.")]
        public int HavocHealthPotionHp { get; set; }

        [Setting]
        [System.ComponentModel.Category("Defensive")]
        [System.ComponentModel.DisplayName("Potion ID")]
        [System.ComponentModel.Description("The ID of the potion you want to use when your health < HP supplied.")]
        public uint HavocHealthPotionID { get; set; }

        [Setting, DefaultValue(false)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Agility Flask")]
        [System.ComponentModel.Description("Uses your highest available Agility Flask automatically.")]
        public bool HavocUseAgilityFlask { get; set; }

        [Setting, DefaultValue(CooldownTypes.Manual)]
        [System.ComponentModel.Category("Cooldowns")]
        [System.ComponentModel.DisplayName("Use Agility Potion")]
        [System.ComponentModel.Description("Uses your highest available Agility Potion as specified.")]
        public CooldownTypes HavocUseAgilityPotionCooldown { get; set; }

        #endregion

        #region Blur Settings

        [Setting, DefaultValue(60)]
        [System.ComponentModel.Category("Defensive")]
        [System.ComponentModel.DisplayName("Blur (HP)")]
        [System.ComponentModel.Description("Uses Blur when your HP is below supplied value.\nSet to 0 to disable.")]
        public int HavocBlurHp { get; set; }

        [Setting, DefaultValue(4)]
        [System.ComponentModel.Category("Defensive")]
        [System.ComponentModel.DisplayName("Blur (Units)")]
        [System.ComponentModel.Description("Uses Blur when the supplied number of units that are in combat with you is met.\nSet to 0 to disable.")]
        public int HavocBlurUnits { get; set; }

        #endregion

        #region Darkness Settings

        [Setting, DefaultValue(50)]
        [System.ComponentModel.Category("Defensive")]
        [System.ComponentModel.DisplayName("Darkness (HP)")]
        [System.ComponentModel.Description("Uses Darkness when your HP is below supplied value.\nSet to 0 to disable.")]
        public int HavocDarknessHp { get; set; }

        [Setting, DefaultValue(5)]
        [System.ComponentModel.Category("Defensive")]
        [System.ComponentModel.DisplayName("Darkness (Units)")]
        [System.ComponentModel.Description("Uses Darkness when the supplied number of units that are in combat with you is met.\nSet to 0 to disable.")]
        public int HavocDarknessUnits { get; set; }

        #endregion

        #region Chaos Nova Settings
        [Setting, DefaultValue(40)]
        [System.ComponentModel.Category("Defensive")]
        [System.ComponentModel.DisplayName("Chaos Nova (HP)")]
        [System.ComponentModel.Description("Uses Chaos Nova when your HP is below supplied value.\nSet to 0 to disable.")]
        public int HavocChaosNovaHp { get; set; }

        [Setting, DefaultValue(3)]
        [System.ComponentModel.Category("Defensive")]
        [System.ComponentModel.DisplayName("Chaos Nova (Units)")]
        [System.ComponentModel.Description("Uses Chaos Nova when the supplied number of units that are in combat with you is met.\nSet to 0 to disable.")]
        public int HavocChaosNovaUnits { get; set; }

        #endregion

        #region Fel Rush / Vengeful Retreat

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Fel Rush (on pull)")]
        [System.ComponentModel.Description("Uses Fel Rush during the \"Pull\" part of the routine.")]
        public bool HavocFelRushOnPull { get; set; }

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Fel Rush (single target)")]
        [System.ComponentModel.Description("Uses Fel Rush while running the \"Single Target\" part of the routine.")]
        public bool HavocFelRushSingleTarget { get; set; }

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Fel Rush (aoe)")]
        [System.ComponentModel.Description("Uses Fel Rush while running the \"AoE\" part of the routine.")]
        public bool HavocFelRushAoe { get; set; }

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Vengeful Retreat (single target)")]
        [System.ComponentModel.Description("Uses Vengeful Retreat while running the \"Single Target\" part of the routine.")]
        public bool HavocVengefulReatreatSingleTarget { get; set; }

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Combat")]
        [System.ComponentModel.DisplayName("Vengeful Retreat (aoe)")]
        [System.ComponentModel.Description("Uses Vengeful Retreat while running the \"AoE\" part of the routine.")]
        public bool HavocVengefulReatreatAoe { get; set; }

        #endregion

        #region DPS cooldowns
        [Setting, DefaultValue(CooldownTypes.Cooldown)]
        [System.ComponentModel.Category("Cooldowns")]
        [System.ComponentModel.DisplayName("Metamorphosis")]
        [System.ComponentModel.Description("Uses Metamorphosis as supplied.")]
        public CooldownTypes HavocUseMetamorphosisCooldown { get; set; }

        [Setting, DefaultValue(CooldownTypes.AoE)]
        [System.ComponentModel.Category("Cooldowns")]
        [System.ComponentModel.DisplayName("Fury of the Illidari")]
        [System.ComponentModel.Description("Uses Fury of the Illidari as supplied.")]
        public CooldownTypes HavocUseFuryOfTheIllidariCooldown { get; set; }

        [Setting, DefaultValue(CooldownTypes.Cooldown)]
        [System.ComponentModel.Category("Cooldowns")]
        [System.ComponentModel.DisplayName("Chaos Blades")]
        [System.ComponentModel.Description("Uses Chaos Blades as supplied.")]
        public CooldownTypes HavocUseChaosBlades { get; set; }

        [Setting, DefaultValue(CooldownTypes.Cooldown)]
        [System.ComponentModel.Category("Cooldowns")]
        [System.ComponentModel.DisplayName("Nemesis")]
        [System.ComponentModel.Description("Uses Nemesis as supplied.")]
        public CooldownTypes HavocUseNemesis { get; set; }


        #endregion

        #endregion

        
    }
}
