var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisOutputCache("appcache");

builder.AddCatalogModule(builder.Configuration)
       .AddBasketModule(builder.Configuration)
       .AddOrderModule(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();

app.UseOutputCache();

app.UseCatalogModule()
    .UseBasketModule()
    .UseOrderModule();

app.Run();