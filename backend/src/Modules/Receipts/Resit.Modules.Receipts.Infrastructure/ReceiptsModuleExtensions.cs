using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Resit.Modules.Receipts.Application.Abstractions;
using Resit.Modules.Receipts.Application.UploadReceipts;

namespace Resit.Modules.Receipts.Infrastructure;

public static class ReceiptsModuleExtensions
{
    public static IServiceCollection AddReceiptsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReceiptsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ResitDb")));

        services.Configure<ReceiptStorageOptions>(configuration.GetSection("ReceiptStorage"));

        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IUploadBatchRepository, UploadBatchRepository>();

        var blobConnectionString = configuration["ReceiptStorage:BlobConnectionString"];
        if (!string.IsNullOrWhiteSpace(blobConnectionString))
        {
            services.AddScoped<IReceiptFileStorage, BlobReceiptFileStorage>();
        }
        else
        {
            services.AddScoped<IReceiptFileStorage, LocalReceiptFileStorage>();
        }

        services.AddHttpClient<IReceiptExtractionClient, PythonReceiptExtractionClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExtractionService:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(120);
        });

        services.AddScoped<ReceiptExtractionJobRunner>();

        return services;
    }
}
