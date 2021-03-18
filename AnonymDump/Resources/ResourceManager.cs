using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace AnonymDump.Resources
{
    public static class ResourceManager
    {
        public static byte[] GetEmbedded(string fileName)
        {
            var provider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly(),
                "AnonymDump.Resources");

            using (var stream = provider.GetFileInfo(fileName).CreateReadStream())
            {
                if (stream == null)
                    return null;

                using (var reader = new BinaryReader(stream))
                {
                    return reader.ReadBytes((int)reader.BaseStream.Length);
                }
            }
        }

        public static string GetEmbeddedAsString(string fileName)
        {
            var provider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly(),
                "AnonymDump.Resources");

            using (var stream = provider.GetFileInfo(fileName).CreateReadStream())
            {
                if (stream == null)
                    return null;

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
