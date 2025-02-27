using Microsoft.AspNetCore.Mvc;
using Payfast.Common.Helpers;
using Payfast.Common.Middleware;
using Payfast.Common.Models;
using Payfast.Domain.Interfaces;
using Payfast.Domain.Services;
using Payfast.Mapping;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

LoadEnvironmentVariables();
RegisterServices(builder);
RegisterAutoMapper(builder);
ConfigureAppSettings(builder);
ConfigureControllers(builder);
ConfigureApiVersioning(builder);
ConfigureOpenApi(builder);

var app = builder.Build();

ConfigureMiddleware(app);

app.Run();

void LoadEnvironmentVariables()
{
    var currentDirectory = Directory.GetCurrentDirectory();
    var envFilePath = Path.Combine(currentDirectory, "dev.env");
    EnvironmentConfigLoader.Load(envFilePath);
}

void RegisterServices(WebApplicationBuilder builder)
{
    builder.Services.AddMvcCore().AddApiExplorer();
    builder.Services.AddTransient<IPayfastService, PayfastService>();
}

void RegisterAutoMapper(WebApplicationBuilder builder)
{
    builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
}

void ConfigureAppSettings(WebApplicationBuilder builder)
{
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddEnvironmentVariables();
}

void ConfigureControllers(WebApplicationBuilder builder)
{
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
    });

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            var response = new ApiResponse<object>(
                success: false,
                message: "Validation failed",
                statusCode: StatusCodes.Status400BadRequest,
                errors: errors);

            return new BadRequestObjectResult(response);
        };
    });
}

void ConfigureApiVersioning(WebApplicationBuilder builder)
{
    builder.Services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
    }).AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });
}

void ConfigureOpenApi(WebApplicationBuilder builder)
{
    builder.Services.AddOpenApi();
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    //https://guides.scalar.com/scalar/scalar-api-references/net-integration?utm_source=chatgpt.com#configuration-options__custom-http-client
    app.MapScalarApiReference(options =>
    {
        options.Title = "Payfast Integration API";
        options.ShowSidebar = true;
        options.HideModels = false;
        options.HideDownloadButton = false;
        options.HideTestRequestButton = false;
        options.DarkMode = true;
        options.HideDarkModeToggle = false;
        options.WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl);
        options.WithDefaultFonts(true);
        options.DefaultOpenAllTags = true;
        options.Theme = ScalarTheme.BluePlanet;
        options.Layout = ScalarLayout.Modern;
    });

    app.UseMiddleware<ApiResponseMiddleware>();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
}