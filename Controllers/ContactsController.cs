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
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ContactPro_MVC.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        // UserManager needs to be handed our version of itself, otherwise will use IdentityUser
        private readonly UserManager<AppUser> _userManager;

        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailSender;

        public ContactsController(ApplicationDbContext context, UserManager<AppUser> userManager, IImageService imageService, IAddressBookService addressBookService, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
            _emailSender = emailSender;
        }

        // GET: Contacts
        // User only exists when the user is logged in (authorized)
        [Authorize]
        public async Task<IActionResult> Index(int? categoryId, string? swalMessage = null)
        {
            string userId = _userManager.GetUserId(User);
            ViewData["SwalMessage"] = swalMessage;
            // c is just a chosen variable to make it easy to write the lambda expression, based on first letter 'c'ontacts
            // this expression quickly searches the database and returns the properties based on contacts class & the user's AppUserId (foreign key), includes AppUser (virtual property created from AppUserId) in list, and organizes the list & returns when requested
            // Include method allows us to later access AppUser directly from within contacts
            List<Contact> contacts = new List<Contact>();

            List<Category> userCategories = await _context.Categories.Where(c => c.AppUserId == userId).ToListAsync();

            if (categoryId == null)
            {
                contacts = await _context.Contacts.Where(c => c.AppUserId == userId).Include(c => c.AppUser).Include(c => c.Categories).ToListAsync();
            }
            else
            {
                contacts = (await _context.Categories.Include(c => c.Contacts).FirstOrDefaultAsync(c => c.AppUserId == userId && c.Id == categoryId))!.Contacts.ToList();
            }

            ViewData["CategoryId"] = new SelectList(userCategories, "Id", "Name", categoryId);
            return View(contacts);
        }

        // POST: SearchContacts
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchContacts(string? searchString)
        {
            string userId = _userManager.GetUserId(User);
            List<Contact> contacts = new List<Contact>();
            List<Category> userCategories = await _context.Categories.Where(c => c.AppUserId == userId).ToListAsync();

            AppUser? appUser = await _context.Users.Include(c => c.Contacts).ThenInclude(c => c.Categories).FirstOrDefaultAsync(u => u.Id == userId);

            // if no match, returns alphabetized contact list
            // else, returns contact list filtered by search string x full name
            if (string.IsNullOrEmpty(searchString))
            {
                contacts = appUser!.Contacts.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();
            }
            else
            {
                contacts = appUser!.Contacts.Where(c => c.FullName!.ToLower().Contains(searchString.ToLower()))
                                   .OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();
            }
            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name");
            // redirects to the contacts action
            return View(nameof(Index), contacts);
        }

        // HttpGet is by default, but labeled as a nicety
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EmailContact(int? id)
        {
            string appUserId = _userManager.GetUserId(User);
            Contact? contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);

            if (contact == null)
            {
                return NotFound();
            }

            // instantiate & set properties directly
            EmailData emailData = new EmailData()
            {
                EmailAddress = contact.Email,
                FirstName = contact.FirstName,
                LastName = contact.LastName
            };

            EmailContactViewModel viewmodel = new EmailContactViewModel()
            {
                Contact = contact,
                EmailData = emailData
            };

            return View(viewmodel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailContact(EmailContactViewModel viewmodel)
        {
            if (ModelState.IsValid)
            {
                string swalMessage = string.Empty;

                try
                {
                    await _emailSender.SendEmailAsync(viewmodel.EmailData!.EmailAddress, viewmodel.EmailData!.EmailSubject, viewmodel.EmailData!.EmailBody);
                    swalMessage = "Success: Email sent!";
                    return RedirectToAction("Index", "Contacts", new { swalMessage });
                }
                catch (Exception)
                {
                    swalMessage = "Error: Failed to send Email.";
                    return RedirectToAction("Index", "Contacts", new { swalMessage });
                    throw;
                }
            }
            return View(viewmodel);
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

            // establish who the user is and get their userId
            string appUserId = _userManager.GetUserId(User);

            // FirstOrDefault will return empty instance (rather than null) as the default value if contact not found
            Contact? contact = await _context.Contacts.Include(c => c.Categories).FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);

            if (contact == null)
            {
                return NotFound();
            }

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());

            List<Category> categories = (await _addressBookService.GetAppUserCategoriesAsync(appUserId)).ToList();
            // get list of IDs which are integers from database based on userId and instantiate
            List<int> categoryIds = contact.Categories.Select(c => c.Id).ToList();

            // constructor to create categories dropdown list; MultiSelectList allows for multiple selections in combination with 'multiple' property in the create form 
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", categoryIds);

            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageFile,ImageData,ImageType")] Contact contact, List<int> categoryList)
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

                    // removes current categories from contact
                    await _addressBookService.RemoveAllContactCategoriesAsync(contact.Id);

                    // add selected categories to contact
                    await _addressBookService.AddContactToCategoriesAsync(categoryList, contact.Id);
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

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            List<Category> categories = (await _addressBookService.GetAppUserCategoriesAsync(contact.AppUserId!)).ToList();
            List<int> categoryIds = contact.Categories.Select(c => c.Id).ToList();
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", categoryIds);

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
