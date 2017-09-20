using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Clr.Loader.Cli.Helpers
{
    public static class IoHelper
    {

        public static string PathFormater(string Path)
        {
            if (!System.IO.Path.IsPathRooted(Path))
            {
                Path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), Path);
            }

            return Path;
        }

        public static string Filecheck(string fullPath)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }


        public static void WriteXmlStringToFile<T>(string Path, T Data)
        {
            using (var writer = new XmlTextWriter(Path, Encoding.UTF8))
            {
                var serializer = new XmlSerializer(typeof(T));

                using (var memStream = new MemoryStream())
                {
                    serializer.Serialize(memStream, Data);
                    memStream.Seek(0, SeekOrigin.Begin);
                    var document = new XmlDocument();
                    document.Load(memStream);
                    writer.Formatting = Formatting.Indented;
                    document.WriteTo(writer);
                }
            }
        }
    }
}
