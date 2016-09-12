using Styx;
using Styx.CommonBot.Coroutines;
using Styx.CommonBot.Frames;
using Styx.WoWInternals.WoWObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using L = Illidari.Core.Utilities.Log;

namespace Illidari.Core
{
    class Item
    {
        private static LocalPlayer Me { get { return StyxWoW.Me; } }
        private static WoWUnit currentTarget { get { return StyxWoW.Me.CurrentTarget; } }

        public static async Task<bool> UseItem(WoWItem item, bool reqs, string log = null)
        {
            if (item == null) { return false; }
            if (!reqs) { return false; }
            if (!CanUseItem(item)) { return false; }

            L.useItemLog(string.Format($"/use {item.Name}" + (String.IsNullOrEmpty(log) || !Main.IS.GeneralDebug ? "" : " - " + log)), Core.Helpers.Common.ItemColor);
            item.Use();
            await CommonCoroutines.SleepForLagDuration();
            return true;
        }
        private static bool CanUseItem(WoWItem item)
        {
            return item.Usable && item.Cooldown <= 0 && !MerchantFrame.Instance.IsVisible;
        }
        public static WoWItem FindBestBandage()
        {
            return Me.CarriedItems
                .Where(b => b.ItemInfo.ItemClass == WoWItemClass.Consumable
                    && b.ItemInfo.ConsumableClass == WoWItemConsumableClass.Bandage
                    && (b.ItemInfo.RequiredSkillId == 0 || Me.GetSkill(b.ItemInfo.RequiredSkillId).CurrentValue >= b.ItemInfo.RequiredSkillLevel)
                    && b.ItemInfo.RequiredLevel <= Me.Level
                    && CanUseItem(b))
                .OrderBy(b => b.ItemInfo.Level)
                .ThenByDescending(b => b.ItemInfo.RequiredSkillLevel)
                .FirstOrDefault();
        }
        public static WoWItem FindBestPrecombatFlask()
        {
            return Me.CarriedItems
                .Where(b => b.ItemInfo.ItemClass == WoWItemClass.Consumable
                    && b.ItemInfo.ConsumableClass == WoWItemConsumableClass.Flask
                    && (b.ItemInfo.RequiredSkillId == 0 || Me.GetSkill(b.ItemInfo.RequiredSkillId).CurrentValue >= b.ItemInfo.RequiredSkillLevel)
                    && b.ItemInfo.RequiredLevel <= Me.Level
                    && CanUseItem(b))
                .OrderBy(b => b.ItemInfo.Level)
                .ThenByDescending(b => b.ItemInfo.RequiredSkillLevel)
                .FirstOrDefault();
        }
        public static WoWItem FindBestHealingPotion()
        {
            return Me.CarriedItems
                .Where(hp => hp.ItemInfo.ItemClass == WoWItemClass.Consumable
                && hp.ItemInfo.ConsumableClass == WoWItemConsumableClass.Potion
                && (hp.ItemInfo.RequiredSkillId == 0 || Me.GetSkill(hp.ItemInfo.RequiredSkillId).CurrentValue >= hp.ItemInfo.RequiredSkillLevel)
                && hp.ItemInfo.RequiredLevel <= Me.Level
                && CanUseItem(hp)
                && Main.IS.HavocHealthPotionList.Contains(hp.ItemInfo.Id))
                .OrderBy(l => l.ItemInfo.Level)
                .ThenByDescending(hp => hp.ItemInfo.RequiredSkillLevel)
                .FirstOrDefault();
                

        }

        public static WoWItem FindBestFood()
        {
            return Me.CarriedItems
                .Where(hp => hp.ItemInfo.ItemClass == WoWItemClass.Consumable
                && hp.ItemInfo.ConsumableClass == WoWItemConsumableClass.FoodAndDrink
                && (hp.ItemInfo.RequiredSkillId == 0 || Me.GetSkill(hp.ItemInfo.RequiredSkillId).CurrentValue >= hp.ItemInfo.RequiredSkillLevel)
                && hp.ItemInfo.RequiredLevel <= Me.Level
                && CanUseItem(hp))
                .OrderBy(l => l.ItemInfo.Level)
                .ThenByDescending(hp => hp.ItemInfo.RequiredSkillLevel)
                .FirstOrDefault();


        }
        public static WoWItem GetItemByName(string itemName)
        {
            return Me.CarriedItems
                .Where(i => i.SafeName == itemName)
                .OrderBy(b => b.ItemInfo.Level)
                .ThenByDescending(i => i.ItemInfo.RequiredSkillLevel)
                .FirstOrDefault();
        }
    }
}
