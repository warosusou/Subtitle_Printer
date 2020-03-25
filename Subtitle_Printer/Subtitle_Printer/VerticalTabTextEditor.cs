using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

        public string SaveVerticalTabText(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            var lines = this.Text.Split(Environment.NewLine.ToCharArray());
            for (int i = 0; i < lines.Count(); i++)
            {
                if (this.VerticalTabs.ElementAt(i))
                    lines[i].Append('\v');
                sb.AppendLine(lines[i]);
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.Filter = "テキスト ファイル(.txt)|*.txt";
            saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
            saveFileDialog.FileName = fileName;
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                using (Stream fileStream = saveFileDialog.OpenFile())
                using (StreamWriter sr = new StreamWriter(fileStream))
                {
                    sr.Write(sb.ToString());
                }
                return saveFileDialog.SafeFileName;
            }
            return "";
        }

        public string LoadVerticalTabText()
        {
            string loadedText = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FilterIndex = 1;
            openFileDialog.Filter = "テキスト ファイル(.txt)|*.txt";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                lineTracker.LineChanged -= LineTracker_LineChanged;
                using (Stream fileStream = openFileDialog.OpenFile())
                {
                    StreamReader sr = new StreamReader(fileStream, true);
                    loadedText = sr.ReadToEnd();
                }
                var lines = loadedText.Split(Environment.NewLine.ToCharArray());
                foreach (var line in lines)
                {
                    if (line.Contains('\v'))
                    {
                        line.Replace("\v", "");
                        this.verticalTabs.Insert(0, true);
                    }
                    else
                    {
                        this.verticalTabs.Insert(0, false);
                    }
                    this.Text.Insert(0, String.Format("{0}{1}", line, Environment.NewLine));
                }
                lineTracker.LineChanged += LineTracker_LineChanged;
                return openFileDialog.FileName;
            }
            return "";
        }
    }
}