namespace client_contact_management.Entities
{
    public class ClientContact
    {
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int ContactId { get; set; }
        public Contact Contact { get; set; } = null!;
    }
}
