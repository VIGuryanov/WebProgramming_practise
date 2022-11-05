using System.Net;
using System.Reflection;

namespace HttpServer.Attributes
{
    internal class ApiController : Attribute
    {
        public string? Path { get; }
        public ApiController() { }
        public ApiController(string path) => Path = path;
    }

    internal class HttpGet : Attribute,IHttpMethod
    {
        public string? Name { get; }
        public HttpGet() { }
        public HttpGet(string name) => Name = name;
    }

    internal class HttpPost : Attribute,IHttpMethod
    {
        public string? Name { get; }
        public HttpPost() { }
        public HttpPost(string name) => Name = name;
    }

    public interface IHttpMethod
    {
        public string? Name { get; }
    }

    public static class AttributesRecognize
    {       
        public static Type? GetClassByControllerAttribute(string controllerName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var controllers = assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(ApiController))).ToList();
            var noUriControllers = controllers.Where(c => c.GetCustomAttributes().Where(x => x is ApiController).Any(x => ((ApiController)x).Path == null)).ToList();
            if (noUriControllers != null)
            {
                var noUriControllersByName = noUriControllers.FirstOrDefault(c => c.Name.ToLower() == controllerName.ToLower());
                if(noUriControllersByName != null)
                    return noUriControllersByName;
                foreach (var noUriController in noUriControllers)
                    controllers.Remove(noUriController);
            }
            var uriControllers = controllers.FirstOrDefault(c => c.GetCustomAttributes().Where(x => x is ApiController).Any(x => ((ApiController)x).Path.ToLower() == controllerName.ToLower()));
            return uriControllers;
        }
        
        public static MethodInfo? GetMethodByHttpAttribute(HttpListenerContext _httpContext, string methodName, Type controller)
        {
            var attributeFormat = $"Http{$"{_httpContext.Request.HttpMethod.Substring(0, 1)}{_httpContext.Request.HttpMethod.Substring(1).ToLower()}"}";
            var attributedMethods = controller.GetMethods().Where(t => t.GetCustomAttributes(true)
                                                                       .Any(attr => attr.GetType().Name == attributeFormat)).ToList();
            var noUriMethods = attributedMethods.Where(m => m.GetCustomAttributes().Where(x => x.GetType().Name == attributeFormat).Any(x => ((IHttpMethod)x).Name == null)).ToList();
            if (noUriMethods != null)
            {
                var noUriMethodsByName = noUriMethods.FirstOrDefault(c => c.Name.ToLower() == methodName.ToLower());
                if(noUriMethodsByName != null)
                    return noUriMethodsByName;
                foreach (var noUriMethod in noUriMethods)
                    attributedMethods.Remove(noUriMethod);
            }
            var uriMethods = attributedMethods.FirstOrDefault(m => m.GetCustomAttributes().Where(x => x.GetType().Name == attributeFormat).Any(x => ((IHttpMethod)x).Name.ToLower() == methodName.ToLower()));
            return uriMethods;
        }
    }
}