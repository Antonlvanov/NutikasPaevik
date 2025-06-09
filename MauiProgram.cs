using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using NutikasPaevik.Database;

namespace NutikasPaevik
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            System.Diagnostics.Debug.WriteLine("MauiProgram.CreateMauiApp started.");
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiCommunityToolkit();

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Database.db");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"), ServiceLifetime.Scoped);

            builder.Services.AddSingleton<IServiceScopeFactory>
                (provider => provider.GetRequiredService<IServiceProvider>()
                as IServiceScopeFactory);
            builder.Services.AddSingleton<DiaryViewModel>();
            builder.Services.AddSingleton<PlannerViewModel>();
            builder.Services.AddSingleton<CalendarViewModel>();
            builder.Services.AddSingleton<HomePageViewModel>();
            builder.Services.AddSingleton<AppSettings>();
            builder.Services.AddSingleton<UserService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            System.Diagnostics.Debug.WriteLine("Services registered successfully.");

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    //dbContext.Database.EnsureDeleted();
                    dbContext.Database.EnsureCreated();
                    System.Diagnostics.Debug.WriteLine("Database created successfully.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Database creation error: {ex.Message}");
                }
            }

            App.Services = app.Services;
            System.Diagnostics.Debug.WriteLine("MauiProgram finished.");
            return app;
        }
    }
}








