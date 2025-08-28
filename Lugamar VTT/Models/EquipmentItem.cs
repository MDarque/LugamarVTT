namespace LugamarVTT.Models
{
    /// <summary>
    /// Represents an inventory item including its categorisation and
    /// descriptive fields used for the character sheet.
    /// </summary>
    public class EquipmentItem
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Subtype { get; set; } = string.Empty;
        public string Cost { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        /// <summary>Formatted description of the item.</summary>
        public string Description { get; set; } = string.Empty;
    }
}
