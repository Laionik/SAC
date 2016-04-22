using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SAC
{
    class Program
    {
        /// <summary>
        /// Get sboxFunctions
        /// </summary>
        /// <returns>List of sbox functions</returns>
        /// Console Color DarkYellow
        static List<string> GetSboxFunctions()
        {
            // read all bytes from sbox
            var sboxFile = File.ReadAllBytes("sbox.sbx").Where((x, i) => i % 2 == 0).ToList();
            string[] sboxTab = new string[8];
            for (int i = 0; i < sboxTab.Count(); i++)
            {
                sboxTab[i] = "";
            }

            // create functions
            foreach (var line in sboxFile)
            {
                var binaryLine = Convert.ToString(line, 2).PadLeft(8, '0');
                for (int i = 0; i < binaryLine.Count(); i++)
                {
                    sboxTab[i] += binaryLine[i];
                }
            }
            return sboxTab.ToList();
        }
        
        /// <summary>
        /// Xor two strings
        /// </summary>
        /// <param name="baseString">base string</param>
        /// <param name="newString">string to xor</param>
        /// <returns>xored string</returns>
        static string xorStrings(string baseString, string newString)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < newString.Length; i++)
                sb.Append(int.Parse(newString[i].ToString()) ^ int.Parse(baseString[i].ToString()));
            String result = sb.ToString();

            return result;
        }

        /// <summary>
        /// Splitting a string into chunks of a certain size
        /// </summary>
        /// <param name="toSplit">string</param>
        /// <param name="chunkSize">size of chunks</param>
        /// <returns>List of strings</returns>
        static List<string> Split(string toSplit, int chunkSize)
        {
            return (from Match m in Regex.Matches(toSplit, @"\d{1," + chunkSize + "}") select m.Value).ToList();
        }

        /// <summary>
        /// Creating a function alpha shifted
        /// </summary>
        /// <param name="toConvert">Splitted elements of function</param>
        /// <returns>Function alpha shifted</returns>
        static string CreateFunction(string sboxFunction, int chunkSize)
        {
            var toConvert = Split(sboxFunction, chunkSize);
            StringBuilder sb = new StringBuilder();
            for(int i = 1; i < toConvert.Count; i += 2)
            {
                sb.Append(toConvert[i]);
                sb.Append(toConvert[i - 1]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generate list of function alpha shifted
        /// </summary>
        /// <param name="sboxFunction"></param>
        /// <param name="alphaTab"></param>
        /// <returns></returns>
        static List<string> FunctionTransform(string sboxFunction, int[] alphaTab)
        {
            List<string> functionTransformed = new List<string>();

            foreach(var alpha in alphaTab)
            {
                functionTransformed.Add(CreateFunction(sboxFunction, alpha));
            }

            return functionTransformed;
        }

        /// <summary>
        /// Compare base function to alpha shifted
        /// </summary>
        /// <param name="sboxFunction">Base function</param>
        /// <param name="AlphaShiftedList">List of alpha shifted functions</param>
        /// <returns>Number of diffirences</returns>
        static double CompareFunctions(string sboxFunction, List<string> AlphaShiftedList)
        {
            double functionSac = 0;
            foreach(var alphaShift in AlphaShiftedList)
            {
                functionSac += xorStrings(sboxFunction, alphaShift).Count(x => x == '1') / 256.0;
            }

            return functionSac / 8;
        }


        static void Main(string[] args)
        {
            // SBOX functions
            var sboxList = GetSboxFunctions();
            // SBOX's SAC result
            double sboxSac = 0;

            int[] alphaTab = { 1, 2, 4, 8, 16, 32, 64, 128 };

            foreach(var sboxFunction in sboxList)
            {
                // transform function
                var functionTransformedList = FunctionTransform(sboxFunction, alphaTab);

                // add result of comparing
                sboxSac += CompareFunctions(sboxFunction, functionTransformedList);
            }

            Console.WriteLine("SAC dla SBOXa: {0}", sboxSac / 8);
            Console.ReadKey();
        }
    }
}
