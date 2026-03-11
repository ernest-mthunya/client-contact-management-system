namespace client_contact_management.Models
{
    public class ClientResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ClientCode { get; set; } = string.Empty;
        public int NumberOfContactLinked { get; set; }
        public List<ContactResponse> LinkedContacts { get; set; } = new();
    }
}
