using client_contact_management.Entities;
using client_contact_management.Models;

namespace client_contact_management.Services
{
    public interface IContactService : IBaseService<ContactRequest, ContactResponse>
    {
        Task LinkClientAsync(int contactId, int clientId, CancellationToken ct = default);
        Task UnlinkClientAsync(int contactId, int clientId, CancellationToken ct = default);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken ct = default);
    }
}
