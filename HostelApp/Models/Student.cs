using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HostelApp.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StudentRollNo { get; set; }
        public string CNIC { get; set; }
        public string Program { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public int? RoomId { get; set; }
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }
    }
}