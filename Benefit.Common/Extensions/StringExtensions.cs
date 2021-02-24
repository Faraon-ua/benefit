using System.Collections.Generic;
using System.Linq;

namespace Benefit.Common.Extensions
{
    public static class StringExtensions
    {
        public static string EncodeBase64(this System.Text.Encoding encoding, string text)
        {
            if (text == null)
            {
                return null;
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return System.Convert.ToBase64String(textAsBytes);
        }


        public static bool IsJson(this string strInput)
        {
            strInput = strInput.Trim();
            return strInput.StartsWith("{") && strInput.EndsWith("}") ||
                   strInput.StartsWith("[") && strInput.EndsWith("]");
        }

        public static string Clear(this string input)
        {
            if (input == null) return null;
            return input.Trim().Replace("\n", string.Empty).Replace("\t", string.Empty);
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (value == null) return null;
            return value.Substring(0, value.Length > maxLength ? maxLength : value.Length);
        }

        public static string ToPhoneFormat(this string value)
        {
            value = value.Replace("+", string.Empty);
            if (value.StartsWith("0"))
            {
                value = value.Insert(0, "38");
            }
            return value;
        }

        public static string Translit(this string value)
        {
            if (value == null) return null;
            var words = new Dictionary<string, string>();
            words.Add("!", "");
            words.Add(":", "");
            words.Add("%", "proc");
            words.Add("(", "");
            words.Add(")", "");
            words.Add("*", "x");
            words.Add("$", "");
            words.Add("&", "");
            words.Add("№", "n");
            words.Add("+", "plus");
            words.Add("/", "-");
            words.Add("\\", "-proc");
            words.Add("\"", "");
            words.Add("'", "");
            words.Add(".", "");
            words.Add(",", "");
            words.Add(" ", "-");
            words.Add("\t", "-");
            words.Add("\n", "");
            words.Add("а", "a");
            words.Add("б", "b");
            words.Add("в", "v");
            words.Add("г", "g");
            words.Add("д", "d");
            words.Add("е", "e");
            words.Add("ё", "yo");
            words.Add("ж", "zh");
            words.Add("з", "z");
            words.Add("и", "i");
            words.Add("й", "j");
            words.Add("к", "k");
            words.Add("л", "l");
            words.Add("м", "m");
            words.Add("н", "n");
            words.Add("о", "o");
            words.Add("п", "p");
            words.Add("р", "r");
            words.Add("с", "s");
            words.Add("т", "t");
            words.Add("у", "u");
            words.Add("ф", "f");
            words.Add("х", "h");
            words.Add("ц", "c");
            words.Add("ч", "ch");
            words.Add("ш", "sh");
            words.Add("щ", "sch");
            words.Add("ъ", "");
            words.Add("ы", "i");
            words.Add("ь", "");
            words.Add("э", "e");
            words.Add("ю", "yu");
            words.Add("я", "ya");
            words.Add("А", "A");
            words.Add("Б", "B");
            words.Add("В", "V");
            words.Add("Г", "G");
            words.Add("Д", "D");
            words.Add("Е", "E");
            words.Add("Ё", "Yo");
            words.Add("Ж", "Zh");
            words.Add("З", "Z");
            words.Add("И", "I");
            words.Add("Й", "J");
            words.Add("К", "K");
            words.Add("Л", "L");
            words.Add("М", "M");
            words.Add("Н", "N");
            words.Add("О", "O");
            words.Add("П", "P");
            words.Add("Р", "R");
            words.Add("С", "S");
            words.Add("Т", "T");
            words.Add("У", "U");
            words.Add("Ф", "F");
            words.Add("Х", "H");
            words.Add("Ц", "C");
            words.Add("Ч", "Ch");
            words.Add("Ш", "Sh");
            words.Add("Щ", "Sch");
            words.Add("Ъ", "J");
            words.Add("Ы", "I");
            words.Add("Ь", "J");
            words.Add("Э", "E");
            words.Add("Ю", "Yu");
            words.Add("Я", "Ya");

            words.Add("і", "i");
            words.Add("ї", "ji");
            words.Add("є", "e");
            words.Add("ґ", "g");

            words.Add("І", "i");
            words.Add("Ї", "ji");
            words.Add("Є", "e");
            words.Add("Ґ", "g");

            return words.Aggregate(value, (current, pair) => current.Replace(pair.Key, pair.Value)).ToLower();
        }
    }
}
