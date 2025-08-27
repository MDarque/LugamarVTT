using System.Collections.Generic;

namespace LugamarVTT.Models
{
    /// <summary>
    /// Represents a player character in a Pathfinder 1e game.  This model is
    /// designed to capture commonly recorded fields from a Pathfinder character
    /// sheet, such as basic identity, ability scores, combat statistics and
    /// other optional details like skills and feats.
    /// </summary>
    public class Character
    {
        /// <summary>
        /// Identifier for the character within the XML database.  This is an
        /// internal index assigned by <see cref="XmlDataService"/> when the
        /// characters are parsed.  It should be stable for a given file
        /// read, but may change if the underlying XML changes or new
        /// characters are inserted before existing ones.
        /// </summary>
        public int Id { get; set; }
        // Basic information
        public string? Name { get; set; }
        public string? Race { get; set; }
        public string? Class { get; set; }
        public string? Alignment { get; set; }
        public int Level { get; set; }

        // Ability scores
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }

        // Combat statistics
        public int ArmorClass { get; set; }
        public int HitPoints { get; set; }
        public string? BaseAttackBonus { get; set; }

        // Optional collections for skills, feats, equipment and spells
        public List<string> Skills { get; set; } = new();
        public List<string> Feats { get; set; } = new();
        public List<string> Equipment { get; set; } = new();
        public List<string> Spells { get; set; } = new();
    }
}
