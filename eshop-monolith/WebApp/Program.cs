using WebApp.ApiClients;
using WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpClient<OrderApiHttpClient>(client =>
{
    client.BaseAddress = new("https+http://ordering");
});

builder.Services.AddHttpClient<CatalogApiHttpClient>(client =>
{
    client.BaseAddress = new("https+http://catalog");
});

builder.Services.AddHttpClient<BasketApiHttpClient>(client =>
{
    client.BaseAddress = new("https+http://basket");
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();