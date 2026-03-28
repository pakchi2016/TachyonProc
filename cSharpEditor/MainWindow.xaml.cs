using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using cSharpEditor.ioControl;
using cSharpEditor.function;


namespace cSharpEditor
{
    public partial class MainWindow : Window
    {
        private int _tabCount = 1;
        private TabUtil _tabUtil;

        public MainWindow()
        {
            InitializeComponent();
            // 起動時に空のタブを一つ用意しておきますわ
            TabUtil.AddNewTab("無題1",EditorTabControl);
        }

        // メニューから「新規タブ」が押された際の処理
        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            _tabCount++;
            TabUtil.AddNewTab($"無題{_tabCount}",EditorTabControl);
        }


        // メニューから「ファイルを開く」が押された際の処理
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            FileControl.OpenCode(EditorTabControl);
        }
        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            FileControl.SaveCode(EditorTabControl, isSaveAs: false);
        }

        private void SaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            FileControl.SaveCode(EditorTabControl, isSaveAs: true);
        }
        private void CheckSyntax_Click(object sender, RoutedEventArgs e)
        {
            if (EditorTabControl.SelectedItem is TabItem selectedTab &&
                selectedTab.Content is TextEditor textEditor)
            {
                string currentCode = textEditor.Text;
                var errors = RoslynUtil.GetAllSyntaxErrors(currentCode).ToList();

                if (errors.Any())
                {
                    // エラーが見つかった場合は一覧を表示します
                    string message = string.Join("\n", errors);
                    MessageBox.Show(message, "構文エラー検知", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    // 何もなければ卿の美しきコーディングを称えますわ
                    MessageBox.Show("構文エラーは見当たりませんわ。美しいコードですこと。", "チェック完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // すべてのタブを順番に確認いたします
            foreach (TabItem tab in EditorTabControl.Items)
            {
                if (tab.Content is TextEditor textEditor && textEditor.IsModified)
                {
                    var result = MessageBox.Show(
                        "未保存のファイルがございます。保存せずに終了してもよろしいですか？",
                        "終了の確認",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true; // 終了処理をキャンセルしてエディタに戻りますわ
                        return;
                    }

                    // 「Yes」が選ばれた場合は他の未保存タブの警告は出さず、そのまま終了させます
                    break;
                }
            }
        }
    }

}