//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class FileOfBug
    {
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
        public string FileName { get; set; }
        [NotMapped]
        [Display(Name ="Upload File")]
        public IFormFile File { get; set; }
        [ForeignKey("Bug")]
        public int BugId { get; set; }
        public Bug Bug { get; set; }
    }
}
