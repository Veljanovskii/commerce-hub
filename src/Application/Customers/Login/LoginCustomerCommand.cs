using Application.Abstractions.Messaging;

namespace Application.Customers.Login;

public sealed class LoginCustomerCommand : ICommand<string>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
