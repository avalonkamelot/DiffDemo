using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace DiffDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static string ReadResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;
            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));
            using (Stream? stream = assembly.GetManifestResourceStream(resourcePath))
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
            else
                return string.Empty;
            
        }
    }
}
