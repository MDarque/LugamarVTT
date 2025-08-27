namespace LugamarVTT.Models
{
    /// <summary>
    /// Breaks down the modifiers that contribute to an attack bonus.
    /// </summary>
    public class AttackBonusDetail
    {
        public int BaseAttackBonus { get; set; }
        public int AbilityMod { get; set; }
        public int SizeBonus { get; set; }
        public int Misc { get; set; }
        public int Temp { get; set; }
        public int Total { get; set; }
    }
}
