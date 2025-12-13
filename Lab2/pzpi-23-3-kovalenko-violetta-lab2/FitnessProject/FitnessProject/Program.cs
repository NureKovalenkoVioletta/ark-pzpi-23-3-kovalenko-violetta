using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.DAL.Repositories;
using FitnessProject.BLL.Services;
using FitnessProject.BLL.Services.Interfaces;

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

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    try
                    {
                        await DbInitializer.InitializeAsync(context);
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

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
