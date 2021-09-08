#region
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace Saiive.BlockCypher.Core.Pcl {
    public static class Extensions {
        [DebuggerStepThrough]
        public static byte[] FromHexString(this string str) {
            str.ThrowIfNull("str");

            int outputLength = str.Length / 2;
            var output = new byte[outputLength];

            for (int i = 0; i < outputLength; i++)
                output[i] = Convert.ToByte(str.Substring(i * 2, 2), 16);

            return output;
        }

        [DebuggerStepThrough]
        public static T FromJson<T>(this string str) {
            str.ThrowIfNull("str");

            return JsonConvert.DeserializeObject<T>(str);
        }

        [DebuggerStepThrough]
        public static IEnumerable<string> SplitInParts(this string s, int partLength) {
            s.ThrowIfNull("s");
            partLength.ThrowIf(partLength <= 0, "Part length has to be positive.", "partLength");

            for (int i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        [DebuggerStepThrough]
        public static void ThrowIf<T>(this T obj, bool fail, string error, string parameterName) {
            if (fail)
                throw new ArgumentException(parameterName);
        }

        [DebuggerStepThrough]
        public static void ThrowIfNull<T>(this T obj, string parameterName) {
            if (obj == null)
                throw new ArgumentNullException(parameterName);
        }

        public static string ToHexString(this byte[] byteArray, bool upperCase = false, int splitInGroups = -1,
                                         string separator = " ") {
            string str = BitConverter.ToString(byteArray).Replace("-", "");

            str = upperCase ? str.ToUpper() : str.ToLower();

            if (splitInGroups > 0)
                str = string.Join(separator, str.SplitInParts(splitInGroups));

            return str;
        }

        public static string ToJson(this object obj) {
            return JsonConvert.SerializeObject(obj);
        }

    }
}
