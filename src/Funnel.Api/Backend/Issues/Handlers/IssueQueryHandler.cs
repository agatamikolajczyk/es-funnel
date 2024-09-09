using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GoldenEye.Backend.Core.DDD.Queries;
using GoldenEye.Backend.Core.Repositories;
using Funnel.Api.Contracts.Issues.Queries;
using Funnel.Api.Contracts.Issues.Views;
using Marten;

namespace Funnel.Api.Backend.Issues.Handlers
{
    internal class IssueQueryHandler:
        IQueryHandler<GetIssues, IReadOnlyList<IssueView>>,
        IQueryHandler<GetIssue, IssueView>
    {
        private readonly IReadonlyRepository<IssueView> repository;

        public IssueQueryHandler(IReadonlyRepository<IssueView> repository)
        {
            this.repository = repository ?? throw new ArgumentException(nameof(repository));
        }

        public Task<IssueView> Handle(GetIssue message, CancellationToken cancellationToken)
        {
            return repository.GetByIdAsync(message.Id, cancellationToken);
        }

        public Task<IReadOnlyList<IssueView>> Handle(GetIssues message, CancellationToken cancellationToken)
        {
            return repository
                .Query()
                .ToListAsync(cancellationToken);
        }
    }
}
