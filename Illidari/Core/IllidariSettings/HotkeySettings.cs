using Styx.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illidari.Core.IllidariSettings
{
    class HotkeySettings : Settings
    {
        private static HotkeySettings _instance;

        public HotkeySettings()
            : base(System.IO.Path.Combine(CharacterSettingsDirectory, Main.CRName + "Routine-Hotkeys.xml"))
        { }

        public static HotkeySettings Instance
        {
            get { return _instance ?? (_instance = new HotkeySettings()); }
        }


        #region HotKeys
        [Setting, DefaultValue(1)] // default Alt = 1
        public int HotkeyVengeanceAoeModifier { get; set; }
        [Setting, DefaultValue("Q")]
        public string HotkeyVengeanceAoeKey { get; set; }
        [Setting, DefaultValue(1)] // default Alt = 1
        public int HotkeyVengeanceDefensiveModifier { get; set; }
        [Setting, DefaultValue("D")]
        public string HotkeyVengeanceDefensiveKey { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyVengeanceDefensiveDemonSpikes { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyVengeanceDefensiveSoulCleave { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyVengeanceDefensiveFieryBrand { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyVengeanceDefensiveMetamorphosis { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyVengeanceDefensiveSoulBarrier { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyVengeanceDefensiveSoulCarver { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyVengeanceDefensiveEmpowerWards { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyVengeanceDefensiveFelDevastation { get; set; }
        [Setting, DefaultValue(1)] // default Alt = 1
        public int HotkeyGeneralRotationOnlyModifier { get; set; }
        [Setting, DefaultValue("R")]
        public string HotkeyGeneralRotationOnlyKey { get; set; }

        [Setting, DefaultValue(1)] // default Alt = 1
        public int HotkeyHavocOffensiveModifier { get; set; }
        [Setting, DefaultValue("A")]
        public string HotkeyHavocOffensiveKey { get; set; }

        [Setting, DefaultValue(true)]
        public bool HotkeyHavocOffensiveAgilityPotion { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyHavocOffensiveFoTI { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyHavocOffensiveMetamorphosis { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyHavocOffensiveNemesis { get; set; }
        [Setting, DefaultValue(true)]
        public bool HotkeyHavocOffensiveChaosBlades { get; set; }
        #endregion

    }
}
