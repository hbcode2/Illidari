using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Styx;
using Styx.Common;
using Styx.Helpers;
using Styx.WoWInternals.WoWObjects;

namespace Illidari.Core.IllidariSettings
{
    public class IllidariSettings : Settings
    {
        /// <summary>
        /// Creating a new instance will either create a default settings file or pull in the existing settings file.
        /// </summary>
        public IllidariSettings()
            : base(System.IO.Path.Combine(CharacterSettingsDirectory, Main.CRName + "Routine.xml"))
        {

        }

        #region General
        [Setting, DefaultValue(false)]
        public bool GeneralDebug { get; set; }

        #region Mobility
        [Setting, DefaultValue(true)]
        public bool GeneralFacing { get; set; }
        [Setting, DefaultValue(true)]
        public bool GeneralMovement { get; set; }
        [Setting, DefaultValue(true)]
        public bool GeneralTargeting { get; set; }
        #endregion

        #endregion

        #region Havoc

        #region HealthPotion
        [Setting, DefaultValue(40)]
        public int HavocHealthPotionHp { get; set; }
        [Setting, DefaultValue("")]
        public string HavocHealthPotionListSetting { get; set; }
        
        public List<uint> HavocHealthPotionList
        {
            get
            {
                List<uint> hpList = new List<uint>();
                if (string.IsNullOrEmpty(HavocHealthPotionListSetting)) { return hpList; }
                string[] potions = HavocHealthPotionListSetting.Split('|');
                foreach (var item in potions)
                {
                    if (string.IsNullOrEmpty(item) || item.SafeGetInt() == 0) { continue; }
                    hpList.Add(item.SafeGetInt());
                }
                return hpList;
            }
        }

        #endregion

        #region Blur Settings
        [Setting, DefaultValue(60)]
        public int HavocBlurHp { get; set; }
        [Setting, DefaultValue("OR")]
        public string HavocBlurOperator { get; set; }
        [Setting, DefaultValue(4)]
        public int HavocBlurUnits { get; set; }
        #endregion

        #region Darkness Settings
        [Setting, DefaultValue(50)]
        public int HavocDarknessHp { get; set; }
        [Setting, DefaultValue(5)]
        public int HavocDarknessUnits { get; set; }
        [Setting, DefaultValue("OR")]
        public string HavocDarknessOperator { get; set; }
        #endregion

        #region Chaos Nova Settings
        [Setting, DefaultValue(40)]
        public int HavocChaosNovaHp { get; set; }
        [Setting, DefaultValue("OR")]
        public string HavocChaosNovaOperator { get; set; }
        [Setting, DefaultValue(3)]
        public int HavocChaosNovaUnits { get; set; }
        #endregion

        #endregion

        #region Vengeance
        [Setting, DefaultValue(false)]
        public bool VengeanceAllowTaunt { get; set; }

        [Setting, DefaultValue(true)]
        public bool VengeanceAllowDemonSpikes { get; set; }
        [Setting, DefaultValue(100)]
        public int VengeanceDemonSpikesHp { get; set; }

        [Setting, DefaultValue(true)]
        public bool VengeanceEmpowerWards { get; set; }

        [Setting, DefaultValue(true)]
        public bool VengeanceAllowSoulCleave { get; set; }
        [Setting, DefaultValue(100)]
        public int VengeanceSoulCleaveHp { get; set; }

        [Setting, DefaultValue(true)]
        public bool VengeanceAllowFieryBrand { get; set; }
        [Setting, DefaultValue(100)]
        public int VengeanceFieryBrandHp { get; set; }

        [Setting, DefaultValue(true)]
        public bool VengeanceCombatAllowSoulCleave { get; set; }
        [Setting, DefaultValue(80)]
        public int VengeanceCombatSoulCleavePain { get; set; }

        #endregion
    }
}
