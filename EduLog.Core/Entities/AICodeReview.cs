namespace EduLog.Core.Entities
{
    public class AICodeReview
    {
        public int SuggestedScore { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Strengths { get; set; } = string.Empty;
        public string Improvements { get; set; } = string.Empty;
        public string MissingParts { get; set; } = string.Empty;
    }
}
