namespace LugamarVTT.Models
{
    /// <summary>
    /// Represents the components that make up a combat maneuver defense value.
    /// </summary>
    public class CmdDetail
    {
        public int BaseAttackBonus { get; set; }
        public int StrBonus { get; set; }
        public int DexBonus { get; set; }
        public int SizeBonus { get; set; }
        public int Misc { get; set; }
        public int Total { get; set; }
    }
}
