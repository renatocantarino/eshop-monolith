var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddCatalogModule(builder.Configuration)
       .AddBasketModule(builder.Configuration)
       .AddOrderModule(builder.Configuration);

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();

app
    .UseCatalogModule()
    .UseBasketModule()
    .UseOrderModule();

app.Run();