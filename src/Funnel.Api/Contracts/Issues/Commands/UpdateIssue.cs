using System;
using FluentValidation;
using GoldenEye.Backend.Core.DDD.Commands;

namespace Funnel.Api.Contracts.Issues.Commands
{
    public class UpdateIssue: ICommand
    {
        public UpdateIssue(Guid id, IssueType type, string title, string description)
        {
            Id = id;
            Type = type;
            Title = title;
            Description = description;
        }

        public Guid Id { get; }

        public IssueType Type { get; }

        public string Title { get; }

        public string Description { get; }
    }

    public class UpdateIssueValidator: AbstractValidator<UpdateIssue>
    {
        public UpdateIssueValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
            RuleFor(r => r.Type).IsInEnum();
            RuleFor(r => r.Title).NotEmpty();
        }
    }
}
