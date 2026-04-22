using EcommerceAPI.Application.Services;
using EcommerceAPI.Application.Strategies;
using EcommerceAPI.Infrastructure.Persistence;
using EcommerceAPI.Infrastructure.Persistence.Repositories;
using EcommerceAPI.Infrastructure.Search;
using EcommerceAPI.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Nest;

var builder = WebApplication.CreateBuilder(args);

// 1. Framework
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Database (Pomelo MySQL)
var conn = builder.Configuration.GetConnectionString("MySql")
    ?? throw new InvalidOperationException("ConnectionStrings:MySql is not configured.");
// Fixed server version so `dotnet ef migrations add` works without a live DB.
var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(conn, serverVersion));

// 3. App services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// 4. Search strategies — all registered; ProductService picks the first whose CanHandle() returns true
builder.Services.AddScoped<ISearchStrategy, ElasticSearchStrategy>(); // handles ?q=...
builder.Services.AddScoped<ISearchStrategy, MySqlSearchStrategy>();   // no query / category filter

// 5. Seeder (DummyJSON -> MySQL on first startup)
builder.Services.AddHttpClient();
builder.Services.AddScoped<DummyJsonSeeder>();

// 6. Elasticsearch (NEST client + indexer)
var esUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
var esSettings = new ConnectionSettings(new Uri(esUri))
    .DefaultIndex(builder.Configuration["Elasticsearch:Index"] ?? "products");
builder.Services.AddSingleton<IElasticClient>(new ElasticClient(esSettings));
builder.Services.AddScoped<ElasticProductIndexer>();

var app = builder.Build();

// Apply pending migrations + seed on startup
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var seeder = sp.GetRequiredService<DummyJsonSeeder>();
    await seeder.SeedAsync();

    var indexer = sp.GetRequiredService<ElasticProductIndexer>();
    await indexer.EnsureIndexAsync();
    await indexer.BulkIndexAllAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
