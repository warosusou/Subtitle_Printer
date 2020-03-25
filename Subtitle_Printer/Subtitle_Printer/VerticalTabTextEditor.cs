using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Subtitle_Printer
{
    class VerticalTabTextEditor : TextEditor
    {
        public IReadOnlyCollection<bool> VerticalTabs { get { return verticalTabs; } }
        private List<bool> verticalTabs = new List<bool> { false };
        private LineTracker lineTracker = new LineTracker();
        private bool shiftPressed = false;

        public VerticalTabTextEditor()
        {
            EventManager.RegisterClassHandler(typeof(VerticalTabTextEditor), PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), false);
            this.Document.LineTrackers.Add(lineTracker);
            lineTracker.LineChanged += LineTracker_LineChanged;
        }

        private void LineTracker_LineChanged(object sender, LineChangedEventArgs e)
        {
            if (e.Deleted)
            {
                verticalTabs.RemoveAt(e.LineNumber - 2);
            }
            else
            {
                if (shiftPressed)
                    verticalTabs.Insert(e.LineNumber - 2, true);
                else
                    verticalTabs.Insert(e.LineNumber - 2, false);
            }
            this.shiftPressed = false;
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is VerticalTabTextEditor control)) return;
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
                control.shiftPressed = true;
        }
    }
}
