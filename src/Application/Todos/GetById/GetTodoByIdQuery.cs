using Application.Abstractions.Messaging;
using Application.Todos.Get;

namespace Application.Todos.GetById;

public sealed record GetTodoByIdQuery(Guid TodoItemId) : IQuery<TodoResponse>;
