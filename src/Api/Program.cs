using Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiSetup(builder.Configuration);

var app = builder.Build();

app.UseApiSetup();

app.Run();

public partial class Program;
