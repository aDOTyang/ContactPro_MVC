﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactPro_MVC.Data;
using ContactPro_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using ContactPro_MVC.Services;
using Microsoft.AspNetCore.Identity;
using ContactPro_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ContactPro_MVC.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IEmailSender _emailSender;

        public CategoriesController(ApplicationDbContext context, UserManager<AppUser> userManager, IImageService imageService, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _emailSender = emailSender;
        }

        // GET: Categories
        [Authorize]
        public async Task<IActionResult> Index(string? swalMessage = null)
        {
            // passes sweetalert data from EmailCategory post method into the index get method
            ViewData["SwalMessage"] = swalMessage;
            
            string userId = _userManager.GetUserId(User);

            List<Category> categories = await _context.Categories.Where(c => c.AppUserId == userId).Include(c => c.AppUser).ToListAsync();
            return View(categories);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EmailCategory(int? id)
        {
            // find category based on incoming id and ensure it also checks for current user
            string appUserId = _userManager.GetUserId(User);
            Category? category = await _context.Categories.Include(c => c.Contacts).FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);

            if (category == null)
            {
                return NotFound();
            }

            List<string> emails = category!.Contacts.Select(c => c.Email).ToList()!;

            EmailData emailData = new EmailData()
            {
                GroupName = category.Name,
                EmailAddress = string.Join("; ", emails),
                EmailSubject = $"Group Message: {category.Name}"
            };

            return View(emailData);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailCategory(EmailData emailData)
        {
            if (ModelState.IsValid)
            {
                string swalMessage = string.Empty;

                try
                {
                    await _emailSender.SendEmailAsync(emailData!.EmailAddress, emailData!.EmailSubject, emailData!.EmailBody);
                    swalMessage = "Success: Email sent!";
                    return RedirectToAction("Index", "Categories", new { swalMessage });
                }
                catch (Exception)
                {
                    swalMessage = "Error: Failed to send Email.";
                    return RedirectToAction("Index", "Categories", new { swalMessage });
                    throw;
                }
            }
            return View(emailData);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User);
                category.AppUserId = userId;

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", category.AppUserId);
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,Name")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

    }
}
