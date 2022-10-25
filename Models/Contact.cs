using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ContactPro_MVC.Enums;

namespace ContactPro_MVC.Models
{
    public class Contact
    {
        // primary key
        public int Id { get; set; }

        // foreign key - is passed through database as a string for hashing purposes
        [Required]
        public string? AppUserId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max of {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max of {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        [Display(Name = "Birthday")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required]
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public States State { get; set; }

        [Required]
        [Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public int ZipCode { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name ="Phone Number")]
        public string? PhoneNumber { get; set; }

        // annotated with [required] so that an input will be supplied prior to database trying to supply default date
        [Required]
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        
        // stores huge string that represents the selected image within byte array
        public byte[]? ImageData { get; set; }

        public string? ImageType { get; set; }

        // interface used only when there is a form -> file is selected and it moves image to post method so it can be used
        // chosen image will bind to properties of the IFormFile
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        // Virtual (Navigation) Properties
        // used to create relationship between databases, can "get" the full object that a foreign key points to and bind it to AppUser
        public virtual AppUser? AppUser { get; set; }

        // ICollection has the most properties/functionalities to go with generic lists
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }
}
