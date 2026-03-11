using System.ComponentModel.DataAnnotations;

namespace client_contact_management.Entities;
public class Client
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Client Code")]
    public string? ClientCode { get; set; }

    public ICollection<ClientContact> ClientContacts { get; set; } = new List<ClientContact>();
}
