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

        public async Task<int> AddAsync(ClientRequest request, CancellationToken ct = default)
        {
            string clientCode = await _codeService.Generate(request.Name, ct);
            var entity = new Client { Name = request.Name, ClientCode = clientCode };
            _context.Clients.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task UpdateAsync(ClientRequest request, CancellationToken ct = default)
        {
            var entity = await _context.Clients.FindAsync(new object[] { request.Id }, ct);
            if (entity == null) return;

            entity.Name = request.Name;
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _context.Clients.FindAsync(new object[] { id }, ct);
            if (entity == null) return;

            _context.Clients.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<ClientResponse>> GetAllAsync(CancellationToken ct = default) =>
            await _context.Clients
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
                    .OrderBy(c => c.Surname).ThenBy(c => c.Name)
                    .ToList()
            };
        }

        public async Task LinkContactAsync(int clientId, int contactId, CancellationToken ct = default)
        {
            bool alreadyLinked = await _context.ClientContacts
                .AnyAsync(cc => cc.ClientId == clientId && cc.ContactId == contactId, ct);

            if (alreadyLinked) return;

            _context.ClientContacts.Add(new ClientContact { ClientId = clientId, ContactId = contactId });
            await _context.SaveChangesAsync(ct);
        }

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
