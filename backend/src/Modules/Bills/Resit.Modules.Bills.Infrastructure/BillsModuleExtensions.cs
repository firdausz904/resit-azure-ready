using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resit.Modules.Bills.Application.Abstractions;
using Resit.Modules.Bills.Infrastructure.Jobs;

namespace Resit.Modules.Bills.Infrastructure;

public static class BillsModuleExtensions
{
    public static IServiceCollection AddBillsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BillsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ResitDb")));

        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<BillReminderCheckJob>();

        return services;
    }

    public static void MapBillsRecurringJobs(this IRecurringJobManager jobs)
    {
        jobs.AddOrUpdate<BillReminderCheckJob>(
            "bill-reminder-check",
            job => job.RunAsync(null!),
            "0 8 * * *");
    }
}
