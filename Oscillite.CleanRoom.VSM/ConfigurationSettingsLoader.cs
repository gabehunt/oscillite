using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;
using Oscillite.CleanRoom.VSM;


namespace Oscillite.CleanRoom.VSM
{
    public static class ConfigurationSettingsLoader
    {
        public static ConfigurationSettings LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("The file does not exist.", filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Step 1: Search for @@ in the first 8KB
                byte[] headerBuffer = new byte[8192];
                int bytesRead = fs.Read(headerBuffer, 0, headerBuffer.Length);
                string headerText = Encoding.UTF8.GetString(headerBuffer, 0, bytesRead);

                int markerIndex = headerText.IndexOf("@@", StringComparison.Ordinal);
                if (markerIndex < 0)
                    throw new InvalidOperationException("'@@' marker not found in the first 8 KB of the file.");

                // Step 2: Move file position right after the marker
                long gzipStartPosition = markerIndex + 2; // +2 for length of '@@'

                fs.Seek(gzipStartPosition, SeekOrigin.Begin);

                // Step 3: Decompress GZip stream
                using (var gzipStream = new GZipStream(fs, CompressionMode.Decompress))
                using (var decompressedReader = new StreamReader(gzipStream, Encoding.UTF8))
                {
                    string xmlContent = decompressedReader.ReadToEnd();
                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(ConfigurationSettings));
                        using (var stringReader = new StringReader(xmlContent))
                        {
                            return (ConfigurationSettings)serializer.Deserialize(stringReader);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"{ex}\r\n{xmlContent}");
                        throw; // rethrow so you don't hide the error
                    }
                }
            }
        }
    }
}