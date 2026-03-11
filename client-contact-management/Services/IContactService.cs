using client_contact_management.Entities;
using client_contact_management.Models;

namespace client_contact_management.Services
{
    public interface IContactService
    {
        Task<IEnumerable<ContactResponse>> GetAllAsync(CancellationToken ct = default);
        Task<Contact?> GetByIdAsync(int id, CancellationToken ct = default);
        Task AddAsync(ContactRequest contact, CancellationToken ct = default);
        Task UpdateAsync(ContactRequest contact, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken ct = default);
    }
}
