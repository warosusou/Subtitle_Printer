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
                if (shiftPressed || e.ContainVertical)
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
            var lines = this.Text.Replace("\n", "").Split(Environment.NewLine.ToCharArray());
            for (int i = 0; i < lines.Count(); i++)
            {
                if (this.VerticalTabs.ElementAt(i))
                    lines[i]  +="\v";
                sb.AppendLine(lines[i]);
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FilterIndex = 1,
                Filter = "テキスト ファイル(.txt)|*.txt",
                InitialDirectory = Environment.CurrentDirectory,
                FileName = fileName
            };
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                FilterIndex = 1,
                Filter = "テキスト ファイル(.txt)|*.txt",
                InitialDirectory = Environment.CurrentDirectory
            };
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                lineTracker.LineChanged -= LineTracker_LineChanged;
                using (Stream fileStream = openFileDialog.OpenFile())
                {
                    StreamReader sr = new StreamReader(fileStream, true);
                    loadedText = sr.ReadToEnd();
                }
                var lines = loadedText.Replace("\n", "").Split(Environment.NewLine.ToCharArray()).Reverse().ToList();
                if (lines.First() == String.Empty) 
                    lines.RemoveAt(0);
                this.Text = "";
                foreach (var line in lines)
                {
                    var text = line;
                    if (line.Contains('\v'))
                    {
                        text = line.Replace("\v", "");
                        this.verticalTabs.Insert(0, true);
                    }
                    else
                    {
                        text = line;
                        this.verticalTabs.Insert(0, false);
                    }
                    this.Text = this.Text.Insert(0, String.Format("{0}{1}", text, Environment.NewLine));
                }
                if (this.Text.EndsWith(Environment.NewLine)) 
                    this.Text = this.Text.Substring(0, this.Text.Length - Environment.NewLine.Length);
                this.UpdateLayout();
                lineTracker.LineChanged += LineTracker_LineChanged;
                return openFileDialog.FileName;
            }
            return "";
        }
    }
}