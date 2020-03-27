using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitle_Printer
{
    class LineTracker : ILineTracker
    {
        public event EventHandler<LineChangedEventArgs> LineChanged;
        private bool noticed = false;

        public void BeforeRemoveLine(DocumentLine line)
        {
            OnLineChanged(new LineChangedEventArgs(true, line.LineNumber));
        }

        public void ChangeComplete(DocumentChangeEventArgs e)
        {
            if (!noticed && (e.InsertedText.Text.Contains(Environment.NewLine) || e.RemovedText.Text.Contains(Environment.NewLine)))
            {
                if (e.InsertionLength != 0)
                {
                    List<string> line = e.InsertedText.Text.Replace("\n", "").Split('\r').ToList();
                    for (int i = 1; i < line.Count; i++)
                    {
                        if (line[i].Contains("\v"))
                            OnLineChanged(new LineChangedEventArgs(false, i + 1, true));
                        else
                            OnLineChanged(new LineChangedEventArgs(false, i + 1));
                    }
                }
                else
                {
                    OnLineChanged(new LineChangedEventArgs(true, 2));
                }
            }
            noticed = false;
        }

        public void LineInserted(DocumentLine insertionPos, DocumentLine newLine)
        {
            OnLineChanged(new LineChangedEventArgs(false, newLine.LineNumber));
        }

        public void RebuildDocument()
        {
        }

        public void SetLineLength(DocumentLine line, int newTotalLength)
        {
        }

        private void OnLineChanged(LineChangedEventArgs e)
        {
            noticed = true;
            this.LineChanged?.Invoke(this, e);
        }
    }

    class LineChangedEventArgs : EventArgs
    {
        public bool Deleted { get; }
        public int LineNumber { get; }
        public bool ContainVertical { get; } = false;

        public LineChangedEventArgs(bool deleted, int lineNumber)
        {
            Deleted = deleted;
            LineNumber = lineNumber;
        }

        public LineChangedEventArgs(bool deleted, int lineNumber, bool containVertical)
        {
            Deleted = deleted;
            LineNumber = lineNumber;
            ContainVertical = containVertical;
        }
    }
}
