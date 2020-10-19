using Bim360.Assembler.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Server.Assembler.Domain;
using Server.Assembler.ModelExportService.Services;

//using Swashbuckle.AspNetCore.Swagger;

namespace Server.Assembler.Api
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
      services.AddMvc(options => options.EnableEndpointRouting = false)
        .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
      services.AddSingleton<IExportService, ExportService>();
      services.AddTransient<DocumentManagementService>();

      // confifure options
      services.Configure<Perfomance>(Configuration.GetSection("Perfomance"));
      // Swagger
      //services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "Navis API", Version = "v1"}); });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseRouting();

      if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

      // Make less noise when log HTTP requests,
      // by combining multiple events into one
      app.UseSerilogRequestLogging();

      //app.UseSwagger();
      //app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Navis API v1"); });
      app.UseDefaultFiles();
      app.UseStaticFiles();
      app.UseHttpsRedirection();
      app.UseMvc();
      //app.UseEndpoints(endpoints =>
      //{
      //  //endpoints.MapHealthChecks("/health");
      //  endpoints.MapDefaultControllerRoute();
      //});
    }
  }
}