using System.ComponentModel.DataAnnotations;

namespace client_contact_management.Models
{
    public class ClientRequest
    {
        [Required(ErrorMessage = "Client name is required.")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        public string? ClientCode { get; set; }

        public int? Id { get; set; }
    }
}
