using Microsoft.AspNetCore.Components;

namespace LugamarVTT.Models
{
    /// <summary>
    /// Represents a character trait drawn from the XML including its source
    /// classification and descriptive text.
    /// </summary>
    public class Trait
    {
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        /// <summary>Formatted description of the trait.</summary>
        public MarkupString Text { get; set; } = new MarkupString();
    }
}
