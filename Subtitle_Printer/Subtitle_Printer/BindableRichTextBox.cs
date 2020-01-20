﻿using System;
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
        internal List<bool?> VerticalTabs { get; private set; }

        static BindableRichTextBox()
        {
            EventManager.RegisterClassHandler(typeof(BindableRichTextBox), PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), false);
        }

        public BindableRichTextBox()
        {
            VerticalTabs = new List<bool?> { null };
            this.Document = new FlowDocument(new Paragraph());
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

          if (!control.Selection.IsEmpty &&
                Keyboard.Modifiers != ModifierKeys.Control &&
                (
                    e.Key == Key.Back ||
                    e.Key == Key.Delete ||
                    e.Key == Key.Enter ||
                    e.Key == Key.Space ||
                    ((int)Key.D0 <= (int)e.Key && (int)e.Key <= (int)Key.Z)))
            {
                var startLine = GetLineIndex(control.Selection.Start);
                var endLine = GetLineIndex(control.Selection.End);
                for (int i = endLine - 1; i >= startLine; i--)
                {
                    control.VerticalTabs.RemoveAt(i);
                }
                if (e.Key == Key.Enter) VerticalTabsModifier(control, Key.Enter,Key.None);
                return;
            }


            if (e.Key != Key.Back &&
                e.Key != Key.Delete &&
                e.Key != Key.Return &&
                !(Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V) &&
                !(Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)) return;
            var modifier = Key.None;
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
            VerticalTabsModifier(control, e.Key,modifier);
        }
        #endregion  // イベントハンドラ

        internal static void VerticalTabsModifier(BindableRichTextBox control, Key key,Key modifier)
        {
            var lineIndex = GetLineIndex(control.CaretPosition);
            switch (key)
            {
                case Key.Back:
                    if (!control.CaretPosition.IsAtLineStartPosition) return;
                    if (lineIndex <= 0) return;
                    control.VerticalTabs.RemoveAt(--lineIndex);
                    break;
                case Key.Delete:
                    if (lineIndex == control.VerticalTabs.Count()) return;
                    if (control.CaretPosition.GetOffsetToPosition(control.CaretPosition.DocumentEnd) <= 2) return;
                    control.VerticalTabs.RemoveAt(lineIndex);
                    break;
                case Key.Return:
                    if (modifier != Key.LeftShift || modifier != Key.RightShift)
                    {
                        control.VerticalTabs.Insert(lineIndex, false);
                    }
                    else
                    {
                        control.VerticalTabs.Insert(lineIndex, true);
                    }
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
                                control.VerticalTabs.Insert(lineIndex, false);
                                lineIndex++;
                            }
                            else if (clipboardText[i] == '\v')
                            {
                                control.VerticalTabs.Insert(lineIndex, true);
                                lineIndex++;
                            }
                        }
                    }
                    break;
                case Key.X:
                    var startLine = GetLineIndex(control.Selection.Start);
                    var endLine = GetLineIndex(control.Selection.End);
                    for (int i = endLine -1; i >= startLine; i--)
                    {
                        if (control.VerticalTabs[i] != null) control.VerticalTabs.RemoveAt(i);
                    }
                    break;
            }
        }

        private static int GetLineIndex(TextPointer position)
        {
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
    }
}