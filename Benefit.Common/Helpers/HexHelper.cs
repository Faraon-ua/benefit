using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Benefit.Common.Helpers
{
    public class HexHelper
    {
        public static byte[] GetBytes(string str)
        {
            var bytes = UTF8Encoding.UTF8.GetBytes(str);
            return bytes;
        }
        public static string HexBytesToString(List<byte> data)
        {
            return string.Join("", data.ToList().Select(entry => (char)entry));
        }

        public static string AsciiToHexString(string str)
        {
            var hexStr = string.Join("", str.Select(entry => ((int)entry).ToString("X")));
            return hexStr;
        }

        public static string AddGarbage(int bytesNumber, string dataBytes, int startByteNumber, int endByteNumber, string garbageValue)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < bytesNumber; i++)
            {
                if (i >= startByteNumber && i <= endByteNumber-1)
                {
                    var dataByte = dataBytes.Substring((i - startByteNumber)*2, 2);
                    sb.Append(dataByte);
                }
                else
                {
                    sb.Append(garbageValue);
                }
            }
            return sb.ToString();
        }

        public static int SumHexBytesInString(string str)
        {
            const int charsInByte = 2;
            var index = 0;
            int sum = 0;
            do
            {
                var byteInString = str.Substring(index, charsInByte);
                sum += Convert.ToInt32(byteInString, 16);
                index += charsInByte;
            } while (index < str.Length);
            return sum;
        }

        public static List<byte> XOR(List<byte> key, List<byte> salt)
        {
            return key.Select((t, i) => t ^ salt[i]).Select(entry => (byte)entry).ToList();
        }
    }
}
