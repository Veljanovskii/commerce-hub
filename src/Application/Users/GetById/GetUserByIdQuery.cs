using Application.Abstractions.Messaging;
using Application.Users.GetByEmail;

namespace Application.Users.GetById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponse>;
