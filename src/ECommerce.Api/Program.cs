using System.Text;
using ECommerce.Api.Common;
using ECommerce.Api.Domain;
using ECommerce.Api.Infrastructure;
using ECommerce.Api.Persistence;
using ECommerce.Api.Persistence.Seed;
using ECommerce.Api.Features.Checkout;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?.Where(origin => !string.IsNullOrWhiteSpace(origin)).ToArray() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString),
        new PostgreSqlStorageOptions { SchemaName = "hangfire" })
    .UseFilter(new AutomaticRetryAttribute { Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail }));
builder.Services.AddHangfireServer();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => Encoding.UTF8.GetByteCount(options.SecretKey) >= 32,
        "JWT secret key must contain at least 32 UTF-8 bytes.")
    .ValidateOnStart();
builder.Services.AddOptions<GoogleOAuthOptions>()
    .Bind(builder.Configuration.GetSection(GoogleOAuthOptions.SectionName));
builder.Services.AddOptions<ClientAppOptions>()
    .Bind(builder.Configuration.GetSection(ClientAppOptions.SectionName))
    .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri) &&
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                         string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment),
        "ClientApp:BaseUrl must be an absolute HTTP(S) origin or base URL without a query or fragment.")
    .ValidateOnStart();
builder.Services.AddOptions<AdminOptions>()
    .Bind(builder.Configuration.GetSection(AdminOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
var emailOptions = builder.Configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>() ?? new EmailOptions();
builder.Services.AddOptions<EmailOptions>()
    .Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
    .Validate(options => options.IsValidForSmtp,
        "When SMTP email is enabled, Email:Username, a valid Email:FromEmail, and a valid Email:Port are required.")
    .ValidateOnStart();
builder.Services.AddOptions<ProductExportOptions>()
    .Bind(builder.Configuration.GetSection(ProductExportOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<SandboxPaymentOptions>()
    .Bind(builder.Configuration.GetSection(SandboxPaymentOptions.SectionName));

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is required.");
var authentication = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
authentication.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Hangfire uses normal browser navigation, which cannot attach the
            // SPA's bearer header. Accept its short-lived, path-scoped cookie there.
            if (context.Request.Path.StartsWithSegments("/hangfire") &&
                context.Request.Cookies.TryGetValue("hangfire_access_token", out var dashboardToken))
                context.Token = dashboardToken;
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt.Issuer,
        ValidateAudience = true,
        ValidAudience = jwt.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});
authentication.AddCookie(IdentityConstants.ExternalScheme, options =>
{
    options.Cookie.Name = "ecommerce.external";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.SlidingExpiration = false;
});
var googleOptions = builder.Configuration.GetSection(GoogleOAuthOptions.SectionName).Get<GoogleOAuthOptions>()
    ?? new GoogleOAuthOptions();
if (googleOptions.IsConfigured)
{
    authentication.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = googleOptions.ClientId;
        options.ClientSecret = googleOptions.ClientSecret;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.CallbackPath = "/signin-google";
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.ClaimActions.MapJsonKey("email_verified", "verified_email");
    });
}
builder.Services.AddAuthorization();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IEmailSender>(services => emailOptions.IsLoggingMode
    ? new LoggingEmailSender(services.GetRequiredService<ILogger<LoggingEmailSender>>())
    : ActivatorUtilities.CreateInstance<SmtpEmailSender>(services));
builder.Services.AddScoped<IProductExcelExporter, ProductExcelExporter>();
builder.Services.AddScoped<ECommerce.Api.Jobs.ProductExcelMailJob>();
builder.Services.AddScoped<ECommerce.Api.Features.Products.ExportProductsToMail.ExportProductsToMailHandler>();
builder.Services.AddScoped<ECommerce.Api.Features.Auth.Register.RegisterHandler>();
builder.Services.AddScoped<ECommerce.Api.Features.Auth.Login.LoginHandler>();
builder.Services.AddScoped<ECommerce.Api.Features.Auth.RefreshToken.RefreshTokenHandler>();
builder.Services.AddScoped<ECommerce.Api.Features.Auth.GoogleLogin.GoogleLoginHandler>();
builder.Services.AddScoped<ECommerce.Api.Features.Auth.GoogleLogin.GoogleLoginExchangeHandler>();
builder.Services.AddScoped<CheckoutHandler>();
builder.Services.AddScoped<ECommerce.Api.Features.Categories.CategoryHandler>();
builder.Services.AddScoped<ECommerce.Api.Features.Brands.BrandHandler>();
builder.Services.AddScoped<ECommerce.Api.Features.Products.ProductHandler>();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddEndpoints(typeof(Program).Assembly);

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors();
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsDevelopment())
        await db.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
    var seedEnabled = builder.Configuration.GetValue<bool?>("Seed:Enabled") ?? app.Environment.IsDevelopment();
    if (seedEnabled)
    {
        var customerPassword = builder.Configuration["Seed:CustomerPassword"];
        if (string.IsNullOrWhiteSpace(customerPassword))
            throw new InvalidOperationException("Seed:CustomerPassword is required when Seed:Enabled is true.");
        await DataSeeder.SeedAsync(scope.ServiceProvider, customerPassword, app.Lifetime.ApplicationStopping);
    }
}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("E-Commerce API")
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .AddHttpAuthentication("Bearer", auth => auth.Token = ""));
}
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new AdminDashboardAuthorizationFilter()]
});
var exportOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<ProductExportOptions>>().Value;
RecurringJob.AddOrUpdate<ECommerce.Api.Jobs.ProductExcelMailJob>(
    "product-excel-mail-daily",
    job => job.RunAsync(exportOptions.RecipientEmail, CancellationToken.None),
    exportOptions.CronExpression);
app.MapEndpoints();
app.Run();

public partial class Program { }
