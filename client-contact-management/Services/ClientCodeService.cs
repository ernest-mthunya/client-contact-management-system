using client_contact_management.Data;
using client_contact_management.Entities;
using Microsoft.EntityFrameworkCore;

namespace client_contact_management.Services
{
    public class ClientCodeService : IClientCodeService
    {
        private readonly ClientContactManagementDbContext _context;
        public ClientCodeService(ClientContactManagementDbContext context)
        {
            _context = context;
        }
        public async Task<string> Generate(string clientName, CancellationToken ct = default)
        {
            var letters = new string(clientName
                .ToUpper()
                .Where(char.IsLetter)
                .ToArray());

            string alpha;
            if (letters.Length >= 3)
            {
                alpha = letters.Substring(0, 3);
            }
            else
            {
                var padChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var padded = letters;
                int padIndex = 0;
                while (padded.Length < 3)
                {
                    padded += padChars[padIndex % 26];
                    padIndex++;
                }
                alpha = padded;
            }

            int counter = 1;
            string code;
            bool exists;

            do
            {
                code = $"{alpha}{counter:D3}";
                exists = await _context.Clients.AnyAsync<Client>(c => c.ClientCode == code, ct);
                counter++;
            }
            while (exists);

            return code;
        }
    }
}
