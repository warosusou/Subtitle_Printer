using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace Subtitle_Printer
{
    public class MainView
    {
        public string Text { get; set; } = "Sample";
        public FlowDocument Document { get; set; } = new FlowDocument(new Paragraph(new Run("test")));
        public Brush MathBrushFore { get; } = Brushes.White;
        public Brush MathBrushBack { get; } = Brushes.Blue;
        public char MathSymbol { get; } = '@';
        public char MathSymbolAlias { get; } = '＠';
    }
}