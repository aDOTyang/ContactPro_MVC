using System.ComponentModel.DataAnnotations;

namespace ContactPro_MVC.Models
{
    public class Category
    {
        // public key
        public int Id { get; set; }

        // foreign key
        [Required]
        public string? AppUserId { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string? Name { get; set; }

        // navigation properties
        public virtual AppUser? AppUser { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
    }
}