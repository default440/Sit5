namespace Models
{
    public class Message
    {
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public Guid ClientId { get; set; }
    }
}
