namespace LugamarVTT.Models
{
    /// <summary>
    /// Represents a single skill entry including its ranks, miscellaneous
    /// modifier and final total bonus.
    /// </summary>
    public class SkillDetail
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>Abbreviated ability that powers this skill (e.g. STR, DEX).</summary>
        public string Ability { get; set; } = string.Empty;

        /// <summary>Modifier derived from the associated ability score.</summary>
        public int AbilityBonus { get; set; }

        /// <summary>Number of skill ranks invested by the character.</summary>
        public int Ranks { get; set; }

        /// <summary>Miscellaneous modifier applied to the skill.</summary>
        public int Misc { get; set; }

        /// <summary>Total bonus after applying ability modifier, ranks and misc.</summary>
        public int Total { get; set; }
    }
}
