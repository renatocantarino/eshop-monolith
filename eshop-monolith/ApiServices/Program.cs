using ApiServices.Data;
using ApiServices.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<EShopDbContext>(connectionName: "EshopDB");

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

app.MapDefaultEndpoints();
app.UseHttpsRedirection();

app.UseMigration();

app.MapApiServiceEndpoints();

app.Run();