using Domain.Customers;

namespace Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(Customer customer);
}
