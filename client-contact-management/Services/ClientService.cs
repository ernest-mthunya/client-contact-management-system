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
        public async Task AddAsync(ClientRequest client, CancellationToken ct = default)
        {
             string clientCode = await _codeService.Generate(client.Name, ct);  
            _context.Clients.Add(new Entities.Client { Name = client.Name, ClientCode = clientCode });
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var client = await _context.Clients.FindAsync(new object[] { id }, ct);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<IEnumerable<ClientResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Clients
                .Include(c => c.ClientContacts) // Ensure the collection is loaded
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

        public async Task<ClientResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var client = await _context.Clients
                        .Include(c => c.ClientContacts)
                        .ThenInclude(cc => cc.Contact)
                        .FirstOrDefaultAsync(c => c.Id == id, ct);

            return new ClientResponse 
            { 
                ClientCode = client!.ClientCode!,
                Name = client.Name,
                Id = client.Id, 
                NumberOfContactLinked = client.ClientContacts.Count 
            };
        }

        public async Task LinkContactAsync(int clientId, int contactId, CancellationToken ct = default)
        {
            if (!await _context.ClientContacts.AnyAsync(cc => cc.ClientId == clientId && cc.ContactId == contactId, ct))
            {
                _context.ClientContacts.Add(new ClientContact { ClientId = clientId, ContactId = contactId });
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task UnlinkContactAsync(int clientId, int contactId, CancellationToken ct = default)
        {
            var link = await _context.ClientContacts.FirstOrDefaultAsync(cc => cc.ClientId == clientId && cc.ContactId == contactId, ct);
            if (link != null)
            {
                _context.ClientContacts.Remove(link);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task UpdateAsync(ClientRequest client, CancellationToken ct = default)
        {
            var existing = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == client.Id, ct);
            if (existing != null)
            {
                string clientCode = await _codeService.Generate(client.Name, ct);
                existing.ClientCode = clientCode; 
                existing.Name = client.Name;

                _context.Clients.Update(existing);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
