using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ContactPro_MVC.Models;

namespace ContactPro_MVC.Data
{
    // override the identity user IdentityDbContext with our AppUser class which inherits IdentityUser class properties
    // this is an example of Extension
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}