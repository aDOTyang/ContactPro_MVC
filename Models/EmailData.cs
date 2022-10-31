using System.ComponentModel.DataAnnotations;

namespace ContactPro_MVC.Models
{
    public class EmailData
    {
        public int? MyProperty { get; set; }

        [Required]
        public string? EmailAddress { get; set; }
        [Required]
        public string? EmailSubject { get; set; }
        [Required]
        public string? EmailBody { get; set; }
        
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? GroupName { get; set; }
    }
}
