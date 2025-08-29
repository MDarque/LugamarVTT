namespace LugamarVTT.Models
{
    /// <summary>
    /// Represents the components that make up a saving throw.
    /// </summary>
    public class SavingThrowDetail
    {
        public int Base { get; set; }
        public int AbilityMod { get; set; }
        public int Misc { get; set; }
        public int Temp { get; set; }
        public int Total { get; set; }
    }
}
