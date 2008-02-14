using System.IO;
using System.Reflection;

namespace System.Common.Helpers
{
    public static class ApplicationEnvironment
    {
        public static string GetExecutingAssemblyPath()
        {
            Uri uri = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase));
            return uri.LocalPath;
        }
    }
}
