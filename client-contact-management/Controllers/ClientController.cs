using Azure;
using client_contact_management.Entities;
using client_contact_management.Models;
using client_contact_management.Services;
using Microsoft.AspNetCore.Mvc;

namespace client_contact_management.Controllers
{
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;
        private readonly IContactService _contactService;
        public ClientController(
            IClientService clientService,
            IContactService contactService)
        {
            _clientService = clientService;
            _contactService = contactService;
        }
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            return View(); // no model needed — data loaded via AJAX
        }

        [HttpGet]
        public async Task<IActionResult> GetClients(CancellationToken ct)
        {
            var clients = await _clientService.GetAllAsync(ct);
            return Json(clients);
        }

        public IActionResult Create() => View(new ClientRequest());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientRequest client, CancellationToken ct)
        {
            if (ModelState.IsValid)
            {
                int newId = await _clientService.AddAsync(client, ct);
                return RedirectToAction(nameof(Edit), new { id = newId });
            }
            return View(client);
        }
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var response = await _clientService.GetByIdAsync(id, ct);
            if (response == null) return NotFound();

            var request = new ClientRequest
            {
                Id = response.Id,
                Name = response.Name,
                ClientCode = response.ClientCode
            };

            await PopulateEditViewBag(id, response.LinkedContacts, ct);

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientRequest client, CancellationToken ct)
        {
            if (ModelState.IsValid)
            {
                await _clientService.UpdateAsync(client, ct);
                return RedirectToAction(nameof(Edit), new { id = client.Id });
            }
            var response = await _clientService.GetByIdAsync(client.Id, ct);
            await PopulateEditViewBag(client.Id, response?.LinkedContacts, ct);

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkContact(int clientId, int contactId, CancellationToken ct)
        {
            await _clientService.LinkContactAsync(clientId, contactId, ct);
            return RedirectToAction(nameof(Edit), new { id = clientId, tab = "contacts" });
        }

        public async Task<IActionResult> UnlinkContact(int clientId, int contactId, CancellationToken ct)
        {
            await _clientService.UnlinkContactAsync(clientId, contactId, ct);
            return RedirectToAction(nameof(Edit), new { id = clientId, tab = "contacts" });
        }

        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = await _clientService.GetByIdAsync(id, ct);
            if (client == null) return NotFound();
            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            await _clientService.DeleteAsync(id, ct);

            // AJAX request — return 200 so the view can remove the row
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Ok();

            // Normal POST — redirect to Index
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateEditViewBag(
            int clientId,
            List<ContactResponse>? linkedContacts,
            CancellationToken ct)
        {
            ViewBag.LinkedContacts = linkedContacts ?? new List<ContactResponse>();
            ViewBag.AllContacts = await _contactService.GetAllAsync(ct);
        }

    }
}
