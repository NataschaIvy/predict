using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.ML;
using camapi.core.Models.ML;

namespace camapi
{
    public class Startup
    {
        /// <summary>
        /// private configuration object
        /// </summary>
        /// <value></value>
        private IConfiguration _config;

        private readonly ILogger<Startup> _logger;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            _config = configuration;
            //_logger = logger;
        }

        /// <summary>
        /// configure dependency injection
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {                        
            services.AddPredictionEnginePool<InMemoryImageData, ImagePrediction>().FromFile(_config["ML:ModelPath"]);            

            services.AddControllers();            

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "camapi", Version = "v1" });
            });
        }

        /// <summary>
        /// Configure the HTTP Pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        /// <param name="dbContext"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation($"Environment is {env.EnvironmentName}");
            logger.LogInformation($"Model Path is {_config["ML:ModelPath"]}");

            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseDeveloperExceptionPage();                
            }

            app.UseSwagger();

            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "camapi v1"));

            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
