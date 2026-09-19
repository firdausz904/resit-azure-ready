using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.OpenApi;
using Resit.Modules.Bills.Api;
using Resit.Modules.Bills.Infrastructure;
using Resit.Modules.Bills.Infrastructure.Jobs;
using Resit.Modules.Identity;
using Resit.Modules.Notifications;
using Resit.Modules.Receipts.Api;
using Resit.Modules.Receipts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ResitClients", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddNotificationsModule();
builder.Services.AddReceiptsModule(builder.Configuration);
builder.Services.AddBillsModule(builder.Configuration);

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("ResitDb"))));

// OCR service is CPU-bound and effectively serial; default worker count floods it and requests time out in the queue.
builder.Services.AddHangfireServer(options => options.WorkerCount = 2);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Components ??= new OpenApiComponents();
    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    document.Security ??= [];
    document.Security.Add(new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    return Task.CompletedTask;
}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Resit API v1"));
    app.UseHangfireDashboard();

    using var seedScope = app.Services.CreateScope();
    var identityDbContext = seedScope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await IdentitySeeder.EnsureCreatedAsync(identityDbContext, CancellationToken.None);
    await IdentitySeeder.SeedDevelopmentDataAsync(identityDbContext, CancellationToken.None);
}

app.UseCors("ResitClients");
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapReceiptsEndpoints();
app.MapBillsEndpoints();
app.MapHub<ProgressHub>("/hubs/progress").RequireAuthorization();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IRecurringJobManager>().MapBillsRecurringJobs();
}

app.Run();
