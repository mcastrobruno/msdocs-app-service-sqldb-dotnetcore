using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    public class Student
    {
        public int ID { get; set; }
        public required string Name { get; set; }

        [DisplayName("Google Meet URL")]
        public string? GoogleMeetUrl { get; set; }
    }
}
