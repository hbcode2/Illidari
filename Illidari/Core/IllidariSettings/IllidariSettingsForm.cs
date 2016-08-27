using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Illidari
{

    public partial class IllidariSettingsForm : Form
    {
        private Illidari.Core.IllidariSettings.IllidariSettings S = new Core.IllidariSettings.IllidariSettings();

        public IllidariSettingsForm()
        {
            InitializeComponent();
            
            #region General Settings load
            GeneralEnableDebug.Checked = S.GeneralDebug;
            GeneralEnableFacing.Checked = S.GeneralFacing;
            GeneralEnableMovement.Checked = S.GeneralMovement;
            GeneralEnableTargeting.Checked = S.GeneralTargeting;
            #endregion

            #region Havoc Settings load
            HavocBlurHp.Value = S.HavocBlurHp;
            HavocBlurOperator.Text = S.HavocBlurOperator;
            HavocBlurUnits.Value = S.HavocBlurUnits;
            HavocChaosNovaHp.Value = S.HavocChaosNovaHp;
            HavocChaosNovaOperator.Text = S.HavocChaosNovaOperator;
            HavocChaosNovaUnits.Value = S.HavocChaosNovaUnits;
            HavocDarknessHp.Value = S.HavocDarknessHp;
            HavocDarknessOperator.Text = S.HavocDarknessOperator;
            HavocDarknessUnits.Value = S.HavocDarknessUnits;
            HavocPotionsHp.Value = S.HavocHealthPotionHp;
            foreach (var pot in S.HavocHealthPotionList)
            {
                HavocPotionsHpList.Text += pot.ToString() + Environment.NewLine;
            }
            #endregion

            #region Vengeance Settings load

            VengeanceAllowTaunt.Checked = S.VengeanceAllowTaunt;

            VengeanceUseDemonSpikes.Checked = S.VengeanceAllowDemonSpikes;
            VengeanceDemonSpikesHP.Value = S.VengeanceDemonSpikesHp;
            VengeanceDemonSpikesHP.Enabled = VengeanceUseDemonSpikes.Checked;

            VengeanceUseSoulCleave.Checked = S.VengeanceAllowSoulCleave;
            VengeanceSoulCleaveHp.Value = S.VengeanceSoulCleaveHp;
            VengeanceSoulCleaveHp.Enabled = S.VengeanceAllowSoulCleave;

            VengeanceUseFieryBrand.Checked = S.VengeanceAllowFieryBrand;
            VengeanceFieryBrandHp.Value = S.VengeanceFieryBrandHp;
            VengeanceFieryBrandHp.Enabled = S.VengeanceAllowFieryBrand;

            VengeanceUseEmpowerWards.Checked = S.VengeanceEmpowerWards;

            VengeanceCombatUseSoulCleave.Checked = S.VengeanceCombatAllowSoulCleave;
            VengeanceCombatSoulCleavePain.Value = S.VengeanceCombatSoulCleavePain;
            VengeanceCombatSoulCleavePain.Enabled = S.VengeanceCombatAllowSoulCleave;
            
            #endregion
        }

        #region General Setting events
        private void GeneralEnableDebug_CheckedChanged(object sender, EventArgs e)
        {
            S.GeneralDebug = GeneralEnableDebug.Checked;
        }

        private void GeneralEnableMovement_CheckedChanged(object sender, EventArgs e)
        {
            S.GeneralMovement = GeneralEnableMovement.Checked;
        }
        private void GeneralEnableTargeting_CheckedChanged(object sender, EventArgs e)
        {
            S.GeneralTargeting = GeneralEnableTargeting.Checked;
        }

        private void GeneralEnableFacing_CheckedChanged(object sender, EventArgs e)
        {
            S.GeneralFacing = GeneralEnableFacing.Checked;
        }


        private void btnListSettings_Click(object sender, EventArgs e)
        {
            Type type = Main.IS.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "SettingsPath") { continue; }
                if (property.PropertyType == typeof(List<uint>))
                {
                    List<uint> uintList = (List<uint>)property.GetValue(Main.IS, null);
                    foreach (var uintItem in uintList)
                    {
                        Illidari.Core.Utilities.Log.infoLog(string.Format($"{property.Name}: {uintItem}"), Core.Helpers.Common.InfoColor);
                    }
                }
                else
                {
                    Illidari.Core.Utilities.Log.infoLog(string.Format($"{property.Name}: {property.GetValue(Main.IS, null)}"), Core.Helpers.Common.InfoColor);
                }

            }
        }

        private void btnHavocFindBestPotion_Click(object sender, EventArgs e)
        {
            WoWItem potion = Core.Item.FindBestHealingPotion();
            if (potion != null)
            {
                Core.Utilities.Log.infoLog(string.Format($"[BestPotion]: Best potion is: {potion.SafeName}.{potion.ItemInfo.Id}."), Core.Helpers.Common.InfoColor);
            }
            else
            {
                Core.Utilities.Log.infoLog(string.Format($"[BestPotion]: You do not have a suitable potion."), Core.Helpers.Common.InfoColor);
            }
        }


        #endregion

        #region Save, Cancel, Export and Import events
        private void btnSaveAndClose_Click(object sender, EventArgs e)
        {
            // grab the list of potions and save them
            string[] havocPotionsHp = HavocPotionsHpList.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            S.HavocHealthPotionListSetting = String.Join("|", havocPotionsHp);
            
            S.Save();
            Main.IS = new Core.IllidariSettings.IllidariSettings();
            this.Close();
        }

        private void btnCancelAndClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.DefaultExt = "xml";
            sfd.Title = "Export Illidari Settings File";
            sfd.RestoreDirectory = true;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                S.SaveToFile(sfd.FileName);
                MessageBox.Show("Exported to file\r\n" + sfd.FileName);
            }
        }
        #endregion

        #region Havoc Events
        private void HavocDarknessHp_ValueChanged(object sender, EventArgs e)
        {
            S.HavocDarknessHp = (int)HavocDarknessHp.Value;
        }

        private void HavocDarknessOperator_SelectedIndexChanged(object sender, EventArgs e)
        {
            S.HavocDarknessOperator = HavocDarknessOperator.Text;
        }

        private void HavocDarknessUnits_ValueChanged(object sender, EventArgs e)
        {
            S.HavocDarknessUnits = (int)HavocDarknessUnits.Value;
        }

        private void HavocBlurHp_ValueChanged(object sender, EventArgs e)
        {
            S.HavocBlurHp = (int)HavocBlurHp.Value;
        }

        private void HavocBlurOperator_SelectedIndexChanged(object sender, EventArgs e)
        {
            S.HavocBlurOperator = HavocBlurOperator.Text;
        }

        private void HavocBlurUnits_ValueChanged(object sender, EventArgs e)
        {
            S.HavocBlurUnits = (int)HavocBlurUnits.Value;
        }

        private void HavocChaosNovaHp_ValueChanged(object sender, EventArgs e)
        {
            S.HavocChaosNovaHp = (int)HavocChaosNovaHp.Value;
        }

        private void HavocChaosNovaOperator_SelectedIndexChanged(object sender, EventArgs e)
        {
            S.HavocChaosNovaOperator = HavocChaosNovaOperator.Text;
        }

        private void HavocChaosNovaUnits_ValueChanged(object sender, EventArgs e)
        {
            S.HavocChaosNovaUnits = (int)HavocChaosNovaUnits.Value;
        }

        private void btnHavocPotionBagLookup_Click(object sender, EventArgs e)
        {
            ViewBagsForm frmBags = new ViewBagsForm();
            if (frmBags.ShowDialog() == DialogResult.OK)
            {
                if (frmBags.SelectedItem != null)
                {
                    HavocPotionsHpList.Text += frmBags.SelectedItem.ItemInfo.Id;
                }
            }
        }

      

        private void HavocPotionsHp_ValueChanged(object sender, EventArgs e)
        {
            S.HavocHealthPotionHp = (int)HavocPotionsHp.Value;
        }
        #endregion

        #region Vengeance Events

        private void VengeanceAllowTaunt_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowTaunt = VengeanceAllowTaunt.Checked;
        }

        private void VengeanceUseDemonSpikes_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowDemonSpikes = VengeanceUseDemonSpikes.Checked;
            VengeanceDemonSpikesHP.Enabled = VengeanceUseDemonSpikes.Checked;
        }

        private void VengeanceUseEmpowerWards_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceEmpowerWards = VengeanceUseEmpowerWards.Checked;
        }

        #endregion

        private void VengeanceUseSoulCleave_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowSoulCleave = VengeanceUseSoulCleave.Checked;
            VengeanceSoulCleaveHp.Enabled = VengeanceUseSoulCleave.Checked;
        }

        private void VengeanceUseFieryBrand_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowFieryBrand = VengeanceUseFieryBrand.Checked;
            VengeanceFieryBrandHp.Enabled = VengeanceUseFieryBrand.Checked;
        }

        private void VengeanceDemonSpikesHP_ValueChanged(object sender, EventArgs e)
        {
            S.VengeanceDemonSpikesHp = (int)VengeanceDemonSpikesHP.Value;
        }

        private void VengeanceSoulCleaveHp_ValueChanged(object sender, EventArgs e)
        {
            S.VengeanceSoulCleaveHp = (int)VengeanceSoulCleaveHp.Value;
        }

        private void VengeanceFieryBrandHp_ValueChanged(object sender, EventArgs e)
        {
            S.VengeanceFieryBrandHp = (int)VengeanceFieryBrandHp.Value;
        }

        private void VengeanceCombatSoulCleavePain_ValueChanged(object sender, EventArgs e)
        {
            S.VengeanceCombatSoulCleavePain = (int)VengeanceCombatSoulCleavePain.Value;
        }

        private void VengeanceCombatUseSoulCleave_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceCombatAllowSoulCleave = VengeanceCombatUseSoulCleave.Checked;
            VengeanceCombatSoulCleavePain.Enabled = VengeanceCombatUseSoulCleave.Checked;
        }
    }
}
