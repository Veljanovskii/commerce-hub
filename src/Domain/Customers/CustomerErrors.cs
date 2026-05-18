using SharedKernel;

namespace Domain.Customers;

public static class CustomerErrors
{
    public static Error NotFound(Guid customerId) => Error.NotFound(
        "Customers.NotFound",
        $"The customer with the Id = '{customerId}' was not found");

    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        "Customers.EmailAlreadyExists",
        $"A customer with the email = '{email}' already exists");

    public static Error InvalidCredentials() => Error.Problem(
        "Customers.InvalidCredentials",
        "The provided credentials are invalid");

    public static Error Unauthorized() => Error.Problem(
        "Customers.Unauthorized",
        "You are not authorized to perform this action");

    public static Error NotFoundByEmail(string email) => Error.NotFound(
        "Customers.NotFoundByEmail",
        $"The customer with the email = '{email}' was not found");
}
