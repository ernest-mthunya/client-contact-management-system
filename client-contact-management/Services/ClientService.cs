using client_contact_management.Data;
using client_contact_management.Entities;
using client_contact_management.Models;
using Microsoft.EntityFrameworkCore;

namespace client_contact_management.Services
{
    public class ClientService : IClientService
    {
        private readonly ClientContactManagementDbContext _context;
        private readonly IClientCodeService _codeService;


        public ClientService(ClientContactManagementDbContext context, IClientCodeService codeService)
        {
            _context = context;
            _codeService = codeService;
        }

        // ─────────────────────────────────────────
        // Add — generates ClientCode, returns new Id
        // ─────────────────────────────────────────
        public async Task<int> AddAsync(ClientRequest client, CancellationToken ct = default)
        {
            string clientCode = await _codeService.Generate(client.Name, ct);

            var entity = new Entities.Client
            {
                Name = client.Name,
                ClientCode = clientCode
            };

            _context.Clients.Add(entity);
            await _context.SaveChangesAsync(ct);

            return entity.Id;
        }

        // ─────────────────────────────────────────
        // Update — only updates Name, never ClientCode
        // ─────────────────────────────────────────
        public async Task UpdateAsync(ClientRequest client, CancellationToken ct = default)
        {
            var existing = await _context.Clients.FindAsync(new object[] { client.Id }, ct);
            if (existing == null) return;

            existing.Name = client.Name;
            // ClientCode is intentionally never changed after creation

            await _context.SaveChangesAsync(ct);
        }

        // ─────────────────────────────────────────
        // Delete
        // ─────────────────────────────────────────
        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var client = await _context.Clients.FindAsync(new object[] { id }, ct);
            if (client == null) return;

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync(ct);
        }

        // ─────────────────────────────────────────
        // Get all — for Index view
        // ─────────────────────────────────────────
        public async Task<IEnumerable<ClientResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Clients
                .Include(c => c.ClientContacts)
                .OrderBy(c => c.Name)
                .Select(c => new ClientResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    ClientCode = c.ClientCode ?? string.Empty,
                    NumberOfContactLinked = c.ClientContacts.Count
                })
                .ToListAsync(ct);
        }

        // ─────────────────────────────────────────
        // Get by Id — for Edit view (includes linked contacts)
        // ─────────────────────────────────────────
        public async Task<ClientResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var client = await _context.Clients
                .Include(c => c.ClientContacts)
                    .ThenInclude(cc => cc.Contact)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (client == null) return null;

            return new ClientResponse
            {
                Id = client.Id,
                Name = client.Name,
                ClientCode = client.ClientCode ?? string.Empty,
                NumberOfContactLinked = client.ClientContacts.Count,
                LinkedContacts = client.ClientContacts
                    .Select(cc => new ContactResponse
                    {
                        Id = cc.Contact.Id,
                        Name = cc.Contact.Name,
                        Surname = cc.Contact.Surname,
                        Email = cc.Contact.Email
                    })
                    .OrderBy(c => c.Surname)
                    .ThenBy(c => c.Name)
                    .ToList()
            };
        }

        // ─────────────────────────────────────────
        // Link contact to client (skips if already linked)
        // ─────────────────────────────────────────
        public async Task LinkContactAsync(int clientId, int contactId, CancellationToken ct = default)
        {
            bool alreadyLinked = await _context.ClientContacts
                .AnyAsync(cc => cc.ClientId == clientId && cc.ContactId == contactId, ct);

            if (alreadyLinked) return;

            _context.ClientContacts.Add(new ClientContact
            {
                ClientId = clientId,
                ContactId = contactId
            });

            await _context.SaveChangesAsync(ct);
        }

        // ─────────────────────────────────────────
        // Unlink contact from client
        // ─────────────────────────────────────────
        public async Task UnlinkContactAsync(int clientId, int contactId, CancellationToken ct = default)
        {
            var link = await _context.ClientContacts
                .FirstOrDefaultAsync(cc => cc.ClientId == clientId && cc.ContactId == contactId, ct);

            if (link == null) return;

            _context.ClientContacts.Remove(link);
            await _context.SaveChangesAsync(ct);
        }
    }
}
