using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace System.Core.Helpers
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
