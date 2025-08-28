namespace LugamarVTT.Models
{
    /// <summary>
    /// Represents an individual class entry for a character.
    /// </summary>
    public class CharacterClass
    {
        public string? Name { get; set; }
        public int Level { get; set; }
        public bool Favored { get; set; }
        public int SkillRanks { get; set; }
        public int SkillRanksUsed { get; set; }
    }
}
