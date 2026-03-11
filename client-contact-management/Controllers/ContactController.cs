using client_contact_management.Models;
using client_contact_management.Services;
using Microsoft.AspNetCore.Mvc;

namespace client_contact_management.Controllers
{
    public class ContactController : Controller
    {
        private readonly IContactService _contactService;
        private readonly IClientService _clientService;

        public ContactController(IContactService contactService, IClientService clientService)
        {
            _contactService = contactService;
            _clientService = clientService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var contacts = await _contactService.GetAllAsync(ct);
            return View(contacts);
        }

        public IActionResult Create() => View(new ContactRequest());
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactRequest contact, CancellationToken ct)
        {
            if (!await _contactService.IsEmailUniqueAsync(contact.Email, ct: ct))
                ModelState.AddModelError(nameof(contact.Email), "This email address is already in use.");

            if (ModelState.IsValid)
            {
                int newId = await _contactService.AddAsync(contact, ct);
                return RedirectToAction(nameof(Edit), new { id = newId });
            }

            return View(contact);
        }

        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var response = await _contactService.GetByIdAsync(id, ct);
            if (response == null) return NotFound();

            var request = new ContactRequest
            {
                Id = response.Id,
                Name = response.Name,
                Surname = response.Surname,
                Email = response.Email
            };

            await PopulateEditViewBag(response.LinkedClients, ct);

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactRequest contact, CancellationToken ct)
        {
            if (!await _contactService.IsEmailUniqueAsync(contact.Email, contact.Id, ct))
                ModelState.AddModelError(nameof(contact.Email), "This email address is already in use.");

            if (ModelState.IsValid)
            {
                await _contactService.UpdateAsync(contact, ct);
                return RedirectToAction(nameof(Edit), new { id = contact.Id });
            }
            var response = await _contactService.GetByIdAsync(contact.Id, ct);
            await PopulateEditViewBag(response?.LinkedClients, ct);

            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkClient(int contactId, int clientId, CancellationToken ct)
        {
            await _contactService.LinkClientAsync(contactId, clientId, ct);
            return RedirectToAction(nameof(Edit), new { id = contactId, tab = "clients" });
        }

        public async Task<IActionResult> UnlinkClient(int contactId, int clientId, CancellationToken ct)
        {
            await _contactService.UnlinkClientAsync(contactId, clientId, ct);
            return RedirectToAction(nameof(Edit), new { id = contactId, tab = "clients" });
        }

        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var contact = await _contactService.GetByIdAsync(id, ct);
            if (contact == null) return NotFound();
            return View(contact);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            await _contactService.DeleteAsync(id, ct);
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateEditViewBag(
            List<ClientResponse>? linkedClients,
            CancellationToken ct)
        {
            ViewBag.LinkedClients = linkedClients ?? new List<ClientResponse>();
            ViewBag.AllClients = await _clientService.GetAllAsync(ct);
        }
    }
}
