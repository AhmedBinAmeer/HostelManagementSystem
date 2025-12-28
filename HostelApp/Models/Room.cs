using System.ComponentModel.DataAnnotations;

namespace HostelApp.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } // e.g. "F1-01"
        public string Floor { get; set; }      // e.g. "1st Floor"
        public int Capacity { get; set; } = 3;
        public virtual ICollection<Student> Students { get; set; }
    }
}