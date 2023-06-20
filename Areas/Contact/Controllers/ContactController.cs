using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Data;
using ContactModel = App.Models.Contacts.Contact;
using Microsoft.AspNetCore.Authorization;

namespace App.Areas.Contact.Controllers
{
    [Area("Contact")]
    [Authorize(Roles = RoleName.Administrator)]
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        // GET: Contact
        [HttpGet("/admin/contact")]
        public async Task<IActionResult> Index()
        {
              return View(await _context.Contacts.ToListAsync());
        }

        // GET: Contact/Details/5
        [HttpGet("/admin/contact/details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contact/Create
        [HttpGet("/contact")]
        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Contact/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/contact")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Email,Message,Phone")] ContactModel contact)
        {
            if (ModelState.IsValid)
            {
                contact.DateSent = DateTime.Now.Date;
                _context.Add(contact);
                await _context.SaveChangesAsync();
                StatusMessage = "Your contact has been sent";
                return RedirectToAction("Index", "Home");
            }
            return View(contact);
        }

        // GET: Contact/Delete/5
        [HttpGet("/admin/contact/delete/{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contact/Delete/5
        [HttpPost("/admin/contact/delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contacts == null)
            {
                return Problem("Entity set 'AppDbContext.Contacts'  is null.");
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
