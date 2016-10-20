using Illidari.Core.IllidariSettings;
using Styx;
using Styx.Common;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

using M = Illidari.Main;

namespace Illidari.Core.Managers
{
    class Hotkeys
    {
        public static bool RotationOnlyOn { get; set; }
        public static bool VengeanceAoEOn { get; set; }
        public static bool VengeanceDefensiveOn { get; set; }
        public static bool HavocAoEOn { get; set; }
        public static bool HavocOffensiveOn { get; set; }
        public static bool cooldownsOn { get; set; }
        public static bool manualOn { get; set; }
        public static bool keysRegistered { get; set; }
        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        #region [Method] - Hotkey Registration
        public static void registerHotkeys()
        {
            if (keysRegistered)
                return;
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Keys));

            // ++++++++++ General AoE +++++++++++++++
            if (!string.IsNullOrEmpty(HotkeySettings.Instance.HotkeyVengeanceAoeKey) && HotkeySettings.Instance.HotkeyVengeanceAoeModifier > 0)
            {
                HotkeysManager.Register("AoEOn", (Keys)converter.ConvertFromString(HotkeySettings.Instance.HotkeyVengeanceAoeKey), (ModifierKeys)HotkeySettings.Instance.HotkeyVengeanceAoeModifier, ret =>
                {
                    VengeanceAoEOn = !VengeanceAoEOn;
                    HavocAoEOn = !HavocAoEOn;
                    StyxWoW.Overlay.AddToast((VengeanceAoEOn ? "AoE Mode: Enabled!" : "AoE Mode: Disabled!"), 2000);
                });
            }

            if (!string.IsNullOrEmpty(HotkeySettings.Instance.HotkeyGeneralRotationOnlyKey) && HotkeySettings.Instance.HotkeyGeneralRotationOnlyModifier > 0)
            {
                HotkeysManager.Register("RotationOnly", (Keys)converter.ConvertFromString(HotkeySettings.Instance.HotkeyGeneralRotationOnlyKey), (ModifierKeys)HotkeySettings.Instance.HotkeyGeneralRotationOnlyModifier, ret =>
                {
                    RotationOnlyOn = !RotationOnlyOn;
                    StyxWoW.Overlay.AddToast((RotationOnlyOn ? "Rotation Only: Enabled!" : "Rotation Only: Disabled!"), 2000);
                });
            }

            if (Me.Specialization == WoWSpec.DemonHunterVengeance)
            {
                // ++++++++ VENGEANCE SPEC ONLY ++++++++++
                
                if (HotkeySettings.Instance.HotkeyVengeanceDefensiveModifier > 0 && !string.IsNullOrEmpty(HotkeySettings.Instance.HotkeyVengeanceDefensiveKey))
                {
                    HotkeysManager.Register("VengeanceDefensiveOn", (Keys)converter.ConvertFromString(HotkeySettings.Instance.HotkeyVengeanceDefensiveKey), (ModifierKeys)HotkeySettings.Instance.HotkeyVengeanceDefensiveModifier, ret =>
                    {
                        VengeanceDefensiveOn = !VengeanceDefensiveOn;
                        StyxWoW.Overlay.AddToast((VengeanceDefensiveOn ? "Defensive Mode: Enabled!" : "Defensive Mode: Disabled!"), 2000);

                    });
                }
            }

            if (Me.Specialization == WoWSpec.DemonHunterHavoc)
            {
                if (HotkeySettings.Instance.HotkeyHavocOffensiveModifier > 0 && !string.IsNullOrEmpty(HotkeySettings.Instance.HotkeyHavocOffensiveKey))
                {
                    HotkeysManager.Register("HavocOffensiveOn", (Keys)converter.ConvertFromString(HotkeySettings.Instance.HotkeyHavocOffensiveKey), (ModifierKeys)HotkeySettings.Instance.HotkeyHavocOffensiveModifier, ret =>
                    {
                        HavocOffensiveOn = !HavocOffensiveOn;
                        StyxWoW.Overlay.AddToast((HavocOffensiveOn ? "Offensive Cooldowns: Enabled!" : "Offensive Cooldowns: Disabled!"), 2000);
                    });
                }
            }

            HotkeysManager.Register("ManualOn", Keys.S, ModifierKeys.Alt, ret =>
                {
                    manualOn = !manualOn;
                    StyxWoW.Overlay.AddToast((manualOn ? "Manual Mode: Enabled!" : "Manual Mode: Disabled!"), 2000);
                });
            keysRegistered = true;
            StyxWoW.Overlay.AddToast(("Hotkeys: Registered!"), 2000);
            Logging.Write(Colors.Green, "Hotkeys: Registered!");
        }
        #endregion

        #region [Method] - Hotkey Removal
        public static void removeHotkeys()
        {
            if (!keysRegistered)
                return;
            HotkeysManager.Unregister("AoEOn");
            HotkeysManager.Unregister("cooldownsOn");
            HotkeysManager.Unregister("manualOn");
            VengeanceAoEOn = false;
            VengeanceDefensiveOn = false;
            cooldownsOn = false;
            manualOn = false;
            keysRegistered = false;
            StyxWoW.Overlay.AddToast(("Hotkeys: Removed!"), 2000);
            Logging.Write(Colors.Red, "Hotkeys: Removed!");
        }
        #endregion
    }
}
