using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "📱 PhoneStore API",
        Version = "v1",
        Description = """
            REST API for managing phone inventory.

            **Features:**
            - Browse all products
            - Add new phones
            - Update existing stock and pricing
            - Delete products

            All endpoints return JSON. Use the **Try it out** button to test live.
            """,
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "PhoneStore Team",
            Email = "support@phonestore.local"
        }
    });

    // Group endpoints with descriptions
    options.TagActionsBy(api => [api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Other"]);

    // Include XML doc comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ✅ CORS (IMPORTANT)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PhoneStore API v1");
        options.DocumentTitle = "PhoneStore API";
        options.DefaultModelsExpandDepth(-1);   // hide schemas section by default
        options.DefaultModelExpandDepth(2);
        options.DisplayRequestDuration();        // show how long each request took
        options.EnableFilter();                  // search/filter bar
        options.EnableTryItOutByDefault();       // try-it-out open by default
        options.InjectStylesheet("/swagger-ui/custom.css");
        options.HeadContent = """<link rel='icon' href='data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><text y=".9em" font-size="90">📱</text></svg>'/>""";
    });

    // Serve custom Swagger CSS — excluded from API docs
    app.MapGet("/swagger-ui/custom.css", () =>
        Results.Content("""
            body { font-family: 'Segoe UI', sans-serif; }
            .swagger-ui .topbar { background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%) !important; }
            .swagger-ui .topbar-wrapper img { display: none; }
            .swagger-ui .topbar-wrapper::before { content: '📱 PhoneStore API'; color: white; font-size: 1.3rem; font-weight: 700; letter-spacing: .5px; }
            .swagger-ui .topbar .download-url-wrapper { display: none; }
            .swagger-ui .info .title { color: #0f3460; font-size: 2rem; }
            .swagger-ui .info { margin: 20px 0; }
            .swagger-ui .scheme-container { background: #f8f9ff; padding: 10px 20px; border-radius: 8px; box-shadow: 0 1px 4px rgba(0,0,0,.1); }
            .swagger-ui .opblock.opblock-get .opblock-summary-method { background: #61affe; }
            .swagger-ui .opblock.opblock-post .opblock-summary-method { background: #49cc90; }
            .swagger-ui .opblock.opblock-put .opblock-summary-method { background: #fca130; }
            .swagger-ui .opblock.opblock-delete .opblock-summary-method { background: #f93e3e; }
            .swagger-ui .opblock.opblock-get { border-color: #61affe; background: rgba(97,175,254,.05); }
            .swagger-ui .opblock.opblock-post { border-color: #49cc90; background: rgba(73,204,144,.05); }
            .swagger-ui .opblock.opblock-put { border-color: #fca130; background: rgba(252,161,48,.05); }
            .swagger-ui .opblock.opblock-delete { border-color: #f93e3e; background: rgba(249,62,62,.05); }
            .swagger-ui .btn.execute { background: #0f3460 !important; border-color: #0f3460 !important; border-radius: 6px; }
            .swagger-ui .btn.execute:hover { background: #16213e !important; }
            .swagger-ui table thead tr th { background: #f0f4ff; color: #0f3460; }
            .swagger-ui .opblock-tag { font-size: 1.1rem; color: #1a1a2e; border-bottom: 2px solid #0f3460; padding-bottom: 6px; }
            .swagger-ui .filter-container { background: #f8f9ff; padding: 8px; border-radius: 6px; margin-bottom: 12px; }
        """, "text/css"))
        .ExcludeFromDescription();
}

// ✅ Enable CORS
app.UseCors("AllowAll");

// ✅ Prometheus metrics — exposes /metrics for Prometheus scraping
app.UseMetricServer();
app.UseHttpMetrics();

app.UseAuthorization();

// Health check endpoint for K8s probes
app.MapGet("/health", () => Results.Ok("Healthy"));

app.MapControllers();

app.Run();