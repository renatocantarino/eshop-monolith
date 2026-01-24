var builder = DistributedApplication.CreateBuilder(args);


var postgresDb = builder.AddPostgres("postgres")
                        .WithPgAdmin(admin => admin.WithUrlForEndpoint("http", url=> url.DisplayText = "PostgreDB Browser"))
                        .WithDataVolume()
                        .WithLifetime(ContainerLifetime.Persistent);

var eshopDB = postgresDb.AddDatabase("eshopdb");
                        
                        
                        


builder
    .AddProject<Projects.WebApp>("webapp")
    .WithUrlForEndpoint("https", url => url.DisplayText = "EShop WebApp (HTTPS)")
    .WithUrlForEndpoint("http", url => url.DisplayText = "EShop WebApp (HTTP)")
    .WithReference(eshopDB)
    .WaitFor(eshopDB); ;

builder.Build().Run();