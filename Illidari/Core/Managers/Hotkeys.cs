using Styx;
using Styx.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace Illidari.Core.Managers
{
    class Hotkeys
    {
        public static bool AoEOn { get; set; }
        public static bool cooldownsOn { get; set; }
        public static bool manualOn { get; set; }
        public static bool keysRegistered { get; set; }

        #region [Method] - Hotkey Registration
        public static void registerHotkeys()
        {
            if (keysRegistered)
                return;
            HotkeysManager.Register("AoEOn", (Keys)char.ToUpper('q'), ModifierKeys.Alt, ret =>
                {
                    AoEOn = !AoEOn;
                    StyxWoW.Overlay.AddToast((AoEOn ? "AoE Mode: Enabled!" : "AoE Mode: Disabled!"), 2000);
                });
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
