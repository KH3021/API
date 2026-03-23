using API.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Mongo service
builder.Services.AddSingleton<MongoService>();

var app = builder.Build();

// ✅ IMPORTANT: Bind port BEFORE running
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

// ✅ Swagger
app.UseSwagger();
app.UseSwaggerUI();

// ✅ Map controllers
app.MapControllers();

app.Run();