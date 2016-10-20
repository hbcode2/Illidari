using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Illidari.Core.IllidariSettings
{
    public partial class SettingsForm : Form
    {
        private static HotkeySettings S => HotkeySettings.Instance;
        
        public SettingsForm()
        {
            InitializeComponent();

            LoadButtonText(S.HotkeyVengeanceAoeModifier, S.HotkeyVengeanceAoeKey, btnHotkeysVengeanceAoe);
            LoadButtonText(S.HotkeyVengeanceDefensiveModifier, S.HotkeyVengeanceDefensiveKey, btnHotkeysVengeanceDefensiveCooldowns);
            checkHotkeysVengeanceDefensiveDemonSpikes.Checked = S.HotkeyVengeanceDefensiveDemonSpikes;
            checkHotkeysVengeanceDefensiveEmpowerWards.Checked = S.HotkeyVengeanceDefensiveEmpowerWards;
            checkHotkeysVengeanceDefensiveFieryBrand.Checked = S.HotkeyVengeanceDefensiveFieryBrand;
            checkHotkeysVengeanceDefensiveMetamorphosis.Checked = S.HotkeyVengeanceDefensiveMetamorphosis;
            checkHotkeysVengeanceDefensiveSoulBarrier.Checked = S.HotkeyVengeanceDefensiveSoulBarrier;
            checkHotkeysVengeanceDefensiveSoulCarver.Checked = S.HotkeyVengeanceDefensiveSoulCarver;
            checkHotkeysVengeanceDefensiveSoulCleave.Checked = S.HotkeyVengeanceDefensiveSoulCleave;
            checkHotkeysVengeanceDefensiveFelDevastation.Checked = S.HotkeyVengeanceDefensiveFelDevastation;

            LoadButtonText(S.HotkeyHavocOffensiveModifier, S.HotkeyHavocOffensiveKey, btnHotkeysHavocOffensiveCooldowns);
            checkHotkeysHavocOffensiveAgilityPotion.Checked = S.HotkeyHavocOffensiveAgilityPotion;
            checkHotkeysHavocOffensiveFoTI.Checked = S.HotkeyHavocOffensiveFoTI;
            checkHotkeysHavocOffensiveMetamorphosis.Checked = S.HotkeyHavocOffensiveMetamorphosis;

            LoadButtonText(S.HotkeyGeneralRotationOnlyModifier, S.HotkeyGeneralRotationOnlyKey, btnHotkeysGeneralRotationOnly);

        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            pgGeneral.SelectedObject = GeneralSettings.Instance;
            GeneralSettings.Instance.Load();

            pgHavoc.SelectedObject = HavocSettings.Instance;
            HavocSettings.Instance.Load();

            pgVengeance.SelectedObject = VengeanceSettings.Instance;
            VengeanceSettings.Instance.Load();
        }


        #region Hotkeys

        #region Hotkey Methods

        private bool captureKeyPress = false;
        private bool controlPressed = false;
        private bool altPressed = false;
        private bool shiftPressed = false;
        private string keyPressed = "";

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
            //MessageBox.Show("shift:" + shift.ToString() + ", alt:" + alt.ToString() + ", ctrl:" + ctrl.ToString());
            string btnText = GetKeyModifierText(alt, shift, ctrl, key);
            btn.Text = btnText;
        }

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
        private enum KeybindTypes
        {
            VengeanceAoe,
            VengeanceDefensive,
            HavocAoe,
            HavocOffensive,
            GeneralRotationOnly
        }
        private void ClearButtonHotkey(Button btn, KeybindTypes kbType)
        {
            btn.Text = "Click to Set"; ResetKeys();
            if (kbType == KeybindTypes.VengeanceAoe)
            {
                S.HotkeyVengeanceAoeKey = "";
                S.HotkeyVengeanceAoeModifier = 0;
            }
            else if (kbType == KeybindTypes.VengeanceDefensive)
            {
                S.HotkeyVengeanceDefensiveModifier = 0;
                S.HotkeyVengeanceDefensiveKey = "";
                S.HotkeyVengeanceDefensiveDemonSpikes = false;
                S.HotkeyVengeanceDefensiveEmpowerWards = false;
                S.HotkeyVengeanceDefensiveFieryBrand = false;
                S.HotkeyVengeanceDefensiveMetamorphosis = false;
                S.HotkeyVengeanceDefensiveSoulBarrier = false;
                S.HotkeyVengeanceDefensiveSoulCarver = false;
                S.HotkeyVengeanceDefensiveSoulCleave = false;
            }
            else if (kbType == KeybindTypes.HavocOffensive)
            {
                S.HotkeyHavocOffensiveKey = "";
                S.HotkeyHavocOffensiveModifier = 0;
            }
            ResetKeys();
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

        #region Hotkey - AoE

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
            if (e.KeyCode == Keys.Escape) { ClearButtonHotkey(btnHotkeysVengeanceAoe, KeybindTypes.VengeanceAoe); return; }
            if (DidPressCorrectKey())
            {
                S.HotkeyVengeanceAoeModifier = GetKeyModifierPressed();
                S.HotkeyVengeanceAoeKey = keyPressed;
            }
            ResetKeys();
        }


        #endregion

        #region Hotkey - Vengeance - Defensive
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
            if (e.KeyCode == Keys.Escape) { ClearButtonHotkey(btnHotkeysVengeanceDefensiveCooldowns, KeybindTypes.VengeanceDefensive); return; }
            if (DidPressCorrectKey())
            {
                S.HotkeyVengeanceDefensiveKey = keyPressed;
                S.HotkeyVengeanceDefensiveModifier = GetKeyModifierPressed();
            }
            ResetKeys();
        }

        private void checkHotkeysVengeanceDefensiveDemonSpikes_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyVengeanceDefensiveDemonSpikes = checkHotkeysVengeanceDefensiveDemonSpikes.Checked;
        }

        private void checkHotkeysVengeanceDefensiveSoulCleave_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyVengeanceDefensiveSoulCleave = checkHotkeysVengeanceDefensiveSoulCleave.Checked;
        }

        private void checkHotkeysVengeanceDefensiveFieryBrand_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyVengeanceDefensiveFieryBrand = checkHotkeysVengeanceDefensiveFieryBrand.Checked;
        }

        private void checkHotkeysVengeanceDefensiveMetamorphosis_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyVengeanceDefensiveMetamorphosis = checkHotkeysVengeanceDefensiveMetamorphosis.Checked;
        }

        private void checkHotkeysVengeanceDefensiveSoulBarrier_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyVengeanceDefensiveSoulBarrier = checkHotkeysVengeanceDefensiveSoulBarrier.Checked;
        }

        private void checkHotkeysVengeanceDefensiveSoulCarver_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyVengeanceDefensiveSoulCarver = checkHotkeysVengeanceDefensiveSoulCarver.Checked;
        }

        private void checkHotkeysVengeanceDefensiveEmpowerWards_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyVengeanceDefensiveEmpowerWards = checkHotkeysVengeanceDefensiveEmpowerWards.Checked;
        }
        private void checkHotkeysVengeanceDefensiveFelDevastation_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyVengeanceDefensiveFelDevastation = checkHotkeysVengeanceDefensiveFelDevastation.Checked;
        }

        #endregion

        #region Hotkey - General - Rotation Only
        private void btnHotkeysGeneralRotationOnly_Click(object sender, EventArgs e)
        {
            captureKeyPress = true;
            btnHotkeysGeneralRotationOnly.Text = "";
        }

        private void btnHotkeysGeneralRotationOnly_KeyDown(object sender, KeyEventArgs e)
        {
            CheckIfKeyPressed(e);
            btnHotkeysGeneralRotationOnly.Text = GetKeyModifierText();
        }

        private void btnHotkeysGeneralRotationOnly_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { ClearButtonHotkey(btnHotkeysGeneralRotationOnly, KeybindTypes.GeneralRotationOnly); }
            if (DidPressCorrectKey())
            {
                S.HotkeyGeneralRotationOnlyKey = keyPressed;
                S.HotkeyGeneralRotationOnlyModifier = GetKeyModifierPressed();
            }
            ResetKeys();
        }
        #endregion

        #region Hotkey - Havoc - Offensive
        private void btnHotkeysHavocOffensiveCooldowns_Click(object sender, EventArgs e)
        {
            captureKeyPress = true;
            btnHotkeysHavocOffensiveCooldowns.Text = "";
        }

        private void btnHotkeysHavocOffensiveCooldowns_KeyDown(object sender, KeyEventArgs e)
        {
            CheckIfKeyPressed(e);
            btnHotkeysHavocOffensiveCooldowns.Text = GetKeyModifierText();
        }

        private void btnHotkeysHavocOffensiveCooldowns_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { ClearButtonHotkey(btnHotkeysHavocOffensiveCooldowns, KeybindTypes.HavocOffensive); return; }
            if (DidPressCorrectKey())
            {
                S.HotkeyHavocOffensiveKey = keyPressed;
                S.HotkeyHavocOffensiveModifier = GetKeyModifierPressed();
            }
            ResetKeys();
        }

        private void checkHotkeysHavocOffensiveAgilityPotion_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyHavocOffensiveAgilityPotion = checkHotkeysHavocOffensiveAgilityPotion.Checked;
        }

        private void checkHotkeysHavocOffensiveFoTI_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyHavocOffensiveFoTI = checkHotkeysHavocOffensiveFoTI.Checked;
        }

        private void checkHotkeysHavocOffensiveMetamorphosis_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyHavocOffensiveMetamorphosis = checkHotkeysHavocOffensiveMetamorphosis.Checked;
        }

        private void checkHotkeysHavocOffensiveNemesis_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyHavocOffensiveNemesis = checkHotkeysHavocOffensiveNemesis.Checked;
        }

        private void checkHotkeysHavocOffensiveChaosBlades_CheckedChanged(object sender, EventArgs e)
        {
            S.HotkeyHavocOffensiveChaosBlades = checkHotkeysHavocOffensiveChaosBlades.Checked;
        } 

        #endregion

        #endregion


    }
}
