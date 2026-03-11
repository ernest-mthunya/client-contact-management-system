using client_contact_management.Entities;
using client_contact_management.Models;

namespace client_contact_management.Services
{
    public interface IContactService
    {
        Task<int> AddAsync(ContactRequest contact, CancellationToken ct = default);
        Task UpdateAsync(ContactRequest contact, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<ContactResponse>> GetAllAsync(CancellationToken ct = default);
        Task<ContactResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task LinkClientAsync(int contactId, int clientId, CancellationToken ct = default);
        Task UnlinkClientAsync(int contactId, int clientId, CancellationToken ct = default);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken ct = default);
    }
}
