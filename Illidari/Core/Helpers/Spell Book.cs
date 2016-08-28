using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illidari.Core.Helpers
{
    class Spell_Book
    {
        public const int
        #region Aura List    
            AuraMetemorphisis = 111,
            AuraDemonSpikes = 203819,
        #endregion

        #region Spell List        
            DemonsBite = 162243,
            ChaosStrike = 162794,
            EyeBeam = 198013,
            BladeDance = 188499,
            ThrowGlaive = 185123,
            VengefulRetreat = 198793,
            FelRush = 195072,
            Blur = 198589,
            ChaosNova = 179057,
            ConsumeMagic = 183752,
            Darkness = 196718,
            MetamorphosisSpell = 191427,
            InfernalStrike = 189110,
            Glide = 131347,
            Imprison = 217832,
            EmpowerWards = 218256,
            Shear = 203782,
            DemonSpikes = 203720,
            Torment = 185245,
            SoulCleave = 228477,
            FieryBrand = 204021,
            ImmolationAura = 178740,
            SigilOfFlame = 204596,
            End = 0;
            
        #endregion

        public static IEnumerable<int> FlaskList = new int[]
        {
            156079,     //Greater Draenic Intellect Flask
            156064,     //Greater Draenic Agility 
            156080,     //Greater Draenic Strength 
            156084,     //Greater Draenic Stamina 
            156073,     //Draenic Agility Flask
            156070,     //Draenic Intellect Flask
            109152,     //Draenic Stamina Flask
            156071      //Draenic Strength Flask
        };
        
    }
}