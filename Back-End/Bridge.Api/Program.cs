using System.Threading.RateLimiting;
using Bridge.Api.Api;
using Bridge.Api.Data;
using Bridge.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BridgeDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("BridgeDb")));
builder.Services.AddSingleton<OnboardingAssessmentService>();
builder.Services.AddSingleton<PromptBuilder>();
builder.Services.AddSingleton<PersonaAssessmentService>();
builder.Services.AddSingleton<PersonaPromptProvider>();
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<PersonaAnalysisOptions>(builder.Configuration.GetSection("PersonaAnalysis"));
builder.Services.AddHttpClient<IAiClient, OpenAiResponsesClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(45);
});
var frontendOrigins = builder.Configuration.GetSection("FrontendOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "http://127.0.0.1:5173"];
builder.Services.AddCors(options => options.AddPolicy("frontend", policy =>
    policy.WithOrigins(frontendOrigins).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("api", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseExceptionHandler();
if (builder.Configuration.GetValue<bool>("HttpsRedirection:Enabled"))
    app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseRateLimiter();
app.MapGet("/", () => Results.Ok(new
{
    name = "Bridge API",
    status = "running",
    health = "/health",
    frontend = frontendOrigins[0]
}));
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow }));
app.MapBridgeEndpoints();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BridgeDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
}
app.Run();

public partial class Program;