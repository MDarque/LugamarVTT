using System;

namespace LugamarVTT.Models
{
    /// <summary>
    /// Represents the components that make up an armor class calculation.
    /// </summary>
    public class ArmorClassDetail
    {
        public int DexModifier { get; set; }
        public int SizeModifier { get; set; }
        public int ArmorBonus { get; set; }
        public int ShieldBonus { get; set; }
        public int NaturalArmor { get; set; }
        public int Dodge { get; set; }
        public int Misc { get; set; }
        public int Deflection { get; set; }
        public int Temp { get; set; }
        public int Total { get; set; }
    }
}
