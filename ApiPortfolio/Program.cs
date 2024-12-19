using ApiPortfolio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure the app to read environment-specific settings
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory()) // Set base path for config files
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddSingleton<DBFunctions>();
builder.Services.AddSingleton<ClassUtility>();

// Add services to the container.
builder.Services.AddControllers();

// Configure JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-auth-server.com";  // URL of the Identity Provider
        options.Audience = "hehe";  // Audience value (e.g., API name)
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Add Bearer Token Authentication to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Description = "Enter 'Bearer' followed by a space and then your JWT token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use exception handling middleware
app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;

        // Check if the exception is a custom "resource not found" exception
        if (exception is ResourceNotFoundException)
        {
            // Return a 404 if resource not found
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("The requested resource was not found." + exception?.Message);
        }
        else
        {
            // Return a 500 for other exceptions
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An unexpected error occurred." + exception?.Message);
        }
    });
});

// Configure exception handling based on the environment
//if (app.Environment.IsDevelopment())
if (true)
{
    app.UseDeveloperExceptionPage(); // Shows detailed error pages in Development
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Shows generic error page in Production
    app.UseHsts(); // Enforces secure HTTP headers in Production
}

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI((c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    //c.RoutePrefix = "docs"; // Serve Swagger UI at the root URL
}));


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseRouting();
app.UseHsts();
app.Run();
