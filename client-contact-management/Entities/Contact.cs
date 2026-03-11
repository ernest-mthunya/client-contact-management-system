using System.ComponentModel.DataAnnotations;
namespace client_contact_management.Entities
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Surname")]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        public ICollection<ClientContact> ClientContacts { get; set; } = new List<ClientContact>();
    }
}
