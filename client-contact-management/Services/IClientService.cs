using client_contact_management.Entities;
using client_contact_management.Models;

namespace client_contact_management.Services
{
    public interface IClientService
    {
        Task<IEnumerable<ClientResponse>> GetAllAsync(CancellationToken ct = default);
        Task<ClientResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task AddAsync(ClientRequest client, CancellationToken ct = default);
        Task UpdateAsync(ClientRequest client, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task LinkContactAsync(int clientId, int contactId, CancellationToken ct = default);
        Task UnlinkContactAsync(int clientId, int contactId, CancellationToken ct = default);
    }
}
