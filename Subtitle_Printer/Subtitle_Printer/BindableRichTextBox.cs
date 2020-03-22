using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Subtitle_Printer
{
    public class BindableRichTextBox : RichTextBox
    {
        public event EventHandler<VerticalTabsChangedEventArgs> VerticalTabsChanged;
        internal IReadOnlyList<bool?> VerticalTabs { get; private set; }

        static BindableRichTextBox()
        {
            EventManager.RegisterClassHandler(typeof(BindableRichTextBox), KeyDownEvent, new KeyEventHandler(OnKeyDown), false);
            EventManager.RegisterClassHandler(typeof(BindableRichTextBox), MouseUpEvent, new MouseButtonEventHandler(OnMouseUp), false);
        }

        public BindableRichTextBox()
        {
            VerticalTabs = new List<bool?> { null };
            LineIndex = 0;
            LineCount = 1;
        }

        #region 依存関係プロパティ
        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register("Document", typeof(FlowDocument), typeof(BindableRichTextBox), new UIPropertyMetadata(null, OnRichTextItemsChanged));
        #endregion  // 依存関係プロパティ

        #region 公開プロパティ
        public new FlowDocument Document
        {
            get { return (FlowDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public int LineIndex { get; private set; }
        public int LineCount { get; private set; }
        #endregion  // 公開プロパティ

        #region イベントハンドラ
        public static void OnRichTextItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as RichTextBox;
            if (control != null && e.NewValue != null)
            {
                control.Document = e.NewValue as FlowDocument;
            }
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            var control = sender as BindableRichTextBox;

            if (!control.Selection.IsEmpty &&
                  Keyboard.Modifiers != ModifierKeys.Control &&
                  (
                      e.Key == Key.Back ||
                      e.Key == Key.Delete ||
                      e.Key == Key.Enter ||
                      e.Key == Key.Space ||
                      ((int)Key.D0 <= (int)e.Key && (int)e.Key <= (int)Key.Z)))
            {
                if (control.Selection.Text != "\r\n")
                {
                    var startLine = RichTextBoxUtil.GetLineIndex(control.Selection.Start);
                    var endLine = RichTextBoxUtil.GetLineIndex(control.Selection.End);
                    var verticalTabs = control.VerticalTabs.ToList();
                    for (int i = endLine - 1; i >= startLine; i--)
                    {
                        verticalTabs.RemoveAt(i);
                    }
                    control.VerticalTabs = verticalTabs;
                    if (e.Key == Key.Enter) VerticalTabsModifier(control, Key.Enter, RichTextBoxUtil.ConvertKeyModifierToKey());
                    return;
                }
                else
                {
                    control.Selection.Text = "";
                    control.Selection.Select(control.CaretPosition.GetPositionAtOffset(-2), control.CaretPosition.GetPositionAtOffset(-2));
                    if (e.Key == Key.Enter) VerticalTabsModifier(control, Key.Enter, RichTextBoxUtil.ConvertKeyModifierToKey());
                    control.Selection.Select(control.CaretPosition.GetPositionAtOffset(2), control.CaretPosition.GetPositionAtOffset(2));
                }
                return;
            }

            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
                ArrowKeyLineDetector(control, e.Key);


            if (e.Key == Key.Back ||
                e.Key == Key.Delete ||
                e.Key == Key.Return ||
                (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V) ||
                (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X))
                VerticalTabsModifier(control, e.Key, RichTextBoxUtil.ConvertKeyModifierToKey());
            /*if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
                e.Handled = true;
        */}

        private static void OnMouseUp(object sender, MouseEventArgs e)
        {
            var control = sender as BindableRichTextBox;
            var i = RichTextBoxUtil.GetLineIndex(control.CaretPosition);
            if (i != -1)
                control.LineIndex = i;
        }

        private void OnVerticalTabsChanged(VerticalTabsChangedEventArgs e)
        {
            VerticalTabsChanged?.Invoke(this, e);
        }
        #endregion  // イベントハンドラ

        internal static void ArrowKeyLineDetector(BindableRichTextBox control, Key key)
        {
            switch (key)
            {
                case Key.Up:
                    if (control.LineIndex != 0)
                        control.LineIndex--;
                    break;
                case Key.Down:
                    if (control.LineIndex != control.VerticalTabs.Count() - 1)
                        control.LineIndex++;
                    break;
                case Key.Left:
                    if (control.LineIndex != 0 && control.CaretPosition.IsAtLineStartPosition)
                        control.LineIndex--;
                    break;
                case Key.Right:
                    if (control.LineIndex != control.VerticalTabs.Count() - 1 && control.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward).IsAtLineStartPosition)
                        control.LineIndex++;
                    break;
            }
        }

        internal static void VerticalTabsModifier(BindableRichTextBox control, Key key, Key modifier)
        {
            var verticalTabs = control.VerticalTabs.ToList();
            bool removed = true;
            var changedIndexes = new List<int>();
            switch (key)
            {
                case Key.Back:
                    if (!control.CaretPosition.IsAtLineStartPosition) return;
                    if (control.LineIndex <= 0) return;
                    control.LineIndex--;
                    verticalTabs.RemoveAt(control.LineIndex);

                    removed = true;
                    changedIndexes.Add(control.LineIndex);
                    control.LineCount--;
                    break;
                case Key.Delete:
                    if (control.CaretPosition.GetOffsetToPosition(control.CaretPosition.DocumentEnd) <= 2) return;
                    if (!control.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward).IsAtLineStartPosition) return;
                    if (control.LineIndex == verticalTabs.Count() - 1) return;
                    verticalTabs.RemoveAt(control.LineIndex);

                    removed = true;
                    changedIndexes.Add(control.LineIndex);
                    control.LineCount--;
                    break;
                case Key.Return:
                    if (modifier != Key.LeftShift && modifier != Key.RightShift)
                    {
                        verticalTabs.Insert(control.LineIndex, false);
                        control.LineIndex++;
                        control.LineCount++;
                    }
                    else
                    {
                        /*control.Document.Blocks.InsertAfter(control.CaretPosition.Paragraph, new Paragraph(new Run("")));
                        control.CaretPosition = control.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward);*/
                        verticalTabs.Insert(control.LineIndex, true);
                        control.LineIndex++;
                        control.LineCount++;
                    }
                    removed = false;
                    changedIndexes.Add(control.LineIndex);
                    break;
                case Key.V:
                    IDataObject clipboardData = Clipboard.GetDataObject();
                    if (clipboardData.GetDataPresent(DataFormats.Text))
                    {
                        string clipboardText = (string)clipboardData.GetData(DataFormats.Text);
                        for (int i = 0; i < clipboardText.Length; i++)
                        {
                            if (clipboardText[i] == '\n')
                            {
                                verticalTabs.Insert(control.LineIndex, false);
                                control.LineIndex++;

                                removed = false;
                                changedIndexes.Add(control.LineIndex);
                                control.LineCount++;
                            }
                            else if (clipboardText[i] == '\v')
                            {
                                verticalTabs.Insert(control.LineIndex, true);
                                control.LineIndex++;

                                removed = false;
                                changedIndexes.Add(control.LineIndex);
                                control.LineCount++;
                            }
                        }
                    }
                    break;
                case Key.X:
                    var startLine = RichTextBoxUtil.GetLineIndex(control.Selection.Start);
                    var endLine = RichTextBoxUtil.GetLineIndex(control.Selection.End);
                    for (int i = endLine - 1; i >= startLine; i--)
                    {
                        if (verticalTabs[i] != null) verticalTabs.RemoveAt(i);

                        removed = true;
                        changedIndexes.Add(i);
                        control.LineCount--;
                    }
                    control.LineIndex = startLine;
                    break;
            }
            control.VerticalTabs = verticalTabs;
            if (changedIndexes.Count != 0)
                control.OnVerticalTabsChanged(new VerticalTabsChangedEventArgs(removed, changedIndexes));
        }
    }

    public class VerticalTabsChangedEventArgs : EventArgs
    {
        public bool Removed { get; }
        public IReadOnlyCollection<int> Indexes { get; }

        public VerticalTabsChangedEventArgs(bool removed, IEnumerable<int> indexes)
        {
            this.Removed = removed;
            this.Indexes = indexes.ToArray();
        }
    }

    public static class RichTextBoxUtil
    {
        public static int GetLineIndex(TextPointer position)
        {
            if (position == null) return -1;
            int lineIndex = 0;

            var rangeFromStartToPosition = new TextRange(position.DocumentStart, position);
            bool linefeed = false;
            for (int i = 0; i < rangeFromStartToPosition.Text.Length; i++)
            {
                if (linefeed && rangeFromStartToPosition.Text[i] == '\n') lineIndex++;
                else if (rangeFromStartToPosition.Text[i] == '\r') linefeed = true;
                else linefeed = false;
            }
            return lineIndex;
        }

        public static int GetLineIndex(BindableRichTextBox control, TextPointer pointer)
        {
            if (control == null || pointer == null) return -1;
            int result = 0;
            foreach (Paragraph p in control.Document.Blocks)
            {
                if (p.Inlines.Contains(pointer.Parent))
                {
                    result += p.Inlines.ToList().IndexOf(pointer.Parent as Inline) - 1;
                    break;
                }
                else
                    result += (p.Inlines.Count() / 2 + 1);
            }
            return result;
        }

        public static TextPointer GetPointerByCharCount(TextPointer reference, int stringLength)
        {
            LogicalDirection direction;
            var text = new TextRange(reference.DocumentStart, reference.DocumentEnd).Text;
            if (stringLength > 0) direction = LogicalDirection.Forward;
            else direction = LogicalDirection.Backward;
            for (int i = 0; i < Math.Abs(stringLength); i++)
            {
                var t = reference.GetNextInsertionPosition(direction);
                if (t == null) return null;
                reference = t;
            }
            return reference;
        }

        public static Key ConvertKeyModifierToKey()
        {
            Key modifier = Key.None;
            switch (Keyboard.Modifiers)
            {
                case ModifierKeys.Alt:
                    modifier = Key.LeftAlt;
                    break;
                case ModifierKeys.Shift:
                    modifier = Key.LeftShift;
                    break;
                case ModifierKeys.Control:
                    modifier = Key.LeftCtrl;
                    break;
                case ModifierKeys.Windows:
                    modifier = Key.LWin;
                    break;
            }
            return modifier;
        }
    }
}