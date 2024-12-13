namespace CloudComputing.Models
{
    public class RedactionResponse
    {
        public required string Hash { get; set; }
        public required List<Link> Links { get; set; }
    }

    public class Link
    {
        public required string Rel { get; set; }
        public required string Href { get; set; }
    }
}
