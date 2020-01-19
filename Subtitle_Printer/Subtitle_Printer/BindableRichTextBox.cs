using System;
using System.Collections.Generic;
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
        private List<bool?> IsLF;

        static BindableRichTextBox()
        {
            EventManager.RegisterClassHandler(typeof(BindableRichTextBox), PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), false);
        }

        public BindableRichTextBox()
        {
            IsLF = new List<bool?> { null };
            this.Document = new FlowDocument(new Paragraph(/*new Run()*/));
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

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var control = sender as BindableRichTextBox;
            if (e.Key != Key.Back && e.Key != Key.Delete && e.Key != Key.Return) return;

            //var paragraphIndex = control.Document.Blocks.ToList().IndexOf(control.CaretPosition.Paragraph);
            var lineIndex = GetLineIndex(control.CaretPosition);
            switch (e.Key)
            {
                case Key.Back:
                    if (!control.CaretPosition.IsAtLineStartPosition) return;
                    if (lineIndex <= 0) return;
                    control.IsLF.RemoveAt(--lineIndex);
                    break;
                case Key.Delete:
                    if (lineIndex == control.IsLF.Count()) return;
                    if (control.CaretPosition.GetOffsetToPosition(control.CaretPosition.DocumentEnd) <= 2) return;
                    control.IsLF.RemoveAt(lineIndex);
                    break;
                case Key.Return:
                    if (Keyboard.Modifiers != ModifierKeys.Shift)
                    {
                        control.IsLF.Insert(lineIndex, false);
                    }
                    else
                    {
                        control.IsLF.Insert(lineIndex, true);
                    }
                    break;
            }
        }
        #endregion  // イベントハンドラ

        private static int GetLineIndex(TextPointer position)
        {
            int lineIndex = 0;

            var rangeFromStartToPosition = new TextRange(position.DocumentStart,position);
            bool linefeed = false;
            for(int i = 0;i < rangeFromStartToPosition.Text.Length; i++)
            {
                if (linefeed && rangeFromStartToPosition.Text[i] == '\n') lineIndex++;
                else if (rangeFromStartToPosition.Text[i] == '\r') linefeed = true;
                else linefeed = false;
            }
            return lineIndex;
        }
    }
}
