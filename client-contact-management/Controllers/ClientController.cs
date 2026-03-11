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

        public ClientController(IClientService clientService) => _clientService = clientService;
        // GET: Clients
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var clients = await _clientService.GetAllAsync(ct);
            return View(clients);
        }

        // GET: Clients/Create
        public IActionResult Create() => View(new ClientRequest());

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientRequest client, CancellationToken ct)
        {
            if (ModelState.IsValid)
            {
                await _clientService.AddAsync(client, ct);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var client = await _clientService.GetByIdAsync(id, ct);
            if (client == null) return NotFound();

            var request = new ClientRequest
            {
                Id = client.Id,
                Name = client.Name
            };

            return View(request);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientRequest client, CancellationToken ct)
        {
            if (ModelState.IsValid)
            {
                await _clientService.UpdateAsync(client, ct);
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // POST: Clients/LinkContact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkContact(int clientId, int contactId, CancellationToken ct)
        {
            await _clientService.LinkContactAsync(clientId, contactId, ct);
            return RedirectToAction(nameof(Edit), new { id = clientId });
        }

        // GET: Clients/UnlinkContact
        public async Task<IActionResult> UnlinkContact(int clientId, int contactId, CancellationToken ct)
        {
            await _clientService.UnlinkContactAsync(clientId, contactId, ct);
            return RedirectToAction(nameof(Edit), new { id = clientId });
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var client = await _clientService.GetByIdAsync(id, ct);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            await _clientService.DeleteAsync(id, ct);
            return RedirectToAction(nameof(Index));
        }
    }
}
