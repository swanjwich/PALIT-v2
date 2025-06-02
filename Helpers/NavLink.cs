using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http;

namespace cce106_palit.Helpers
{
    [HtmlTargetElement("nav-link", Attributes = "asp-controller, asp-action")]
    public class NavLink : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public NavLink(IHttpContextAccessor httpContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        [HtmlAttributeName("asp-controller")]
        public string Controller { get; set; } = string.Empty;

        [HtmlAttributeName("asp-action")]
        public string Action { get; set; } = string.Empty;

        [HtmlAttributeName("asp-route-id")]
        public string? RouteId { get; set; }

        public bool Active { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                output.SuppressOutput();
                return;
            }

            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

            var generatedUrl = urlHelper.Action(Action, Controller, RouteId != null ? new { id = RouteId } : null);
            var currentPath = httpContext.Request.Path;
            bool isActive = Active || currentPath == generatedUrl;

            string activeClass = isActive
                ? "relative block text-[var(--color-primary)] px-3 py-2 font-medium before:absolute before:left-0 before:bottom-0 before:w-full before:h-[2px] before:bg-[var(--color-primary)] dark:text-gray-300 dark:hover:text-blue-400"
                : "relative block text-black px-3 py-2 font-medium hover:text-[var(--color-primary)] before:absolute before:left-0 before:bottom-0 before:w-0 before:h-[2px] before:bg-[var(--color-primary)] before:transition-all before:duration-300 hover:before:w-full dark:text-gray-300 dark:hover:text-blue-400";

            output.TagName = "a";
            output.Attributes.SetAttribute("href", generatedUrl);
            output.Attributes.SetAttribute("class", activeClass);
        }
    }
}
