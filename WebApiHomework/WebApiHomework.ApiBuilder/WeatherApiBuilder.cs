using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApiHomework.ApiBuilder.Controllers;
using WebApiHomework.ApiBuilder.Responses;
using WebApiHomework.ApiBuilder.Settings;
using WebApiHomework.Core;

namespace WebApiHomework.ApiBuilder;

public class WeatherApiBuilder
{
    private readonly WebApplicationBuilder _builder;

    private readonly IEnumerable<Type> _dependencies = new[]
    {
        typeof(IWeatherResponseRenderer),
        typeof(IApiSettingsProvider),
        typeof(IWeatherServiceIntegrationFactory)
    };

    public WeatherApiBuilder(string[] args)
    {
        _builder = WebApplication.CreateBuilder(args);
    }
    
    private void SetupSwagger()
    {
        _builder.Services.AddEndpointsApiExplorer();
        _builder.Services.AddSwaggerGen();
    }

    private void SetupControllers()
    {
        _builder.Services.AddControllers().AddApplicationPart(typeof(WeatherApiBuilder).Assembly);
    }

    private void CheckDependencies()
    {
        foreach (var dependency in _dependencies)
        {
            if (_builder.Services.All(service => service.ServiceType != dependency))
            {
                throw new Exception($"Required dependency {dependency} not specified");
            }
        }
    }

    public WeatherApiBuilder SetupDependency(Action<IServiceCollection> action)
    {
        action(_builder.Services);
        return this;
    }

    public WebApplication BuildApplication()
    {
        SetupControllers();
        SetupSwagger();

        var app = _builder.Build();

        app.UseExceptionHandler(EndpointRoute.ErrorEndpoint);
        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(opt => opt.RouteTemplate = $"{EndpointRoute.SwaggerEndpoint}/{{documentName}}/swagger.json");
            app.UseSwaggerUI(opt => opt.RoutePrefix = EndpointRoute.SwaggerEndpoint);
        }

        return app;
    }
}