using System.Collections.Generic;

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

        /// <summary>
        /// Raw details parsed from the XML node. Keys are the original element
        /// names (e.g. "type", "weight") and values are the formatted string
        /// representations.  This is used to dynamically render a modal with
        /// item details on the character sheet.
        /// </summary>
        public Dictionary<string, string> Details { get; set; } = new();
    }
}
