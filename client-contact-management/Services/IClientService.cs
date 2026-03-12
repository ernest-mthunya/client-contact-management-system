using client_contact_management.Entities;
using client_contact_management.Models;

namespace client_contact_management.Services
{
    public interface IClientService : IBaseService<ClientRequest, ClientResponse>
    {
        Task LinkContactAsync(int clientId, int contactId, CancellationToken ct = default);
        Task UnlinkContactAsync(int clientId, int contactId, CancellationToken ct = default);
    }
}
