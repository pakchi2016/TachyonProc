using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PowerTerminal;

namespace PowerTerminal.Command
{
    public class Insert
    {
        MainWindow _window;
        PowerShell _ps;
        Popup CompletionPopup;
        TextBlock OutputTextBlock;
        TextBox InputTextBox;
        ListBox CompletionListBox;


        public Insert(MainWindow window, PowerShell ps)
        {
            _window = window;
            CompletionPopup = _window.CompletionPopup;
            OutputTextBlock = _window.OutputTextBlock;
            InputTextBox = _window.InputTextBox;
            CompletionListBox = _window.CompletionListBox;

            _ps = ps;
        }

        public void Initialize()
        {
            _ps.AddScript("function Set-Connect {}");
            _ps.AddScript("function Set-Reserve {}");
        }

        public void TextChanged()
        {
            if (MainWindow._isInserting) return; // 補完文字の挿入中は何もしない

            if (string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                CompletionPopup.IsOpen = false;
                return;
            }

            // PowerShell SDKから現在の入力に対する補完候補を取得
            CommandCompletion completion = CommandCompletion.CompleteInput(
                InputTextBox.Text,
                InputTextBox.CaretIndex,
                null,
                _ps // クラス直下でインスタンス化して維持しているPowerShellオブジェクト
            );

            if (completion.CompletionMatches.Count > 0)
            {
                // 置換すべき文字の位置と長さを記憶
                MainWindow._replacementIndex = completion.ReplacementIndex;
                MainWindow._replacementLength = completion.ReplacementLength;

                CompletionListBox.Items.Clear();
                foreach (var match in completion.CompletionMatches)
                {
                    CompletionListBox.Items.Add(match.CompletionText);
                }

                CompletionListBox.SelectedIndex = 0;
                CompletionPopup.IsOpen = true;
            }
            else
            {
                CompletionPopup.IsOpen = false;
            }
        }

        public void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (CompletionPopup.IsOpen)
            {
                if (e.Key == Key.Down)
                {
                    CompletionListBox.Focus();
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    CompletionPopup.IsOpen = false;
                    e.Handled = true;
                }
                else if (e.Key == Key.Tab || e.Key == Key.Enter)
                {
                    InsertCompletion();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                // ポップアップが閉じていれば通常のコマンド実行処理へ
                Intorsions.DispatchCommand(_window,_ps);
                e.Handled = true;
            }
        }

        // 選択した補完文字列を入力欄に適用する処理
        public void InsertCompletion()
        {
            if (CompletionListBox.SelectedItem != null)
            {
                MainWindow._isInserting = true;

                string selectedText = CompletionListBox.SelectedItem.ToString();
                string originalText = InputTextBox.Text;

                // 入力途中の文字を選択した補完候補の文字列に置き換える
                InputTextBox.Text = originalText.Remove(MainWindow._replacementIndex, MainWindow._replacementLength)
                                                .Insert(MainWindow._replacementIndex, selectedText);

                InputTextBox.CaretIndex = MainWindow._replacementIndex + selectedText.Length;

                CompletionPopup.IsOpen = false;
                InputTextBox.Focus();

                MainWindow._isInserting = false;
            }
        }
    }
}
