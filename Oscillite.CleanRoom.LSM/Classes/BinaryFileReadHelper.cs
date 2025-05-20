using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Oscillite.CleanRoom.LSM
{
    public static class BinaryFileReadHelper
    {
        public static T ReadFieldValue<T>(BinaryReader binaryReader, Stream stream, string[] allowedFieldNames, Func<string, T> parseFunc)
        {
            int rawLength = binaryReader.ReadInt32();
            int stringLength = rawLength - Constants.ENCODING_NULL_BYTE_COUNT;

            if (stringLength <= 0)
                throw new Exception("Invalid field length");

            var buffer = new byte[stringLength];
            stream.Read(buffer, 0, stringLength);

            string[] tokens = Encoding.Unicode.GetString(buffer).Split(' ');
            string actualName = tokens[0];

            bool isMatch = allowedFieldNames.Any(allowed =>
                string.Equals(actualName, allowed, StringComparison.OrdinalIgnoreCase));

            if (!isMatch)
                throw new Exception($"Field name mismatch: expected one of [{string.Join(", ", allowedFieldNames)}], got '{actualName}'");

            stream.Position += Constants.ENCODING_NULL_BYTE_COUNT;

            return parseFunc(tokens[1]);
        }

        public static T ReadFieldValue<T>(BinaryReader binaryReader, Stream stream, string expectedFieldName, Func<string, T> parseFunc)
        {
            return ReadFieldValue(binaryReader, stream, new[] { expectedFieldName }, parseFunc);
        }

        public static string ReadStringValueFromFile(BinaryReader binaryReader, Stream stream, string fieldName)
        {
            return ReadFieldValue(binaryReader, stream, fieldName, val => val);
        }

        public static int ReadIntegerValueFromFile(BinaryReader binaryReader, Stream stream, string fieldName)
        {
            return ReadFieldValue(binaryReader, stream, fieldName, int.Parse);
        }

        public static double ReadDoubleValueFromFile(BinaryReader binaryReader, Stream stream, string fieldName)
        {
            // we found sequential numeric bytes, but they had comma delimiters.  switch to dots made them parse.
            return ReadFieldValue(binaryReader, stream, fieldName, s =>
                double.Parse(s.Replace(",", "."), CultureInfo.InvariantCulture));
        }
    }
}
