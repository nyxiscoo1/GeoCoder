using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GeoCoder.Yandex
{
    public class Substitutor
    {
        public static Substitutor FromFile(string path)
        {
            if (!File.Exists(path))
                return new Substitutor();

            var splitBy = new[] { '\t' };

            var result = new Dictionary<string, string>();

            foreach (var line in File.ReadAllLines(path, Encoding.GetEncoding(1251)))
            {
                var parts = line.Split(splitBy, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    continue;

                result[parts[0]] = parts[1];
            }

            if (result.Count == 0)
                return new Substitutor();

            return new Substitutor(result);
        }

        private readonly Dictionary<string, string> _result;

        private Substitutor(Dictionary<string, string> result)
        {
            _result = result;
        }

        private Substitutor()
            : this(null)
        {
        }

        public string Substitute(string str)
        {
            if (_result == null)
                return str;

            string result;
            if (_result.TryGetValue(str, out result))
                return result;

            return str;
        }
    }
}
