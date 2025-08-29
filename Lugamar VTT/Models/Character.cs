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

        // Extended basic information
        public string? Gender { get; set; }
        public string? Age { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }
        public string? Size { get; set; }
        public string? Deity { get; set; }
        public int Experience { get; set; }
        public int ExperienceNeeded { get; set; }
        public List<CharacterClass> Classes { get; set; } = new();

        // Ability scores keyed by ability name (e.g. "strength", "dexterity")
        public Dictionary<string, AbilityScore> Abilities { get; set; } = new();

        // Combat statistics
        public int ArmorClass { get; set; }
        public int TouchArmorClass { get; set; }
        public int FlatFootedArmorClass { get; set; }

        public ArmorClassDetail ArmorClassBreakdown { get; set; } = new();
        public ArmorClassDetail TouchArmorClassBreakdown { get; set; } = new();
        public ArmorClassDetail FlatFootedArmorClassBreakdown { get; set; } = new();
        public int HitPoints { get; set; }
        public int CurrentHitPoints { get; set; }
        public int TempHitPoints { get; set; }
        public int Wounds { get; set; }
        public int NonLethalDamage { get; set; }
        public string DamageReduction { get; set; } = string.Empty;
        public int SpellResistance { get; set; }
        public string Resistances { get; set; } = string.Empty;
        public string Immunities { get; set; } = string.Empty;
        public string SpecialQualities { get; set; } = string.Empty;
        public SavingThrowDetail Fortitude { get; set; } = new();
        public SavingThrowDetail Reflex { get; set; } = new();
        public SavingThrowDetail Will { get; set; } = new();
        public int Initiative { get; set; }
        public int Speed { get; set; }
        public int BaseAttackBonus { get; set; }
        public AttackBonusDetail MeleeAttackBonus { get; set; } = new();
        public AttackBonusDetail RangedAttackBonus { get; set; } = new();
        public AttackBonusDetail CombatManeuverBonus { get; set; } = new();
        public CmdDetail CombatManeuverDefense { get; set; } = new();

        // Skill tracking
        public int SkillPointsSpent { get; set; }

        // Optional collections for skills, feats, equipment and spells
        public List<string> Skills { get; set; } = new();
        public List<SkillDetail> SkillDetails { get; set; } = new();

        public List<string> Feats { get; set; } = new();
        public List<FeatDetail> FeatDetails { get; set; } = new();

        public List<SpecialAbility> SpecialAbilities { get; set; } = new();
        public List<Trait> Traits { get; set; } = new();

        public List<string> Proficiencies { get; set; } = new();

        public List<string> Equipment { get; set; } = new();
        public List<EquipmentItem> EquipmentDetails { get; set; } = new();

        public List<string> Spells { get; set; } = new();
    }
}
