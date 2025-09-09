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
using MediatR;

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
    options.SizeLimit = 10240;
});

// 4. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000", "http://192.168.2.39:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
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

// 6. Dependency Injection via Assembly Scanning
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IUserRepository>() // use any type from the target assembly
    .AddClasses(classes => classes.AssignableToAny(
        typeof(ISupportRepository),
        typeof(IContactRepository),
        typeof(IUserRepository),
        typeof(IOtpRepository),
        typeof(ILookupRepository),
        typeof(IStudentRepository),
        typeof(IOtpGenerator)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.Scan(scan => scan
    .FromAssemblyOf<CreateLookupDtoValidator>()
    .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddFluentValidationAutoValidation();

// 7. Other Services
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddSignalR();
builder.Services.AddHttpClient<GroqBotService>();
builder.Services.AddScoped<IBotService, GroqBotService>();
builder.Services.AddMediatR(typeof(Program));

// 8. Quartz Scheduler Setup
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

// 9. API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// 10. Swagger Versioned API Explorer
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

// 11. Serilog Logging
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// 12. Configure host URLs
builder.WebHost.UseUrls("http://localhost:5020", "http://192.168.2.39:5020");

var app = builder.Build();

// --- Middleware pipeline ---

app.UseSerilogRequestLogging();

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

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api").MapLookupApi();
app.MapHub<ChatHub>("/chatbot");

app.MapControllers();
app.Run();
