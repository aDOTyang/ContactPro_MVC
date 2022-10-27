using ContactPro_MVC.Data;
using ContactPro_MVC.Models;
using ContactPro_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContactPro_MVC.Services
{
    public class AddressBookService : IAddressBookService
    {
        ApplicationDbContext _context;

        // every class has an empty constructor by default
        public AddressBookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddContactToCategoryAsync(int categoryId, int contactId)
        {
            try
            {
                // check if contact is already in category and if it is not then adds
                if (!await IsContactInCategory(categoryId, contactId))
                {
                    // if not, add category to contact's collection of categories
                    Contact? contact = await _context.Contacts.FindAsync(contactId);
                    Category? category = await _context.Categories.FindAsync(categoryId);

                    if (contact != null && category != null)
                    {
                        // adds the contact to the category list
                        category.Contacts.Add(contact);
                        // must save changes to database
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            Contact? contact = await _context.Contacts.FindAsync(contactId);

            //returns true/false whether contact already exists in category
            bool isInCategory = await _context.Categories.Include(c => c.Contacts).Where(c => c.Id == categoryId && c.Contacts.Contains(contact!)).AnyAsync();
            return isInCategory;
        }
    }
}
