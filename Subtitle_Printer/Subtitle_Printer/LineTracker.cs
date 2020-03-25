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
            if (this.LineChanged != null)
                OnLineChanged(new LineChangedEventArgs(true, line.LineNumber));
        }

        public void ChangeComplete(DocumentChangeEventArgs e)
        {
            if (!noticed && (e.InsertedText.Text == Environment.NewLine || e.RemovedText.Text == Environment.NewLine))
            {
                if (e.InsertionLength != 0)
                    OnLineChanged(new LineChangedEventArgs(false, 2));
                else
                    OnLineChanged(new LineChangedEventArgs(true, 2));
            }
            noticed = false;
        }

        public void LineInserted(DocumentLine insertionPos, DocumentLine newLine)
        {
            if (this.LineChanged != null)
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

        public LineChangedEventArgs(bool deleted, int lineNumber)
        {
            Deleted = deleted;
            LineNumber = lineNumber;
        }
    }
}
