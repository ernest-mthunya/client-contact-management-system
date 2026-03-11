namespace client_contact_management.Models
{
    public class ClientResponse
    {
        public required string Name { get; set; }

        public required string ClientCode { get; set; }

        public required int NumberOfContactLinked { get; set; }

        public required int Id { get; set; }
    }
}
