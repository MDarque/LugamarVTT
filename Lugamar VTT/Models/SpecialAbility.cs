namespace LugamarVTT.Models
{
    /// <summary>
    /// Represents a special ability a character possesses, including the
    /// source of the ability and its descriptive text.
    /// </summary>
    public class SpecialAbility
    {
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        /// <summary>Formatted description of the ability.</summary>
        public string Text { get; set; } = string.Empty;
    }
}
