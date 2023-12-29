using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Traefik;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

var yamlLocation = Environment.GetEnvironmentVariable("TRAEFIK_YAML_LOCATION") ?? "/etc/traefik/traefik.yaml";
var traefik = new TraefikHelper(yamlLocation);

app.MapGet("/api/router", (string? routerName) =>
    {
        if (routerName == null)
        {
            var routers = traefik.GetRouters();
            var json = JsonSerializer.Serialize(routers, new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            app.Logger.LogInformation("GetRouter: {0}", "All");
            return json;
        }
        else
        {
            var router = traefik.GetRouter(routerName);
            var json = JsonSerializer.Serialize(router, new JsonSerializerOptions()
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            app.Logger.LogInformation("GetRouter: {0}", routerName);
            return json;
        }
    })
    .WithName("GetRouter")
    .WithOpenApi();

app.MapGet("/api/service", (string? serviceName) =>
{

    if (serviceName == null)
    {
        var services = traefik.GetServices();
        var json = JsonSerializer.Serialize(services, new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        app.Logger.LogInformation("GetService: {0}", "All");
        return json;
    }
    else
    {
        var service = traefik.GetService(serviceName);
        var json = JsonSerializer.Serialize(service, new JsonSerializerOptions()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        app.Logger.LogInformation("GetService: {0}", serviceName);
        return json;
    }
});

app.MapPost("/api/router", (string routerName, Router router) =>
    {
        try
        {
            var addedRouter = traefik.AddRouter(routerName, router);
            if (addedRouter)
            {
                app.Logger.LogInformation("AddRouter: {0}", routerName);
                return new StatusCodeResult(StatusCodes.Status201Created);
            }
            app.Logger.LogError("AddRouter: {0}", "Failed");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        catch (Exception e)
        {
            app.Logger.LogError("AddRouter: {0}", "Failed");
            Console.WriteLine(e);
            throw;
        }
        
    })
    .WithName("AddRouter")
    .WithOpenApi();

app.MapPost("/api/service", (string serviceName, Service service) =>
{
    try
    {
        var addedService = traefik.AddService(serviceName, service);
        if (addedService)
        {
            app.Logger.LogInformation("AddService: {0}", serviceName);
            return new StatusCodeResult(StatusCodes.Status201Created);
        }
        app.Logger.LogError("AddService: {0}", "Failed");
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
    catch (Exception e)
    {
        app.Logger.LogError("AddService: {0}", "Failed");
        Console.WriteLine(e);
        throw;
    }

});

app.MapPut("/api/router", (string routerName, Router router) =>
    {
        try
        {
            var updatedRouter = traefik.UpdateRouter(routerName, router);
            if (updatedRouter)
            {
                app.Logger.LogInformation("UpdateRouter: {0}", routerName);
                return new StatusCodeResult(StatusCodes.Status200OK);
            }
            app.Logger.LogError("UpdateRouter: {0}", "Failed");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        catch (Exception e)
        {
            app.Logger.LogError("UpdateRouter: {0}", "Failed");
            Console.WriteLine(e);
            throw;
        }
        
    })
    .WithName("UpdateRouter")
    .WithOpenApi();

app.MapPut("/api/service", (string serviceName, Service service) =>
{
    try
    {
        var updatedService = traefik.UpdateService(serviceName, service);
        if (updatedService)
        {
            app.Logger.LogInformation("UpdateService: {0}", serviceName);
            return new StatusCodeResult(StatusCodes.Status200OK);
        }
        app.Logger.LogError("UpdateService: {0}", "Failed");
        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
    }
    catch (Exception e)
    {
        app.Logger.LogError("UpdateService: {0}", "Failed");
        Console.WriteLine(e);
        throw;
    }

});

app.MapDelete("/api/router", (string routerName) =>
    {
        try
        {
            var deletedRouter = traefik.DeleteRouter(routerName);
            if (deletedRouter)
            {
                app.Logger.LogInformation("DeleteRouter: {0}", routerName);
                return new StatusCodeResult(StatusCodes.Status200OK);
            }
            app.Logger.LogError("DeleteRouter: {0}", "Failed");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        catch (Exception e)
        {
            app.Logger.LogError("DeleteRouter: {0}", "Failed");
            Console.WriteLine(e);
            throw;
        }
        
    })
    .WithName("DeleteRouter")
    .WithOpenApi();

app.MapDelete("/api/service", (string serviceName) =>
    {
        try
        {
            var deleteService = traefik.DeleteService(serviceName);
            if (deleteService)
            {
                app.Logger.LogInformation("DeleteService: {0}", serviceName);
                return new StatusCodeResult(StatusCodes.Status200OK);
            }
            app.Logger.LogError("DeleteService: {0}", "Failed");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        catch (Exception e)
        {
            app.Logger.LogError("DeleteService", e);
            throw;
        }
        
    })
    .WithName("DeleteService")
    .WithOpenApi();

app.MapGet("/api/middleware", () =>
{
    var middlewares = traefik.GetMiddlewares();
    var json = JsonSerializer.Serialize(middlewares, new JsonSerializerOptions()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
    app.Logger.LogInformation("GetMiddlewares: {0}", "All");
    return json;
});
    

app.MapPost("/api/save", () =>
    {
        try
        {
            var saved = traefik.SaveToFile("/Users/emizac/RiderProjects/TraefikApi/TraefikApi/test.yaml");
            if (saved)
            {
                app.Logger.LogInformation("SaveToFile: {0}", "Success");
                return new StatusCodeResult(StatusCodes.Status200OK);
            }
            app.Logger.LogError("SaveToFile: {0}", "Failed");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        catch (Traefik.ConfigurationInvalidExeption e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    })
    .WithName("SaveToFile")
    .WithOpenApi();

app.Run();
