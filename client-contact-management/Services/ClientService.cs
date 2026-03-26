using client_contact_management.Data;
using client_contact_management.Entities;
using client_contact_management.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace client_contact_management.Services
{
    public class ClientService : IClientService
    {
        private readonly ClientContactManagementDbContext _context;
        private readonly IClientCodeService _codeService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ClientService> _logger;

        private const string AllClientsCacheKey = "clients_all";
        private static string ClientCacheKey(int id) => $"client_{id}";

        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        public ClientService(
            ClientContactManagementDbContext context,
            IClientCodeService codeService,
            IDistributedCache cache,
            ILogger<ClientService> logger)
        {
            _context = context;
            _codeService = codeService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<int> AddAsync(ClientRequest request, CancellationToken ct = default)
        {
            string clientCode = await _codeService.Generate(request.Name, ct);
            var entity = new Client { Name = request.Name, ClientCode = clientCode };
            _context.Clients.Add(entity);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("CACHE INVALIDATE: {Key}", AllClientsCacheKey);
            await _cache.RemoveAsync(AllClientsCacheKey, ct);

            return entity.Id;
        }

        public async Task UpdateAsync(ClientRequest request, CancellationToken ct = default)
        {
            var entity = await _context.Clients.FindAsync(new object[] { request.Id }, ct);
            if (entity == null)
            {
                _logger.LogWarning("Update skipped — client {Id} not found", request.Id);
                return;
            }

            entity.Name = request.Name;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("CACHE INVALIDATE: {Key1}, {Key2}", AllClientsCacheKey, ClientCacheKey(request.Id));
            await _cache.RemoveAsync(AllClientsCacheKey, ct);
            await _cache.RemoveAsync(ClientCacheKey(request.Id), ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entity = await _context.Clients.FindAsync(new object[] { id }, ct);
            if (entity == null)
            {
                _logger.LogWarning("Delete skipped — client {Id} not found", id);
                return;
            }

            _context.Clients.Remove(entity);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("CACHE INVALIDATE: {Key1}, {Key2}", AllClientsCacheKey, ClientCacheKey(id));
            await _cache.RemoveAsync(AllClientsCacheKey, ct);
            await _cache.RemoveAsync(ClientCacheKey(id), ct);
        }

        public async Task<IEnumerable<ClientResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var cached = await _cache.GetStringAsync(AllClientsCacheKey, ct);
            if (cached != null)
            {
                _logger.LogInformation("CACHE HIT: {Key}", AllClientsCacheKey);
                return JsonSerializer.Deserialize<List<ClientResponse>>(cached)!;
            }

            _logger.LogInformation("CACHE MISS: {Key}", AllClientsCacheKey);

            var result = await _context.Clients
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

            await _cache.SetStringAsync(
                AllClientsCacheKey,
                JsonSerializer.Serialize(result),
                CacheOptions,
                ct);

            _logger.LogInformation("CACHE SET: {Key} ({Count} clients)", AllClientsCacheKey, result.Count);

            return result;
        }

        public async Task<ClientResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var cached = await _cache.GetStringAsync(ClientCacheKey(id), ct);
            if (cached != null)
            {
                _logger.LogInformation("CACHE HIT: {Key}", ClientCacheKey(id));
                return JsonSerializer.Deserialize<ClientResponse>(cached);
            }

            _logger.LogInformation("CACHE MISS: {Key}", ClientCacheKey(id));

            var client = await _context.Clients
                .Include(c => c.ClientContacts)
                    .ThenInclude(cc => cc.Contact)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (client == null)
            {
                _logger.LogWarning("Client {Id} not found in database", id);
                return null;
            }

            var result = new ClientResponse
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

            await _cache.SetStringAsync(
                ClientCacheKey(id),
                JsonSerializer.Serialize(result),
                CacheOptions,
                ct);

            _logger.LogInformation("CACHE SET: {Key}", ClientCacheKey(id));

            return result;
        }

        public async Task LinkContactAsync(int clientId, int contactId, CancellationToken ct = default)
        {
            bool alreadyLinked = await _context.ClientContacts
                .AnyAsync(cc => cc.ClientId == clientId && cc.ContactId == contactId, ct);

            if (alreadyLinked)
            {
                _logger.LogWarning("Link skipped — client {ClientId} already linked to contact {ContactId}", clientId, contactId);
                return;
            }

            _context.ClientContacts.Add(new ClientContact { ClientId = clientId, ContactId = contactId });
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("CACHE INVALIDATE: {Key1}, {Key2}", AllClientsCacheKey, ClientCacheKey(clientId));
            await _cache.RemoveAsync(AllClientsCacheKey, ct);
            await _cache.RemoveAsync(ClientCacheKey(clientId), ct);
        }

        public async Task UnlinkContactAsync(int clientId, int contactId, CancellationToken ct = default)
        {
            var link = await _context.ClientContacts
                .FirstOrDefaultAsync(cc => cc.ClientId == clientId && cc.ContactId == contactId, ct);

            if (link == null)
            {
                _logger.LogWarning("Unlink skipped — no link found between client {ClientId} and contact {ContactId}", clientId, contactId);
                return;
            }

            _context.ClientContacts.Remove(link);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("CACHE INVALIDATE: {Key1}, {Key2}", AllClientsCacheKey, ClientCacheKey(clientId));
            await _cache.RemoveAsync(AllClientsCacheKey, ct);
            await _cache.RemoveAsync(ClientCacheKey(clientId), ct);
        }
    }
}