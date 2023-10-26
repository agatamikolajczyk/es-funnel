using System;
using GoldenEye.Backend.Core.DDD.Events;

namespace Funnel.Api.Contracts.Issues.Events
{
    public class IssueDeleted: IEvent
    {
        public IssueDeleted(Guid issueId)
        {
            IssueId = issueId;
        }

        public Guid IssueId { get; }

        public Guid StreamId => IssueId;
    }
}
