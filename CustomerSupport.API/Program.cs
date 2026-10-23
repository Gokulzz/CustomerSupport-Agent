using CustomerSupport.Api.Agents;
using CustomerSupport.Api.Authentication;
using CustomerSupport.Api.ExceptionHandling;
using CustomerSupport.Api.Invoices;
using CustomerSupport.Api.OpenApi;
using CustomerSupport.Api.Subscriptions;
using CustomerSupport.Api.SupportTickets;
using CustomerSupport.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiAuthentication();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerWithBearerAuthentication();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    await app.Services.SeedDevelopmentDataAsync();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthenticationEndpoints();
app.MapSubscriptionEndpoints();
app.MapInvoiceEndpoints();
app.MapSupportTicketEndpoints();
app.MapAgentEndpoints();

app.Run();

