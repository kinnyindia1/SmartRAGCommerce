
using Microsoft.EntityFrameworkCore;
using SmartRAG.Dashboard;
using SmartRAG.Enums;
using SmartRAG.Extensions;
using SmartRAGCommerce.Application.Interfaces.AI;
using SmartRAGCommerce.Application.Interfaces.Analytics;
using SmartRAGCommerce.Application.Interfaces.Categories;
using SmartRAGCommerce.Application.Interfaces.Customers;
using SmartRAGCommerce.Application.Interfaces.Orders;
using SmartRAGCommerce.Application.Interfaces.Payments;
using SmartRAGCommerce.Application.Interfaces.Products;
using SmartRAGCommerce.Application.Interfaces.Repositories;
using SmartRAGCommerce.Application.Services.Analytics;
using SmartRAGCommerce.Application.Services.Categories;
using SmartRAGCommerce.Application.Services.Customers;
using SmartRAGCommerce.Application.Services.Orders;
using SmartRAGCommerce.Application.Services.Payments;
using SmartRAGCommerce.Application.Services.Products;
using SmartRAGCommerce.Infrastructure.AI;
using SmartRAGCommerce.Infrastructure.Analytics;
using SmartRAGCommerce.Infrastructure.Persistence.Context;
using SmartRAGCommerce.Infrastructure.Persistence.Seed;
using SmartRAGCommerce.Infrastructure.Repositories.Categories;
using SmartRAGCommerce.Infrastructure.Repositories.Customers;
using SmartRAGCommerce.Infrastructure.Repositories.Orders;
using SmartRAGCommerce.Infrastructure.Repositories.Payments;
using SmartRAGCommerce.Infrastructure.Repositories.Products;

var builder = WebApplication.CreateBuilder(args);

var analyticsConnection = builder.Configuration.GetConnectionString("AnalyticsConnection");
if (string.IsNullOrWhiteSpace(analyticsConnection))
{
    throw new InvalidOperationException("AnalyticsConnection is missing.");
}
builder.Configuration["SmartRAG:DatabaseConnections:0:ConnectionString"] = analyticsConnection;
builder.Services.AddSmartRag(builder.Configuration, options =>
{
    options.AIProvider = AIProvider.Custom;
    options.StorageProvider = StorageProvider.Qdrant;
    options.Features.EnableDatabaseSearch = false;
    options.Features.EnableDocumentSearch = true;
    options.Features.EnableAudioSearch = false;
    options.Features.EnableImageSearch = false;
    options.Features.EnableMcpSearch = false;
    options.Features.EnableFileWatcher = false;
});
builder.Services.AddSmartRagDashboard(options =>
{
    options.Path = "/smartrag";
});

builder.Services.AddSingleton<AnalyticsConversationStore>();
// Register Controllers
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IDatabaseSchemaProvider, DatabaseSchemaProvider>();
builder.Services.AddScoped<ISmartRagQueryService, SmartRagQueryService>();
builder.Services.AddScoped<IAnalyticsQueryExecutor, SqlQueryExecutor>();
builder.Services.AddHttpClient<IOllamaService, OllamaService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.DocInclusionPredicate((docName, apiDescription) =>
    {
        var path = apiDescription.RelativePath ?? string.Empty;

        return !path.StartsWith(
            "smartrag/",
            StringComparison.OrdinalIgnoreCase);
    });
});

builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSmartRagDashboard("/smartrag");

app.MapControllers();

app.Run();
