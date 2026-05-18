using System.Diagnostics;
using System.Windows.Input;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;
using static Application.Abstractions.Behaviors.LoggingDecorator;

namespace Application.Abstractions.Behaviors;

internal static class LoggingDecorator
{
    internal sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Processing command {Command}", commandName);
            }

            using Activity? activity = Activity.Current?.Source.StartActivity(commandName);
            activity?.SetTag("messaging.command.name", commandName);

            Result<TResponse> result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Completed command {Command}", commandName);
                }
            }
            else
            {
                var data = new Dictionary<string, object>
                {
                    ["Error"] = result.Error
                };
                using (logger.BeginScope(data))
                {
                    logger.LogError("Completed command {Command} with error", commandName);
                }

                activity?.SetStatus(ActivityStatusCode.Error, result.Error.Code);
            }

            return result;
        }
    }

    internal sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : Messaging.ICommand
    {
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            string commandName = typeof(TCommand).Name;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Processing command {Command}", commandName);
            }

            using Activity? activity = Activity.Current?.Source.StartActivity(commandName);
            activity?.SetTag("messaging.command.name", commandName);

            Result result = await innerHandler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Completed command {Command}", commandName);
                }
            }
            else
            {
                var data = new Dictionary<string, object>
                {
                    ["Error"] = result.Error
                };
                using (logger.BeginScope(data))
                {
                    logger.LogError("Completed command {Command} with error", commandName);
                }

                activity?.SetStatus(ActivityStatusCode.Error, result.Error.Code);
            }

            return result;
        }
    }

    internal sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            string queryName = typeof(TQuery).Name;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Processing query {Query}", queryName);
            }

            using Activity? activity = Activity.Current?.Source.StartActivity(queryName);
            activity?.SetTag("messaging.query.name", queryName);

            Result<TResponse> result = await innerHandler.Handle(query, cancellationToken);

            if (result.IsSuccess)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Completed query {Query}", queryName);
                }
            }
            else
            {
                var data = new Dictionary<string, object>
                {
                    ["Error"] = result.Error
                };
                using (logger.BeginScope(data))
                {
                    logger.LogError("Completed query {Query} with error", queryName);
                }

                activity?.SetStatus(ActivityStatusCode.Error, result.Error.Code);
            }

            return result;
        }
    }
}
