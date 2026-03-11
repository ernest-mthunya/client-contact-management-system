namespace client_contact_management.Models
{
    public class ContactResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int NumberOfClientsLinked { get; set; }
        public List<ClientResponse> LinkedClients { get; set; } = new();
    }
}
