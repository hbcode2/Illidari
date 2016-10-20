using Styx.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illidari.Core.IllidariSettings
{
    class GeneralSettings : Settings
    {
        private static GeneralSettings _instance;
        public GeneralSettings()
            : base(System.IO.Path.Combine(CharacterSettingsDirectory, Main.CRName + "Routine-General.xml"))
        { }

        public static GeneralSettings Instance
        {
            get { return _instance ?? (_instance = new GeneralSettings()); }
        }

        [Setting, DefaultValue(false)]
        [System.ComponentModel.Category("Debug")]
        [System.ComponentModel.DisplayName("Debug Logging")]
        [System.ComponentModel.Description("Allows more descriptive information to be seen in the log window.")]
        public bool GeneralDebug { get; set; }

        #region Mobility
        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mobility")]
        [System.ComponentModel.DisplayName("Allow Auto Facing")]
        [System.ComponentModel.Description("Will automatically face current target while in combat.")]
        public bool GeneralFacing { get; set; }

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mobility")]
        [System.ComponentModel.DisplayName("Allow Auto Movement")]
        [System.ComponentModel.Description("Will automatically move towards target while in combat.\r\nAlso used for other items specifically in rotations. I.E. Havoc Vengeful Retreat / Fel Rush combos.")]
        public bool GeneralMovement { get; set; }

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mobility")]
        [System.ComponentModel.DisplayName("Allow Auto Targeting")]
        [System.ComponentModel.Description("Will automatically select the closest target if in combat. Also other applications need this like Targeting for Taunting and Interrupts.")]
        public bool GeneralTargeting { get; set; }

        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Mobility")]
        [System.ComponentModel.DisplayName("Ignore Opposing Faction")]
        [System.ComponentModel.Description("Will not see Opposing Faction as a valid target if enabled. **Still in Development. Set to False if you have issues with this**")]
        public bool GeneralIgnoreOpposingFaction { get; set; }

        #endregion

        #region Resting


        [Setting, DefaultValue(true)]
        [System.ComponentModel.Category("Resting")]
        [System.ComponentModel.DisplayName("Use Bandages")]
        [System.ComponentModel.Description("Uses bandages while resting.")]
        public bool GeneralRestingUseBandages { get; set; }

        [Setting, DefaultValue(50)]
        [System.ComponentModel.Category("Resting")]
        [System.ComponentModel.DisplayName("Rest HP")]
        [System.ComponentModel.Description("Rest when HP is lower than supplied value.")]
        public int GeneralRestingRestHp { get; set; }

        [Setting, DefaultValue(90)]
        [System.ComponentModel.Category("Resting")]
        [System.ComponentModel.DisplayName("Rest Until HP")]
        [System.ComponentModel.Description("Will keep resting until HP reaches supplied value.")]
        public int GeneralRestingRestHpReaches { get; set; }

        [Setting, DefaultValue(30)]
        [System.ComponentModel.Category("Resting")]
        [System.ComponentModel.DisplayName("Rest HP Timeout (seconds)")]
        [System.ComponentModel.Description("If this timeout is hit, it will stop resting regardless of HP.")]
        public int GeneralRestingRestHpSecondsTimeout { get; set; }

        [Setting, DefaultValue(false)]
        [System.ComponentModel.Category("Resting")]
        [System.ComponentModel.DisplayName("Use Specialty Food")]
        [System.ComponentModel.Description("Will use specialty food that gives you a buff.")]
        public bool GeneralRestingUseSpecialtyFood { get; set; }

        #endregion
    }
}
