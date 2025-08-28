using System.Collections.Generic;

namespace LugamarVTT.Models
{
    /// <summary>
    /// Represents a single ability score with various modifiers and permanent adjustments.
    /// </summary>
    public class AbilityScore
    {
        public int Score { get; set; }
        public int Bonus { get; set; }
        public int Base { get; set; }
        public int Damage { get; set; }
        public int Perm { get; set; }
        public List<AbilityPerm> Perms { get; set; } = new();
    }

    /// <summary>
    /// Describes a permanent adjustment to an ability score.
    /// </summary>
    public class AbilityPerm
    {
        public int PermNum { get; set; }
        public string? BonusType { get; set; }
        public string? Name { get; set; }
    }
}
