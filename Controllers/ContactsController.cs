using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactPro_MVC.Data;
using ContactPro_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ContactPro_MVC.Enums;
using ContactPro_MVC.Services.Interfaces;
using ContactPro_MVC.Services;

namespace ContactPro_MVC.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        // UserManager needs to be handed our version of itself, otherwise will use IdentityUser
        private readonly UserManager<AppUser> _userManager;

        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;

        public ContactsController(ApplicationDbContext context, UserManager<AppUser> userManager, IImageService imageService, IAddressBookService addressBookService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
        }

        // GET: Contacts
        // User only exists when the user is logged in (authorized)
        [Authorize]
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);

            // c is just a chosen variable to make it easy to write the lambda expression, based on first letter 'c'ontacts
            // this expression quickly searches the database and returns the properties based on contacts class & the user's AppUserId (foreign key), includes AppUser (virtual property created from AppUserId) in list, and organizes the list & returns when requested
            // Include method allows us to later access AppUser directly from within contacts
            List<Contact> contacts = await _context.Contacts.Where(c => c.AppUserId == userId).Include(c => c.AppUser).Include(c => c.Categories).ToListAsync();
            return View(contacts);
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        [Authorize]
        // Task<>
        public async Task<IActionResult> Create()
        {
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());

            // establish who the user is and get their userId
            string userId = _userManager.GetUserId(User);
            // get list of categories from database based on userId and instantiate
            List<Category> categories = await _context.Categories.Where(c => c.AppUserId == userId).ToListAsync();
            // constructor to create categories dropdown list; MultiSelectList allows for multiple selections in combination with 'multiple' property in the create form 
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name");

            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber, ImageFile")] Contact contact, List<int> categoryList)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                contact.AppUserId = _userManager.GetUserId(User);
                contact.Created = DateTime.UtcNow;

                if (contact.BirthDate != null)
                {
                    // formats date for PostGreSQL
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                }

                // checks whether file/image has been selected
                // if ImageFile is NOT null (i.e. selected), set ImageData property -> converts file to byte[]
                // if ImageFile is NOT null (i.e. selected), set ImageType property -> use file extension as value
                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();

                // handles categories list
                // TODO - use the list of category IDs to 1) find associated category and 2) add category to collection of categories for current Contact
                foreach (int categoryId in categoryList)
                {
                    await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                }

                return RedirectToAction(nameof(Index));
            }

            // if model state is NOT valid: skips if block, resets the states list, re-gets userid and categories before returning
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());

            string userId = _userManager.GetUserId(User);
            // get list of categories from database based on userId and instantiate
            List<Category> categories = await _context.Categories.Where(c => c.AppUserId == userId).ToListAsync();
            // constructor to create categories dropdown list; MultiSelectList allows for multiple selections in combination with 'multiple' property in the create form 
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name");

            return View(contact);
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            Contact? contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageFile,ImageData,ImageType")] Contact contact)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc);

                    if (contact.BirthDate != null)
                    {
                        // formats date for PostGreSQL
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }

                    // checks whether file/image has been selected
                    // if ImageFile is NOT null (i.e. selected), set ImageData property -> converts file to byte[]
                    // if ImageFile is NOT null (i.e. selected), set ImageType property -> use file extension as value
                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }

                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    // TODO add categories

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserId);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contacts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contacts'  is null.");
            }
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
