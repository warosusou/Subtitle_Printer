using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitle_Printer
{
    static class LineNumberCalc
    {
        public static IEnumerable<string> Calc(int start, int end, IEnumerable<bool> VerticalTabs)
        {
            int alphaNum = -1;
            int lineNumber = start;
            for (int lineIndex = start - 1; lineIndex < end; lineIndex++)
            {
                if (lineIndex == start - 1)
                {
                    var verticalTabsBlock = VerticalTabs.Take(lineNumber).ToList();
                    verticalTabsBlock.RemoveAt(verticalTabsBlock.Count() - 1);
                    lineNumber -= verticalTabsBlock.Where(x => x == true).Count();
                    if (lineIndex >= 1 && VerticalTabs.ElementAt(lineIndex - 1))
                    {
                        verticalTabsBlock.Reverse();
                        alphaNum = verticalTabsBlock.TakeWhile(x => x == true).Count();
                    }
                    else if (VerticalTabs.ElementAt(lineIndex))
                    {
                        alphaNum = 0;
                    }
                }
                else
                {
                    if (VerticalTabs.ElementAt(lineIndex - 1))
                        alphaNum++;
                    else if (VerticalTabs.ElementAt(lineIndex))
                    {
                        alphaNum = 0;
                        lineNumber++;
                    }
                    else
                    {
                        alphaNum = -1;
                        lineNumber++;
                    }
                }
                string format = "{0}-{1}";
                if (alphaNum < 0)
                    format = "{0}";
                yield return String.Format(format, lineNumber.ToString(CultureInfo.CurrentCulture), AlphabetCalc(alphaNum));
            }
        }

        public static string AlphabetCalc(int num)
        {
            if (num == -1) return "";
            string result = "";
            //A == 0, Z == 25
            while (true)
            {
                char c = (char)((num % 26) + 'A');
                result = result.Insert(0, c.ToString(CultureInfo.CurrentCulture));
                num = num / 26 - 1;
                if (num == -1) break;
            }
            return result;
        }
    }
}
