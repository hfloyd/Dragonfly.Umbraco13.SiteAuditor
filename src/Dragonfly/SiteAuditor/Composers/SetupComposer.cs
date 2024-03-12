#pragma warning disable 1591

namespace Dragonfly.SiteAuditor.Composers
{
    using Dragonfly.NetHelperServices;
    using Dragonfly.SiteAuditor.Services;
    using Dragonfly.UmbracoServices;
    using Microsoft.Extensions.DependencyInjection;
    using Umbraco.Cms.Core.Composing;
    using Umbraco.Cms.Core.DependencyInjection;

    public class SetupComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddMvcCore().AddRazorViewEngine();
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            //builder.Services.AddSingleton<IRazorViewEngine>();
            //  builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            // builder.Services.AddScoped<IServiceProvider, ServiceProvider>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IViewRenderService, Dragonfly.NetHelperServices.ViewRenderService>();
            builder.Services.AddScoped<Dragonfly.UmbracoServices.FileHelperService>();

            builder.Services.AddScoped<Dragonfly.SiteAuditor.Services.DependencyLoader>();
            builder.Services.AddScoped<Dragonfly.SiteAuditor.Services.SiteAuditorService>();
            builder.Services.AddScoped<Dragonfly.SiteAuditor.Services.AuditorInfoService>();
            
            //builder.AddUmbracoOptions<Settings>();

        }

    }

}