namespace client_contact_management.Services
{
    public interface IClientCodeService
    {
        Task<string> Generate(string clientName, CancellationToken ct = default);
    }
}
