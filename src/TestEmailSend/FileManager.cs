using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace TestEmailSend
{
    public static class FileManager
    {
        public static async Task<string> ReadResourceFileAsync(string path)
        {
            var result = string.Empty;
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream(path))
                {
                    if (stream != null)
                        using (var reader = new StreamReader(stream))
                        {
                            result = await reader.ReadToEndAsync();
                            return result;
                        }
                }
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine(exp);
            }

            return result;
        }
    }
}
