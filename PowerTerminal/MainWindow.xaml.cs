using PowerTerminal.Command;
using System;
using System.Management.Automation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PowerTerminal
{
    public partial class MainWindow : Window
    {
        private Insert _insert;
        private Intorsions _intorsions;

        private PowerShell _ps;
        public static int _replacementIndex;
        public static int _replacementLength;
        public static bool _isInserting = false;
        public MainWindow()
        {
            InitializeComponent();
            _ps = PowerShell.Create();
            _insert = new Insert(this,_ps);
            _intorsions = new Intorsions(_ps);
        }

        // 実行ボタンが押された時の処理
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            Intorsions.DispatchCommand(this,_ps);
        }

        // テキストボックスでEnterキーが押された時の処理（利便性のため）
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Intorsions.DispatchCommand(this,_ps);
            }
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _insert.TextChanged();
        }

        // 入力欄でのキー操作（ポップアップ表示時の上下キー移動やTab/Enterでの決定）
        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _insert.PreviewKeyDown(sender, e);
        }

        // リストボックス側でのキー操作
        private void CompletionListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                _insert.InsertCompletion();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CompletionPopup.IsOpen = false;
                InputTextBox.Focus();
                e.Handled = true;
            }
        }

        // リストボックスのダブルクリック操作
        private void CompletionListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _insert.InsertCompletion();
        }
    }
}