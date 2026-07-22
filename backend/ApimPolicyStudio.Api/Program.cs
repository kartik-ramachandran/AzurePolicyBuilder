using ApimPolicyStudio.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as kebab-case strings ("inbound", "on-error") — the
        // frontend filters and displays categories by these string values
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.KebabCaseLower));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:5173",
                  "http://127.0.0.1:5173",
                  "http://localhost:3000",
                  "http://127.0.0.1:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Shared-library storage: configurable so a hosted instance can point at
// persistent storage (e.g. a mounted volume or App Service home directory).
// Precedence: Storage:Directory in appsettings > APIM_STUDIO_DATA env var > LocalAppData.
var storageDirectory = builder.Configuration["Storage:Directory"];
if (string.IsNullOrWhiteSpace(storageDirectory))
{
    storageDirectory = Environment.GetEnvironmentVariable("APIM_STUDIO_DATA");
}
if (string.IsNullOrWhiteSpace(storageDirectory))
{
    storageDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ApimPolicyStudio");
}

// Shared library lives in SQLite; legacy JSON stores are migrated on first run
var database = new StudioDatabase(Path.Combine(storageDirectory, "apim-policy-studio.db"));

// Register services
builder.Services.AddSingleton(database);
builder.Services.AddSingleton<PolicyTemplateService>();
builder.Services.AddSingleton<PolicyValidationService>();
builder.Services.AddSingleton(new PolicyFragmentService(database, Path.Combine(storageDirectory, "fragments.json")));
builder.Services.AddSingleton<ArmTemplateService>();
builder.Services.AddSingleton<ApimProjectGeneratorService>();
builder.Services.AddSingleton<ApimProjectImportService>();
builder.Services.AddSingleton<AzureApimImportService>();
builder.Services.AddSingleton(new ArtifactLibraryService(database, storageDirectory));
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
