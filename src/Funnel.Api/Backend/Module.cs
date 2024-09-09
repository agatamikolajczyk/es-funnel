using System.Collections.Generic;
using GoldenEye.Backend.Core.DDD.Registration;
using GoldenEye.Backend.Core.Marten.Events.Storage;
using GoldenEye.Backend.Core.Marten.Registration;
using GoldenEye.Backend.Core.Registration;
using GoldenEye.Shared.Core.Modules;
using Funnel.Api.Backend.Issues;
using Funnel.Api.Backend.Issues.Handlers;
using Funnel.Api.Backend.Issues.Projections;
using Funnel.Api.Contracts.Issues.Commands;
using Funnel.Api.Contracts.Issues.Queries;
using Funnel.Api.Contracts.Issues.Views;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Funnel.Api.Backend
{
    public class BackendModule: Module
    {
        private readonly IConfiguration _configuration;

        public BackendModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void Configure(IServiceCollection services)
        {
            ConfigureIntrastructure(services);
            RegisterHandlers(services);
            base.Configure(services);
        }

        public override void Use()
        {
            base.Use();
        }

        private void ConfigureIntrastructure(IServiceCollection services)
        {
            var connectionString = _configuration.GetConnectionString("DDDSample") ??
                                   "PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'";

            services.AddMarten(sp => connectionString, SetupEventStore, "DDDSample");
            services.AddEventStore<MartenEventStore>();
            services.AddEventStorePipeline();
            services.AddValidationPipeline();
            services.AddAllValidators();
        }

        private void SetupEventStore(StoreOptions options)
        {
            options.Events.InlineProjections.AggregateStreamsWith<Issue>();
            options.Events.InlineProjections.Add(new IssueProjection());
        }

        private void RegisterHandlers(IServiceCollection services)
        {
            services.AddMartenDocumentRepository<Issue>();
            services.RegisterCommandHandler<CreateIssue, IssueCommandHandler>();
            services.RegisterCommandHandler<UpdateIssue, IssueCommandHandler>();
            services.RegisterCommandHandler<DeleteIssue, IssueCommandHandler>();

            services.AddMartenDocumentReadonlyRepository<IssueView>();
            services.RegisterQueryHandler<GetIssues, IReadOnlyList<IssueView>, IssueQueryHandler>();
            services.RegisterQueryHandler<GetIssue, IssueView, IssueQueryHandler>();
        }
    }
}
