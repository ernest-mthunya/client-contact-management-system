using client_contact_management.Entities;
using client_contact_management.Models;

namespace client_contact_management.Services
{
    public class ContactService : IContactService
    {
        public Task AddAsync(ContactRequest contact, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ContactResponse>> GetAllAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Contact?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(ContactRequest contact, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
