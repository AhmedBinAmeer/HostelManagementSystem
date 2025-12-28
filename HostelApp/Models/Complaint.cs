namespace HostelApp.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime Date { get; set; } = DateTime.Now;
    }
}