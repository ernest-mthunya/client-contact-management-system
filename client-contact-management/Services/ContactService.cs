using client_contact_management.Data;
using client_contact_management.Entities;
using client_contact_management.Models;
using Microsoft.EntityFrameworkCore;

namespace client_contact_management.Services
{
    public class ContactService : IContactService
    {
        private readonly ClientContactManagementDbContext _context;

        public ContactService(ClientContactManagementDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(ContactRequest request, CancellationToken ct = default)
        {
            var entity = new Contact
            {
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email
            };

            _context.Contacts.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task UpdateAsync(ContactRequest request, CancellationToken ct = default)
        {
            var entity = await _context.Contacts.FindAsync(new object[] { request.Id }, ct);
            if (entity == null) return;

            entity.Name = request.Name;
            entity.Surname = request.Surname;
            entity.Email = request.Email;
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _context.Contacts.FindAsync(new object[] { id }, ct);
            if (entity == null) return;

            _context.Contacts.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<ContactResponse>> GetAllAsync(CancellationToken ct = default) =>
            await _context.Contacts
                .Include(c => c.ClientContacts)
                .OrderBy(c => c.Surname).ThenBy(c => c.Name)
                .Select(c => new ContactResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Surname = c.Surname,
                    Email = c.Email,
                    NumberOfClientsLinked = c.ClientContacts.Count
                })
                .ToListAsync(ct);

        public async Task<ContactResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var contact = await _context.Contacts
                .Include(c => c.ClientContacts)
                    .ThenInclude(cc => cc.Client)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (contact == null) return null;

            return new ContactResponse
            {
                Id = contact.Id,
                Name = contact.Name,
                Surname = contact.Surname,
                Email = contact.Email,
                NumberOfClientsLinked = contact.ClientContacts.Count,
                LinkedClients = contact.ClientContacts
                    .Select(cc => new ClientResponse
                    {
                        Id = cc.Client.Id,
                        Name = cc.Client.Name,
                        ClientCode = cc.Client.ClientCode ?? string.Empty
                    })
                    .OrderBy(c => c.Name)
                    .ToList()
            };
        }

        public async Task LinkClientAsync(int contactId, int clientId, CancellationToken ct = default)
        {
            bool alreadyLinked = await _context.ClientContacts
                .AnyAsync(cc => cc.ContactId == contactId && cc.ClientId == clientId, ct);

            if (alreadyLinked) return;

            _context.ClientContacts.Add(new ClientContact { ContactId = contactId, ClientId = clientId });
            await _context.SaveChangesAsync(ct);
        }

        public async Task UnlinkClientAsync(int contactId, int clientId, CancellationToken ct = default)
        {
            var link = await _context.ClientContacts
                .FirstOrDefaultAsync(cc => cc.ContactId == contactId && cc.ClientId == clientId, ct);

            if (link == null) return;

            _context.ClientContacts.Remove(link);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken ct = default) =>
            !await _context.Contacts
                .AnyAsync(c => c.Email == email && (excludeId == null || c.Id != excludeId), ct);
    }
}
