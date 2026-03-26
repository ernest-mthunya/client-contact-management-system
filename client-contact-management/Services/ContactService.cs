using client_contact_management.Data;
using client_contact_management.Entities;
using client_contact_management.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace client_contact_management.Services
{
    public class ContactService : IContactService
    {
        private readonly ClientContactManagementDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ContactService> _logger;

        private const string AllContactsCacheKey = "contacts_all";
        private static string ContactCacheKey(int id) => $"contact_{id}";

        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        public ContactService(
            ClientContactManagementDbContext context,
            IDistributedCache cache,
            ILogger<ContactService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
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

            _logger.LogInformation("CACHE INVALIDATE: {Key}", AllContactsCacheKey);
            await _cache.RemoveAsync(AllContactsCacheKey, ct);

            return entity.Id;
        }

        public async Task UpdateAsync(ContactRequest request, CancellationToken ct = default)
        {
            var entity = await _context.Contacts.FindAsync(new object[] { request.Id }, ct);
            if (entity == null)
            {
                _logger.LogWarning("Update skipped — contact {Id} not found", request.Id);
                return;
            }

            entity.Name = request.Name;
            entity.Surname = request.Surname;
            entity.Email = request.Email;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("CACHE INVALIDATE: {Key1}, {Key2}", AllContactsCacheKey, ContactCacheKey(request.Id));
            await _cache.RemoveAsync(AllContactsCacheKey, ct);
            await _cache.RemoveAsync(ContactCacheKey(request.Id), ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _context.Contacts.FindAsync(new object[] { id }, ct);
            if (entity == null)
            {
                _logger.LogWarning("Delete skipped — contact {Id} not found", id);
                return;
            }

            _context.Contacts.Remove(entity);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("CACHE INVALIDATE: {Key1}, {Key2}", AllContactsCacheKey, ContactCacheKey(id));
            await _cache.RemoveAsync(AllContactsCacheKey, ct);
            await _cache.RemoveAsync(ContactCacheKey(id), ct);
        }

        public async Task<IEnumerable<ContactResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var cached = await _cache.GetStringAsync(AllContactsCacheKey, ct);
            if (cached != null)
            {
                _logger.LogInformation("CACHE HIT: {Key}", AllContactsCacheKey);
                return JsonSerializer.Deserialize<List<ContactResponse>>(cached)!;
            }

            _logger.LogInformation("CACHE MISS: {Key}", AllContactsCacheKey);

            var result = await _context.Contacts
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

            await _cache.SetStringAsync(
                AllContactsCacheKey,
                JsonSerializer.Serialize(result),
                CacheOptions,
                ct);

            _logger.LogInformation("CACHE SET: {Key} ({Count} contacts)", AllContactsCacheKey, result.Count);

            return result;
        }

        public async Task<ContactResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var cached = await _cache.GetStringAsync(ContactCacheKey(id), ct);
            if (cached != null)
            {
                _logger.LogInformation("CACHE HIT: {Key}", ContactCacheKey(id));
                return JsonSerializer.Deserialize<ContactResponse>(cached);
            }

            _logger.LogInformation("CACHE MISS: {Key}", ContactCacheKey(id));

            var contact = await _context.Contacts
                .Include(c => c.ClientContacts)
                    .ThenInclude(cc => cc.Client)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (contact == null)
            {
                _logger.LogWarning("Contact {Id} not found in database", id);
                return null;
            }

            var result = new ContactResponse
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

            await _cache.SetStringAsync(
                ContactCacheKey(id),
                JsonSerializer.Serialize(result),
                CacheOptions,
                ct);

            _logger.LogInformation("CACHE SET: {Key}", ContactCacheKey(id));

            return result;
        }

        public async Task LinkClientAsync(int contactId, int clientId, CancellationToken ct = default)
        {
            bool alreadyLinked = await _context.ClientContacts
                .AnyAsync(cc => cc.ContactId == contactId && cc.ClientId == clientId, ct);

            if (alreadyLinked)
            {
                _logger.LogWarning("Link skipped — contact {ContactId} already linked to client {ClientId}", contactId, clientId);
                return;
            }

            _context.ClientContacts.Add(new ClientContact { ContactId = contactId, ClientId = clientId });
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("CACHE INVALIDATE: {Key1}, {Key2}", AllContactsCacheKey, ContactCacheKey(contactId));
            await _cache.RemoveAsync(AllContactsCacheKey, ct);
            await _cache.RemoveAsync(ContactCacheKey(contactId), ct);
        }

        public async Task UnlinkClientAsync(int contactId, int clientId, CancellationToken ct = default)
        {
            var link = await _context.ClientContacts
                .FirstOrDefaultAsync(cc => cc.ContactId == contactId && cc.ClientId == clientId, ct);

            if (link == null)
            {
                _logger.LogWarning("Unlink skipped — no link found between contact {ContactId} and client {ClientId}", contactId, clientId);
                return;
            }

            _context.ClientContacts.Remove(link);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("CACHE INVALIDATE: {Key1}, {Key2}", AllContactsCacheKey, ContactCacheKey(contactId));
            await _cache.RemoveAsync(AllContactsCacheKey, ct);
            await _cache.RemoveAsync(ContactCacheKey(contactId), ct);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken ct = default) =>
            !await _context.Contacts
                .AnyAsync(c => c.Email == email && (excludeId == null || c.Id != excludeId), ct);
    }
}