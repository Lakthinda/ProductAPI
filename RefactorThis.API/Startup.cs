using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RefactorThis.API.Services;

namespace RefactorThis.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
                        
            var connection = Configuration["connectionStrings:productConnectionString"];
            
            services.AddDbContext<Entities.ProductDBContext>(options => options.UseSqlServer(connection));
            
            services.AddScoped<IProductRepository, ProductRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
                              IHostingEnvironment env, 
                              ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entities.Product, Models.ProductDto>();
                cfg.CreateMap<Entities.ProductOption, Models.ProductOptionDto>();

                cfg.CreateMap<Models.ProductForCreationDto, Entities.Product>();
                cfg.CreateMap<Models.ProductForUpdateDto, Entities.Product>();
                cfg.CreateMap<Models.ProductOptionForCreationDto, Entities.ProductOption>();
                cfg.CreateMap<Models.ProductOptionForUpdateDto, Entities.ProductOption>();
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
