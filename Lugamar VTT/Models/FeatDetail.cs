namespace LugamarVTT.Models
{
    /// <summary>
    /// Detailed information about a feat including prerequisite text and
    /// formatted benefit descriptions.
    /// </summary>
    public class FeatDetail
    {
        public string Name { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Prerequisites { get; set; } = string.Empty;
        public string Benefit { get; set; } = string.Empty;
        public string Normal { get; set; } = string.Empty;
        public string Special { get; set; } = string.Empty;
    }
}
