namespace Dragonfly.SiteAuditor.Services
{
	using Dragonfly.NetHelperServices;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Umbraco.Cms.Core.Hosting;
    using Umbraco.Cms.Core.Services;
    using Umbraco.Cms.Core.Web;
    using Umbraco.Cms.Web.Common;

    public class DependencyLoader
    {
        public IHostingEnvironment HostingEnvironment { get; }
        public IHttpContextAccessor ContextAccessor { get; }
        public IUmbracoContextAccessor UmbracoContextAccessor { get; }
     
        public UmbracoHelper UmbHelper;
        public HttpContext Context;
        public ServiceContext Services;
        public FileHelperService DragonflyFileHelperService { get; }

        public DependencyLoader(
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor contextAccessor,
            IUmbracoContextAccessor umbracoContextAccessor,
            FileHelperService fileHelperService,
            ServiceContext serviceContext
           )
        {
            HostingEnvironment = hostingEnvironment;
            ContextAccessor = contextAccessor;
            UmbracoContextAccessor = umbracoContextAccessor;
            UmbHelper = contextAccessor.HttpContext.RequestServices.GetRequiredService<UmbracoHelper>();
            DragonflyFileHelperService = fileHelperService;
            Context = contextAccessor.HttpContext;
            Services = serviceContext;
        }
    }
}
