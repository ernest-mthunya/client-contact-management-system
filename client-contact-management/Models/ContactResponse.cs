namespace client_contact_management.Models
{
    public class ContactResponse
    {
        public required string Name { get; set; }

        public required string Surname { get; set; }

        public required string Email { get; set; }

        public required int NumberOfClientLinked { get; set; }
    }
}
