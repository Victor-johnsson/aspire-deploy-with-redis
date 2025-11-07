using Azure.Provisioning.Redis;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureAppServiceEnvironment("env");

var cache = builder.AddAzureRedis("cache")
    .WithAccessKeyAuthentication()
    .ConfigureInfrastructure(infra =>
    {
        var redisc = infra.GetProvisionableResources()
                          .OfType<Azure.Provisioning.Redis.RedisResource>()
                          .Single();

        redisc.Sku = new()
        {
            Family = RedisSkuFamily.BasicOrStandard,
            Name = RedisSkuName.Basic,
            Capacity = 0,
        };
    });

var apiService = builder.AddProject<Projects.aspire_deploy_with_redis_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .PublishAsAzureAppServiceWebsite((infra, conf) => { });

builder.AddProject<Projects.aspire_deploy_with_redis_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService)
    .PublishAsAzureAppServiceWebsite((infra, conf) => { });

builder.Build().Run();
