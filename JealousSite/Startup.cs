using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace JealousSite
{
    public class Startup
    {
        private readonly string defaultFile;

        public Startup(IHostingEnvironment env)
        {
            if (env.IsDevelopment()) defaultFile = "dev-index.html";
            else defaultFile = "index.html";
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<CatUrlRewriter>();

            DefaultFilesOptions dfo = new DefaultFilesOptions();
            dfo.DefaultFileNames.Clear();
            dfo.DefaultFileNames.Add(defaultFile);
            app.UseDefaultFiles(dfo);
            app.UseStaticFiles();
        }
    }
}
