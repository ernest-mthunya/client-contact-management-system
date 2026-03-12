namespace client_contact_management.Services
{
    public interface IBaseService<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        Task<int> AddAsync(TRequest request, CancellationToken ct = default);
        Task UpdateAsync(TRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<IEnumerable<TResponse>> GetAllAsync(CancellationToken ct = default);
        Task<TResponse?> GetByIdAsync(int id, CancellationToken ct = default);
    }
}
