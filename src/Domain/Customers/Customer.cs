using SharedKernel;

namespace Domain.Customers;

public sealed class Customer : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<Address> Addresses { get; set; } = [];
}
