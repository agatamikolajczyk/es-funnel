using System;
using GoldenEye.Backend.Core.DDD.Queries;
using GoldenEye.Shared.Core.Objects.General;
using Funnel.Api.Contracts.Issues.Events;

namespace Funnel.Api.Contracts.Issues.Views
{
    public class IssueView: IView<Guid>
    {
        public IssueType Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        public Guid Id { get; set; }

        object IHaveId.Id => Id;

        public void Apply(IssueCreated @event)
        {
            Id = @event.IssueId;
            Type = @event.Type;
            Title = @event.Title;
            Description = @event.Description;
        }

        public void Apply(IssueUpdated @event)
        {
            Id = @event.IssueId;
            Type = @event.Type;
            Title = @event.Title;
            Description = @event.Description;
        }
    }
}
