using System.Linq;

namespace HTML_Engine_Library
{
    internal static class BuiltInMethodsExecutor
    {
        internal static dynamic TryExecute(dynamic target, string methodName)
        {
            return methodName switch
            {
                "Count()" => (target as IEnumerable<object>).Count(),
                _ => throw new MissingMethodException(methodName),
            };
        }
    }
}
