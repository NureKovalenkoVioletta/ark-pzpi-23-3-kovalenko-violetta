using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.DAL.Repositories;
using FitnessProject.BLL.Services;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Threading;

namespace FitnessProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddAutoMapper(typeof(Program));

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            builder.Services.AddScoped<ITemplateDietPlanRepository, TemplateDietPlanRepository>();
            builder.Services.AddScoped<IDailyDietPlanRepository, DailyDietPlanRepository>();
            builder.Services.AddScoped<IMealRepository, MealRepository>();
            builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IMealRecipeRepository, MealRecipeRepository>();
            builder.Services.AddScoped<IRecipeProductRepository, RecipeProductRepository>();
            builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();
            builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
            builder.Services.AddScoped<ITelemetrySampleRepository, TelemetrySampleRepository>();
            builder.Services.AddScoped<ISleepRecordRepository, SleepRecordRepository>();
            builder.Services.AddScoped<ITrainingSessionRepository, TrainingSessionRepository>();
            builder.Services.AddScoped<IUnitConversionService, UnitConversionService>();
            builder.Services.AddScoped<ILocalizationAdminService, LocalizationAdminService>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<ITemplateDietPlanService, TemplateDietPlanService>();
            builder.Services.AddScoped<IDailyDietPlanService, DailyDietPlanService>();
            builder.Services.AddScoped<IMealService, MealService>();
            builder.Services.AddScoped<IRecipeService, RecipeService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IMealRecipeService, MealRecipeService>();
            builder.Services.AddScoped<IRecipeProductService, RecipeProductService>();
            builder.Services.AddScoped<IRecommendationService, RecommendationService>();
            builder.Services.AddScoped<IDeviceService, DeviceService>();
            builder.Services.AddScoped<ITelemetrySampleService, TelemetrySampleService>();
            builder.Services.AddScoped<ISleepRecordService, SleepRecordService>();
            builder.Services.AddScoped<ITrainingSessionService, TrainingSessionService>();
            builder.Services.AddScoped<ITelemetryProcessingService, TelemetryProcessingService>();
            builder.Services.AddScoped<IMealPlanGeneratorService, MealPlanGeneratorService>();
            builder.Services.AddScoped<IActivityMonitorService, ActivityMonitorService>();
            builder.Services.AddScoped<ISleepAnalysisService, SleepAnalysisService>();
            builder.Services.AddScoped<IDietCorrectionService, DietCorrectionService>();
            builder.Services.AddScoped<IStatisticsService, StatisticsService>();

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Basic";
                options.DefaultChallengeScheme = "Basic";
            }).AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);
            builder.Services.AddAuthorization();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FitnessProject API", Version = "v1" });
                c.AddSecurityDefinition("basic", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Basic auth header"
                });
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "basic"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            var supportedCultures = new[] { new CultureInfo("uk-UA"), new CultureInfo("en-US") };
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("uk-UA"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                ApplyCurrentCultureToResponseHeaders = true
            };
            app.UseRequestLocalization(localizationOptions);

            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    try
                    {
                        await DbInitializer.InitializeAsync(context, forceRecreate: true);

                    }
                    catch (Exception ex)
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while seeding the database.");
                    }
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // В dev можно обойтись без редиректа на https, чтобы не терять Authorization при отсутствии https эндпоинта
            // app.UseHttpsRedirection();

            app.UseAuthentication();

        // Применяем локаль из профиля пользователя (claim locale), если она задана
        app.Use(async (context, next) =>
        {
            var localeClaim = context.User?.FindFirst("locale")?.Value;
            if (!string.IsNullOrWhiteSpace(localeClaim))
            {
                try
                {
                    var culture = new CultureInfo(localeClaim);
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                }
                catch
                {
                    // fallback silently
                }
            }

            await next();
        });

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
