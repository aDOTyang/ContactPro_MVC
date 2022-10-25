using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactPro_MVC.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max of {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max of {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        // this property does not need to be mapped as we already have this user input info, will create duplicate info in database otherwise
        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }


        // creates relationship between Contact Model and Contacts Table
        // can have many contacts per user via ICollection and initialize to HashSet which is also a list
        // when contact is created, it pulls the user's ID and attaches it to the contact
        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();

        // creates relationship to Category Model
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }
}
