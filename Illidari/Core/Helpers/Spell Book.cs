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
            BladeDance = 188499,
            Blur = 198589,
            ChaosNova = 179057,
            ChaosStrike = 162794,
            ConsumeMagic = 183752,
            Darkness = 196718,
            DemonsBite = 162243,
            DemonSpikes = 203720,
            EmpowerWards = 218256,
            EyeBeam = 198013,
            FelBlade = 213241,
            FelDevastation = 212084,
            FelEruption = 211881,
            FelRush = 195072,
            FieryBrand = 204021,
            Fracture = 209795,
            Glide = 131347,
            ImmolationAura = 178740,
            Imprison = 217832,
            InfernalStrike = 189110,
            MetamorphosisSpell = 191427,
            NetherBond = 207810,
            SigilOfChains = 202138,
            SigilOfFlame = 204596,
            SigilOfFlameTalented = 204513,
            SigilOfMisery = 207684,
            SigilOfSilence = 202137,
            Shear = 203782,
            SoulBarrier = 227225,
            SoulCarver = 207407,
            SoulCleave = 228477,
            SpiritBomb = 218679,
            ThrowGlaive = 185123,
            Torment = 185245,
            VengefulRetreat = 198793,
            End = 0;
            
        #endregion

        public static IEnumerable<int> FlaskList = new int[]
        {
            //156079,     //Greater Draenic Intellect Flask
            156064,     //Greater Draenic Agility 
            //156080,     //Greater Draenic Strength 
            //156084,     //Greater Draenic Stamina 
            156073,     //Draenic Agility Flask
            //156070,     //Draenic Intellect Flask
            //109152,     //Draenic Stamina Flask
            //156071      //Draenic Strength Flask
        };
        
    }
}