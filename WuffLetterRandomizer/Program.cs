using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WuffLetterRandomizer.XMLClasses;

namespace WuffLetterRandomizer
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Ask("Enter path of file to randomize:");
            if (!File.Exists(path))
            {
                Write("File doesn't exist");
                return;
            }
            string fileString = File.ReadAllText(path);
            XmlStrings file = ReadXmlString(fileString);
            XmlStrings newFile = new XmlStrings()
            {
                Language = file.Language
            };
            Regex regex = new Regex(@"[^\@]\b(\w)+\b");
            Regex number = new Regex(@"\d+");
            foreach (XmlString str in file.Strings)
            {
                XmlString newStr = new XmlString()
                {
                    Isgif = str.Isgif,
                    Key = str.Key
                };
                foreach (string value in str.Values)
                {
                    Dictionary<string, string> replace = new Dictionary<string, string>();
                    foreach (var m in regex.Matches(value.Replace("\\n", "\n")))
                    {
                        string match = m.ToString();
                        if (number.IsMatch(match) || match.Length < 3)
                        {
                            continue;
                        }
                        Random rnd = new Random();
                        string first = match.Substring(0, 1);
                        string last = match.Substring(match.Length - 1);
                        string proc = match.Substring(1, match.Length - 2);
                        string output = first;
                        char[] chars = new char[proc.Length];
                        var randomNumbers = Enumerable.Range(0, proc.Length).OrderBy(x => rnd.Next()).Take(proc.Length).ToList();
                        for(int i=0; i < proc.Length; i++)
                        {
                            chars[i] = proc[randomNumbers[i]];
                        }
                        foreach (var c in chars)
                        {
                            output += c;
                        }
                        output += last;
                        if (!replace.ContainsKey(match)) replace.Add(match, output);
                    }
                    string newValue = value.Replace("\\n", "\n");
                    foreach (var kvp in replace)
                    {
                        newValue = newValue.Replace(kvp.Key, kvp.Value);
                    }
                    newStr.Values.Add(newValue.Replace("\n", "\\n"));
                }
                newFile.Strings.Add(newStr);
            }
            string finalString = SerializeXmlToString(newFile);
            Write(finalString);
            string toPath = Ask("Path to write file to:");
            File.WriteAllText(toPath, finalString);
            Write("Done.");
            Ask("");
        }

        private static void Write(string text)
        {
            Console.WriteLine(text);
        }

        private static string  Ask(string question)
        {
            Console.WriteLine(question);
            return Console.ReadLine();
        }

        #region Read xml string
        private static XmlStrings ReadXmlString(string fileString)
        {
            XmlStrings result;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(XmlStrings));
                using (TextReader tr = new StringReader(fileString))
                {
                    result = (XmlStrings)serializer.Deserialize(tr);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }
        #endregion
        #region Serialize xml to string
        private static string SerializeXmlToString(XmlStrings xmls)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XmlStrings));
            using (TextWriter tw = new StringWriter())
            {
                serializer.Serialize(tw, xmls);
                string result = tw.ToString();
                //result = Utf16ToUtf8(result);
                return result.Replace("utf-16", "utf-8");
            }
        }
        #endregion
    }
}
