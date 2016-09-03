using Illidari.Core.IllidariSettings;
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
        private bool _isLoading = true;
        private IllidariSettings S = new IllidariSettings();
        private IllidariSettings originalSettings = Main.IS;
        //private IllidariSettings S => S;

        public IllidariSettingsForm()
        {
            _isLoading = true;

            InitializeComponent();

            #region Load Comboboxes
            HavocUseAgilityPotionCooldown.DataSource = Enum.GetValues(typeof(IllidariSettings.CooldownTypes));
            HavocUseMetamorphosisCooldown.DataSource = Enum.GetValues(typeof(IllidariSettings.CooldownTypes));
            #endregion

            #region General Settings load
            GeneralEnableDebug.Checked = S.GeneralDebug;
            GeneralEnableFacing.Checked = S.GeneralFacing;
            GeneralEnableMovement.Checked = S.GeneralMovement;
            GeneralEnableTargeting.Checked = S.GeneralTargeting;
            #endregion

            #region Havoc Settings load

            #region Defensive
            HavocUseAgilityFlask.Checked = S.HavocUseAgilityFlask;
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

            #region Combat
            HavocFelRushOnPull.Checked = S.HavocFelRushOnPull;
            HavocFelRushSingleTarget.Checked = S.HavocFelRushSingleTarget;
            HavocFelRushAoe.Checked = S.HavocFelRushAoe;
            HavocVengefulReatreatSingleTarget.Checked = S.HavocVengefulReatreatSingleTarget;
            HavocVengefulReatreatAoe.Checked = S.HavocVengefulReatreatAoe;

            #endregion

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

            VengeanceInterruptConsumeMagic.Checked = S.VengeanceAllowInterruptConsumeMagic;

            VengeanceInterruptSigilOfSilence.Checked = S.VengeanceAllowInterruptSigilOfSilence;

            VengeanceInterruptSigilOfMisery.Checked = S.VengeanceAllowInterruptSigilOfMisery;

            VengeanceStunSigilOfMisery.Checked = S.VengeanceAllowStunSigilOfMisery;
            VengeanceStunSigilOfMiseryCount.Value = S.VengeanceStunSigilOfMiseryCount;
            VengeanceStunSigilOfMiseryCount.Enabled = S.VengeanceAllowStunSigilOfMisery;

            VengeanceUseMetamorphosis.Checked = S.VengeanceAllowMetamorphosis;
            VengeanceMetamorphosisHp.Value = S.VengeanceMetamorphosisHp;
            VengeanceMetamorphosisHp.Enabled = S.VengeanceAllowMetamorphosis;

            VengeanceUseSoulBarrier.Checked = S.VengeanceAllowSoulBarrier;
            VengeanceSoulBarrierHp.Value = S.VengeanceSoulBarrierHp;
            VengeanceSoulBarrierHp.Enabled = S.VengeanceAllowSoulBarrier;

            VengeanceUseSoulCarver.Checked = S.VengeanceAllowSoulCarver;
            VengeanceSoulCarverHp.Value = S.VengeanceSoulCarverHp;
            VengeanceSoulCarverHp.Enabled = S.VengeanceAllowSoulCarver;

            VengeancePreferPullWithFelblade.Checked = S.VengeancePreferPullWithFelblade;

            #endregion

            #region Hotkeys load
            MessageBox.Show(S.HotkeyVengeanceAoeModifier.ToString() + ":" + S.HotkeyVengeanceAoeKey);
            LoadButtonText(S.HotkeyVengeanceAoeModifier, S.HotkeyVengeanceAoeKey, btnHotkeysVengeanceAoe);



            #endregion


        }
        private void LoadButtonText(int modifierKey, string key, Button btn)
        {
            if (modifierKey <= 0 || string.IsNullOrEmpty(key)) { btn.Text = "Click to Set"; return; }

            bool shift = false;
            bool alt = false;
            bool ctrl = false;

            // singles
            if (modifierKey == (int)Styx.Common.ModifierKeys.Alt) { alt = true; shift = false; ctrl = false; }
            if (modifierKey == (int)Styx.Common.ModifierKeys.Shift) { alt = false; shift = true; ctrl = false; }
            if (modifierKey == (int)Styx.Common.ModifierKeys.Control) { alt = false; shift = false; ctrl = true; }

            // doubles
            if (modifierKey == (int)Styx.Common.ModifierKeys.Alt + (int)Styx.Common.ModifierKeys.Control) { alt = true; shift = false; ctrl = true; }
            if (modifierKey == (int)Styx.Common.ModifierKeys.Alt + (int)Styx.Common.ModifierKeys.Shift) { alt = true; shift = true; ctrl = false; }
            if (modifierKey == (int)Styx.Common.ModifierKeys.Control + (int)Styx.Common.ModifierKeys.Shift) { alt = false; shift = true; ctrl = true; }

            // one triple
            if (modifierKey == (int)Styx.Common.ModifierKeys.Alt + (int)Styx.Common.ModifierKeys.Control + (int)Styx.Common.ModifierKeys.Shift) { alt = true; shift = true; ctrl = true; }
            MessageBox.Show("shift:" + shift.ToString() + ", alt:" + alt.ToString() + ", ctrl:" + ctrl.ToString());
            string btnText = GetKeyModifierText(alt, shift, ctrl, key);
            btn.Text = btnText;
        }

        private void IllidariSettingsForm_Load(object sender, EventArgs e)
        {
            //IllidariSettings sett = new IllidariSettings();

            // must set these on form load as the initialization process populates the dropdowns with data
            HavocUseAgilityPotionCooldown.SelectedItem = S.HavocUseAgilityPotionCooldown;
            HavocUseMetamorphosisCooldown.SelectedItem = S.HavocUseMetamorphosisCooldown;

            _isLoading = false;
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
            Type type = S.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "SettingsPath") { continue; }
                if (property.PropertyType == typeof(List<uint>))
                {
                    List<uint> uintList = (List<uint>)property.GetValue(S, null);
                    foreach (var uintItem in uintList)
                    {
                        Illidari.Core.Utilities.Log.infoLog(string.Format($"{property.Name}: {uintItem}"), Core.Helpers.Common.InfoColor);
                    }
                }
                else
                {
                    Illidari.Core.Utilities.Log.infoLog(string.Format($"{property.Name}: {property.GetValue(S, null)}"), Core.Helpers.Common.InfoColor);
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
            Dictionary<string, object> oldProperties = GetPropertiesOfSettings(Main.IS);
            Dictionary<KeyValuePair<string, object>, KeyValuePair<string, object>> changedProperties = new Dictionary<KeyValuePair<string, object>, KeyValuePair<string, object>>();
            // grab the list of potions and save them
            string[] havocPotionsHp = HavocPotionsHpList.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            S.HavocHealthPotionListSetting = String.Join("|", havocPotionsHp);
            S.Save();

            Dictionary<string, object> newProperties = GetPropertiesOfSettings(S);

            foreach (var oldItem in oldProperties)
            {
                foreach (var newItem in newProperties)
                {
                    if (oldItem.Key == newItem.Key && oldItem.Value.ToString() != newItem.Value.ToString())
                    {
                        //MessageBox.Show(string.Format($"oldItem.Key:'{oldItem.Key}'='{newItem.Key}' && '{oldItem.Value.ToString()}' == '{newItem.Value.ToString()}'"));
                        changedProperties.Add(oldItem, newItem);
                    }
                }
            }

            if (changedProperties.Count > 0)
            {
                Core.Utilities.Log.debugLog("Properties Changed:");
            }
            foreach (var changedItem in changedProperties)
            {
                Core.Utilities.Log.debugLog(string.Format($"{changedItem.Key.Key}: from '{changedItem.Key.Value}' to '{changedItem.Value.Value}'"));
            }

            Main.IS = new IllidariSettings();



            this.Close();
        }

        private Dictionary<string, object> GetPropertiesOfSettings(IllidariSettings settings)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            Type type = settings.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name == "SettingsPath") { continue; }
                if (property.PropertyType == typeof(List<uint>))
                {
                    List<uint> uintList = (List<uint>)property.GetValue(settings, null);
                    foreach (var uintItem in uintList)
                    {
                        //Core.Utilities.Log.infoLog(string.Format($"{property.Name}: {uintItem}"), Core.Helpers.Common.InfoColor);
                    }
                }
                else
                {
                    //Core.Utilities.Log.infoLog(string.Format($"{property.Name}: {property.GetValue(settings, null)}"), Core.Helpers.Common.InfoColor);
                    dict.Add(property.Name, property.GetValue(settings, null));
                }

            }
            return dict;
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

        #region Defensive
        private void HavocUseAgilityFlask_CheckedChanged(object sender, EventArgs e)
        {
            S.HavocUseAgilityFlask = HavocUseAgilityFlask.Checked;
        }
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

        #region Combat

        private void HavocFelRushOnPull_CheckedChanged(object sender, EventArgs e)
        {
            S.HavocFelRushOnPull = HavocFelRushOnPull.Checked;
        }

        private void HavocFelRushSingleTarget_CheckedChanged(object sender, EventArgs e)
        {
            S.HavocFelRushSingleTarget = HavocFelRushSingleTarget.Checked;
        }

        private void HavocVengefulReatreatSingleTarget_CheckedChanged(object sender, EventArgs e)
        {
            S.HavocVengefulReatreatSingleTarget = HavocVengefulReatreatSingleTarget.Checked;
        }

        private void HavocFelRushAoe_CheckedChanged(object sender, EventArgs e)
        {
            S.HavocFelRushAoe = HavocFelRushAoe.Checked;
        }

        private void HavocVengefulReatreatAoe_CheckedChanged(object sender, EventArgs e)
        {
            S.HavocVengefulReatreatAoe = HavocVengefulReatreatAoe.Checked;
        }
        #endregion

        private void HavocUseAgilityPotionCooldown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                IllidariSettings.CooldownTypes potionCooldown;
                //MessageBox.Show(HavocUseAgilityPotionCooldown.SelectedValue.ToString() + "\r\n" + HavocUseAgilityPotionCooldown.SelectedText + "\r\n" + HavocUseAgilityPotionCooldown.SelectedItem.ToString());
                Enum.TryParse(HavocUseAgilityPotionCooldown.SelectedValue.ToString(), out potionCooldown);
                //MessageBox.Show(potionCooldown.ToString());
                S.HavocUseAgilityPotionCooldown = potionCooldown;
            }
        }

        private void HavocUseMetamorphosisCooldown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoading)
            {
                IllidariSettings.CooldownTypes metamorphosiscd;
                Enum.TryParse(HavocUseMetamorphosisCooldown.SelectedValue.ToString(), out metamorphosiscd);
                S.HavocUseMetamorphosisCooldown = metamorphosiscd;
            }
        }

        #endregion

        #region Vengeance Events

        private void VengeanceMetamorphosisHp_ValueChanged(object sender, EventArgs e)
        {
            S.VengeanceMetamorphosisHp = (int)VengeanceMetamorphosisHp.Value;
        }

        private void VengeanceSoulBarrierHp_ValueChanged(object sender, EventArgs e)
        {
            S.VengeanceSoulBarrierHp = (int)VengeanceSoulBarrierHp.Value;
        }

        private void VengeanceSoulCarverHp_ValueChanged(object sender, EventArgs e)
        {
            S.VengeanceSoulCarverHp = (int)VengeanceSoulCarverHp.Value;
        }
        private void VengeancePreferPullWithFelblade_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeancePreferPullWithFelblade = VengeancePreferPullWithFelblade.Checked;
        }
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

        private void VengeanceInterruptConsumeMagic_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowInterruptConsumeMagic = VengeanceInterruptConsumeMagic.Checked;
        }

        private void VengeanceInterruptSigilOfSilence_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowInterruptSigilOfSilence = VengeanceInterruptSigilOfSilence.Checked;
        }

        private void VengeanceInterruptSigilOfMisery_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowInterruptSigilOfMisery = VengeanceInterruptSigilOfMisery.Checked;
        }

        private void VengeanceStunSigilOfMisery_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowStunSigilOfMisery = VengeanceStunSigilOfMisery.Checked;
            VengeanceStunSigilOfMiseryCount.Enabled = VengeanceStunSigilOfMisery.Checked;
        }

        private void VengeanceStunSigilOfMiseryCount_ValueChanged(object sender, EventArgs e)
        {
            S.VengeanceStunSigilOfMiseryCount = (int)VengeanceStunSigilOfMiseryCount.Value;
        }

        private void VengeanceUseMetamorphosis_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowMetamorphosis = VengeanceUseMetamorphosis.Checked;
            VengeanceMetamorphosisHp.Enabled = VengeanceUseMetamorphosis.Checked;
        }

        private void VengeanceUseSoulBarrier_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowSoulBarrier = VengeanceUseSoulBarrier.Checked;
            VengeanceSoulBarrierHp.Enabled = VengeanceUseSoulBarrier.Checked;
        }

        private void VengeanceUseSoulCarver_CheckedChanged(object sender, EventArgs e)
        {
            S.VengeanceAllowSoulCarver = VengeanceUseSoulCarver.Checked;
            VengeanceSoulCarverHp.Enabled = VengeanceUseSoulCarver.Checked;
        }




        #endregion

        #region Hotkey Methods

        private bool captureKeyPress = false;
        private bool controlPressed = false;
        private bool altPressed = false;
        private bool shiftPressed = false;
        private string keyPressed = "";
        private void CheckIfKeyPressed(KeyEventArgs e)
        {
            if (captureKeyPress)
            {
                if (e.Alt) { altPressed = true; }
                if (e.Control) { controlPressed = true; }
                if (e.Shift) { shiftPressed = true; }
                bool isLetterOrDigit = char.IsLetterOrDigit((char)e.KeyCode);
                if (isLetterOrDigit) { keyPressed = new KeysConverter().ConvertToString(e.KeyCode); }
            }
        }


        private void ResetKeys()
        {
            captureKeyPress = false;
            altPressed = false;
            controlPressed = false;
            shiftPressed = false;
            keyPressed = "";
        }
        private string GetKeyModifierText()
        {
            string text = "";
            if ((altPressed || controlPressed || shiftPressed) && !String.IsNullOrEmpty(keyPressed))
            {
                #region Control + other stuff
                if (controlPressed && !altPressed && !shiftPressed)
                {
                    return string.Format($"Ctrl + {keyPressed.ToUpper()}");
                }
                if (controlPressed && altPressed && !shiftPressed)
                {
                    return string.Format($"Ctrl + Alt + {keyPressed.ToUpper()}");
                }
                if (controlPressed && !altPressed && shiftPressed)
                {
                    return string.Format($"Ctrl + Shift + {keyPressed.ToUpper()}");
                }
                if (controlPressed && altPressed && shiftPressed)
                {
                    return string.Format($"Ctrl + Alt + Shift + {keyPressed.ToUpper()}");
                }
                #endregion  

                if (!controlPressed && altPressed && !shiftPressed)
                {
                    return string.Format($"Alt + {keyPressed.ToUpper()}");
                }
                if (!controlPressed && altPressed && shiftPressed)
                {
                    return string.Format($"Alt + Shift + {keyPressed.ToUpper()}");
                }
                if (!controlPressed && !altPressed && shiftPressed)
                {
                    return string.Format($"Shift + {keyPressed.ToUpper()}");
                }
            }
            return text;
        }

        private string GetKeyModifierText(bool alt, bool shift, bool ctrl, string key)
        {
            string text = "";
            if ((alt || ctrl || shift) && !String.IsNullOrEmpty(key))
            {
                #region Control + other stuff
                if (ctrl && !alt && !shift)
                {
                    return string.Format($"Ctrl + {key.ToUpper()}");
                }
                if (ctrl && alt && !shift)
                {
                    return string.Format($"Ctrl + Alt + {key.ToUpper()}");
                }
                if (ctrl && !alt && shift)
                {
                    return string.Format($"Ctrl + Shift + {key.ToUpper()}");
                }
                if (ctrl && alt && shift)
                {
                    return string.Format($"Ctrl + Alt + Shift + {key.ToUpper()}");
                }
                #endregion  

                if (!ctrl && alt && !shift)
                {
                    return string.Format($"Alt + {key.ToUpper()}");
                }
                if (!ctrl && alt && shift)
                {
                    return string.Format($"Alt + Shift + {key.ToUpper()}");
                }
                if (!ctrl && !alt && shift)
                {
                    return string.Format($"Shift + {key.ToUpper()}");
                }
            }
            return text;
        }

        private bool DidPressCorrectKey()
        {
            if (captureKeyPress)
            {
                if ((altPressed || controlPressed || shiftPressed) && !string.IsNullOrEmpty(keyPressed))
                {
                    return true;
                }
            }
            return false;
        }

        private int GetKeyModifierPressed()
        {
            if (altPressed || controlPressed || shiftPressed)
            {
                int modifierKey = 0;
                if (altPressed) { modifierKey += (int)Styx.Common.ModifierKeys.Alt; }
                if (controlPressed) { modifierKey += (int)Styx.Common.ModifierKeys.Control; }
                if (shiftPressed) { modifierKey += (int)Styx.Common.ModifierKeys.Shift; }
                return modifierKey;
            }
            return 0;
        }
        #endregion

        #region Hotkey - Vengeance - AoE

        private void btnHotkeysVengeanceAoe_KeyDown(object sender, KeyEventArgs e)
        {
            CheckIfKeyPressed(e);
            btnHotkeysVengeanceAoe.Text = GetKeyModifierText();
        }


        private void btnHotkeysVengeanceAoe_Click(object sender, EventArgs e)
        {
            captureKeyPress = true;
            btnHotkeysVengeanceAoe.Text = "";
        }

        private void btnHotkeysVengeanceAoe_KeyUp(object sender, KeyEventArgs e)
        {
            if (DidPressCorrectKey())
            {
                S.HotkeyVengeanceAoeModifier = GetKeyModifierPressed();
                S.HotkeyVengeanceAoeKey = keyPressed;
            }
            ResetKeys();
        }


        private void btnHotkeysVengeanceDefensiveCooldowns_Click(object sender, EventArgs e)
        {
            captureKeyPress = true;
            btnHotkeysVengeanceDefensiveCooldowns.Text = "";
        }

        private void btnHotkeysVengeanceDefensiveCooldowns_KeyDown(object sender, KeyEventArgs e)
        {
            CheckIfKeyPressed(e);
            btnHotkeysVengeanceDefensiveCooldowns.Text = GetKeyModifierText();
        }

        private void btnHotkeysVengeanceDefensiveCooldowns_KeyUp(object sender, KeyEventArgs e)
        {
            if (DidPressCorrectKey())
            {
                S.HotkeyVengeanceDefensiveKey = keyPressed;
                S.HotkeyVengeanceDefensiveModifier = GetKeyModifierPressed();
            }
            ResetKeys();
        }
    }
}
