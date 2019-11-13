using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitle_Printer
{
    public class MainView
    {
        public string Text { get; set; } = "Sample";

        public int TextInsertCRLF(int pos)
        {
            const string crlf = "\r\n";
            Text.Insert(pos, crlf);
            return (pos + crlf.Length);
        }

        public int TextInsert(int pos, string text)
        {
            Text.Insert(pos, text);
            return (pos + text.Length);
        }
    }
}
