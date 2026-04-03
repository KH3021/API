using API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MongoService>();

builder.Services.AddScoped<RoadmapService>();

builder.Services.AddScoped<ProgressService>();

builder.Services.AddScoped<AnalysisService>();

builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapGet("/", () => "API is running 🚀");

app.Run();