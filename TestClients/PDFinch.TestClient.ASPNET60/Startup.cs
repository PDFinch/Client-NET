using PDFinch.Client.Common;
using PDFinch.Client.Extensions;

namespace PDFinch.TestClient.ASPNET60
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            // MVC setup: API controllers, Views, Pages, JSON config, and so on.
            services.AddMvc();

            // Add PDFinch API Client
            
            // You would usually just need to call:
            //services.AddPDFinch(_configuration);

            // Here using a custom config section name:
            services.AddPDFinch(_configuration, configSectionName: "PDFinch-Dev")
            // ... and, optionally, manually add configs (here incomplete ones for demonstration purposes).
                    .AddPDFinch(new PdfClientOptions { ApiKey = "fail-01", ApiSecret = "secret-01" })
                    .AddPDFinch(new PDFinchClientSettings { Clients = new[] { new PdfClientOptions { ApiKey = "fail-02", ApiSecret = "secret-02" } } })
                    .AddPDFinch(options => { options.ApiKey = "fail-03"; options.ApiSecret = "secret-03"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
