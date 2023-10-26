using System;
using GoldenEye.Backend.Core.DDD.Aggregates;
using GoldenEye.Shared.Core.Objects.General;
using Funnel.Api.Contracts.Issues;

namespace Funnel.Api.Backend.Issues
{
    public class Issue: IAggregate
    {
        public Issue()
        {
        }

        public Issue(Guid id, IssueType type, string title, string description)
        {
            Id = id;
            Type = type;
            Title = title;
            Description = description;
        }

        public IssueType Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
        object IHaveId.Id => Id;

        public Guid Id { get; set; }

        public void Update(IssueType type, string title, string description)
        {
            Type = type;
            Title = title;
            Description = description;
        }
    }
}
