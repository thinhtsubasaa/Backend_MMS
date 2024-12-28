using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using ERP.Attributes;

namespace ERP.Filters
{
    public class ApiKeyActionFilter : IActionFilter
    {
        private readonly IConfiguration _configuration;

        public ApiKeyActionFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var apiKeyAction = context.ActionDescriptor.EndpointMetadata.OfType<ApiKeyAttribute>().FirstOrDefault();
            if (apiKeyAction != null)
            {
                var allowedApiKeys = _configuration.GetSection("AllowedApiKeys").Get<List<string>>();
                var apiKey = context.HttpContext.Request.Headers["ApiKey"].FirstOrDefault();
                if (apiKey == null || (apiKeyAction != null && apiKey.ToLower() != apiKeyAction.ApiKey.ToLower()) || !allowedApiKeys.Contains(apiKey))
                {
                    context.Result = new ContentResult()
                    {
                        StatusCode = 401,
                        Content = apiKey == null ? "Cần nhập ApiKey để xác thực" : "ApiKey không hợp lệ"
                    };
                    return;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }

}