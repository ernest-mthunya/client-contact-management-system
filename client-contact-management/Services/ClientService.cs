using client_contact_management.Models;

namespace client_contact_management.Services
{
    public class ClientService : IClientService
    {
        public Task AddAsync(ClientRequest client, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ClientResponse>> GetAllAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ClientResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task LinkContactAsync(int clientId, int contactId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UnlinkContactAsync(int clientId, int contactId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(ClientRequest client, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
