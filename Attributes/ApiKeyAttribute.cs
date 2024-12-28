using System;

namespace ERP.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute
    {
        public string ApiKey { get; }
        public ApiKeyAttribute(string apiKey)
        {
            ApiKey = apiKey;
        }
    }
}