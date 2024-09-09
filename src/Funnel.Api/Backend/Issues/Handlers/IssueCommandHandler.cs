using System;
using System.Threading;
using System.Threading.Tasks;
using GoldenEye.Backend.Core.DDD.Commands;
using GoldenEye.Backend.Core.DDD.Events;
using GoldenEye.Backend.Core.Repositories;
using Funnel.Api.Contracts.Issues.Commands;
using Funnel.Api.Contracts.Issues.Events;
using MediatR;

namespace Funnel.Api.Backend.Issues.Handlers
{
    internal class IssueCommandHandler:
        ICommandHandler<CreateIssue>,
        ICommandHandler<UpdateIssue>,
        ICommandHandler<DeleteIssue>
    {
        private readonly IEventBus eventBus;
        private readonly IRepository<Issue> repository;

        public IssueCommandHandler(
            IEventBus eventBus,
            IRepository<Issue> repository)
        {
            this.eventBus = eventBus ?? throw new ArgumentException(nameof(eventBus));
            this.repository = repository ?? throw new ArgumentException(nameof(repository));
        }

        public async Task<Unit> Handle(CreateIssue command, CancellationToken cancellationToken)
        {
            var aggregate = new Issue(Guid.NewGuid(), command.Type, command.Title, command.Description);
            await repository.AddAsync(aggregate, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            var @event = new IssueCreated(aggregate.Id, aggregate.Type, aggregate.Title, aggregate.Description);
            await eventBus.PublishAsync(@event, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(DeleteIssue command, CancellationToken cancellationToken)
        {
            await repository.DeleteByIdAsync(command.Id, cancellationToken);

            await repository.SaveChangesAsync(cancellationToken);

            await eventBus.PublishAsync(new IssueDeleted(command.Id), cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(UpdateIssue command, CancellationToken cancellationToken)
        {
            var aggregate = await repository.GetByIdAsync(command.Id, cancellationToken);
            aggregate.Update(command.Type, command.Title, command.Description);
            await repository.UpdateAsync(aggregate, cancellationToken);

            await repository.SaveChangesAsync(cancellationToken);

            var @event = new IssueUpdated(aggregate.Id, aggregate.Type, aggregate.Title, aggregate.Description);
            await eventBus.PublishAsync(@event, cancellationToken);

            return Unit.Value;
        }
    }
}
