using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Todos;
using Microsoft.EntityFrameworkCore;
using Domain.Common;

namespace Application.Todos.Delete;

internal sealed class DeleteTodoCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<DeleteTodoCommand>
{
    public async Task<Result> Handle(DeleteTodoCommand command, CancellationToken cancellationToken)
    {
        // TodoItem? todoItem = await context.TodoItems
        //     .SingleOrDefaultAsync(t => t.Id == command.TodoItemId && t.UserId == userContext.UserId, cancellationToken);
        //
        // if (todoItem is null)
        // {
        //     return Result.Failure(TodoItemErrors.NotFound(command.TodoItemId));
        // }
        //
        // context.TodoItems.Remove(todoItem);
        //
        // todoItem.Raise(new TodoItemDeletedDomainEvent(todoItem.Id));
        //
        // await context.SaveChangesAsync(cancellationToken);

        return await Task.FromResult(Result.Success());
    }
}
