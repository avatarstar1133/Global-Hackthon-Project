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
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));
builder.Services.AddHttpClient<IAiClient, OpenAiResponsesClient>((sp, c) => { var o = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAiOptions>>().Value; c.BaseAddress = new Uri(o.BaseUrl); c.Timeout = TimeSpan.FromSeconds(45); });
builder.Services.AddCors(o => o.AddPolicy("frontend", p => p.WithOrigins(builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddRateLimiter(o => { o.RejectionStatusCode = 429; o.AddPolicy("api", ctx => RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })); });
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
app.MapGet("/", () => Results.Ok(new { name = "Bridge API", status = "running", health = "/health", frontend = builder.Configuration["FrontendOrigin"] }));
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTimeOffset.UtcNow }));
app.MapBridgeEndpoints();
using (var scope = app.Services.CreateScope()) { var db = scope.ServiceProvider.GetRequiredService<BridgeDbContext>(); await db.Database.MigrateAsync(); await DatabaseSeeder.SeedAsync(db); }
app.Run();
public partial class Program;
