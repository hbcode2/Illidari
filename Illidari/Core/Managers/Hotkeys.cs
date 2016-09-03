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
        public static bool AoEOn { get; set; }
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
            if (Me.Specialization == WoWSpec.DemonHunterVengeance)
            {
                if (!string.IsNullOrEmpty(M.IS.HotkeyVengeanceAoeKey) && M.IS.HotkeyVengeanceAoeModifier > 0)
                {
                    HotkeysManager.Register("AoEOn", (Keys)converter.ConvertFromString(M.IS.HotkeyVengeanceAoeKey), (ModifierKeys)M.IS.HotkeyVengeanceAoeModifier, ret =>
                    {
                        AoEOn = !AoEOn;
                        StyxWoW.Overlay.AddToast((AoEOn ? "AoE Mode: Enabled!" : "AoE Mode: Disabled!"), 2000);
                    });
                }
            }

            HotkeysManager.Register("cooldownsOn", Keys.E, ModifierKeys.Alt, ret =>
                {
                    cooldownsOn = !cooldownsOn;
                    StyxWoW.Overlay.AddToast((cooldownsOn ? "Cooldowns: Enabled!" : "Cooldowns: Disabled!"), 2000);
                });
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
            AoEOn = false;
            cooldownsOn = false;
            manualOn = false;
            keysRegistered = false;
            StyxWoW.Overlay.AddToast(("Hotkeys: Removed!"), 2000);
            Logging.Write(Colors.Red, "Hotkeys: Removed!");
        }
        #endregion
    }
}
