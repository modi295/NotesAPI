using System.Text; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotesAPI.Models;
using NotesAPI.Helpers;
using Quartz;
using NotesAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Serilog;
using FluentValidation;
using FluentValidation.AspNetCore;
using NotesAPI.Validator;
using NotesAPI.Endpoints;
using NotesAPI.DTO.Mapper;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure DbContext
builder.Services.AddDbContext<NotesApplicationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// 3. Configure caching
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 10240; // Cache size limit
});

// 4. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000", "http://192.168.2.39:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));
});

// 5. Configure Authentication (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddFluentValidationAutoValidation();

// 6. Register repositories and helpers (DI)
builder.Services.AddTransient<IOtpGenerator, OtpGenerator>();
builder.Services.AddScoped<ISupportRepository, SupportRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<ILookupRepository, LookupRepository>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateLookupDtoValidator>();

builder.Services.AddAutoMapper(typeof(UserMappingProfile));


// 7. Quartz scheduler setup
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("OtpCleanupJob");

    q.AddJob<OtpCleanupJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("OtpCleanupTrigger")
        .WithCronSchedule("0 0 0 * * ?")); // Runs daily at midnight
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// 8. API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// 9. Versioned API Explorer (for Swagger)
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// 10. Swagger configuration
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// 11. Serilog logging setup
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
);

// Configure host URLs
builder.WebHost.UseUrls("http://localhost:5020", "http://192.168.2.39:5020");


var app = builder.Build();

// Middleware pipeline starts here

// 1. Serilog request logging
app.UseSerilogRequestLogging();

// 2. Enable CORS
app.UseCors("AllowFrontend");

// 3. Swagger UI in development only
if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", desc.GroupName.ToUpperInvariant());
        }
    });
}

// 4. HTTPS redirection
app.UseHttpsRedirection();

// 5. Authentication & Authorization (add Authentication middleware if you want to validate JWT on requests)
app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api").WithTags("Lookup").WithOpenApi().MapLookupApi();

// 6. Map controllers (endpoints)
app.MapControllers();

// Run the app
app.Run();
