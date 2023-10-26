using System;
using FluentValidation;
using GoldenEye.Backend.Core.DDD.Queries;
using Funnel.Api.Contracts.Issues.Views;

namespace Funnel.Api.Contracts.Issues.Queries
{
    public class GetIssue: IQuery<IssueView>
    {
        public GetIssue(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }

    public class GetIssueValidator: AbstractValidator<GetIssue>
    {
        public GetIssueValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
        }
    }
}
