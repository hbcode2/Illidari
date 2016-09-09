using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illidari.Core
{
    public class SpellBlacklist
    {
        public SpellBlacklist()
        {
            SpellId = 0;
            DateBlacklisted = DateTime.MinValue;
        }
        public SpellBlacklist(int spellId)
        {
            SpellId = spellId;
            Count = 1;
            //DateBlacklisted = DateTime.Now;
        }
        
        public int SpellId { get; set; }
        public DateTime DateBlacklisted { get; set; }
        public int Count { get; set; }

        public bool IsBlacklisted()
        {
            if (DateBlacklisted == DateTime.MinValue) { return false;
            }
            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - DateBlacklisted.Ticks);
            if (ts.TotalSeconds < 5 && Count >= 3)
            {
                // we are blacklisted still
                return true;
            }
            else if (Count >= 3 && ts.TotalSeconds >= 5)
            {
                ClearBlacklist();
            }
            return false;
        }
        public void AddBlacklistCounter()
        {
            Count++;
            if (Count >= 3) { DateBlacklisted = DateTime.Now; }
        }
        public void ClearBlacklist()
        {
            Count = 0;
            DateBlacklisted = DateTime.MinValue;
        }
    }
}
