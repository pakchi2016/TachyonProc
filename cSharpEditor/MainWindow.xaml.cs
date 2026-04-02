using cSharpEditor.function;
using cSharpEditor.ioControl;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using cSharpEditor.Solution;
using System.Reflection;
using System.Linq;

namespace cSharpEditor
{
    // ツリーに表示する要素のデータクラスですわ
    public class TreeItem
    {
        public string Name { get; set; } = "";
        public string FullPath { get; set; } = "";
        public bool IsFile { get; set; } = false;

        // フォルダの場合は、この中に子要素が入ります
        public System.Collections.ObjectModel.ObservableCollection<TreeItem> Children { get; set; } = new();
    }

    public partial class MainWindow : Window
    {
        private int _tabCount = 1;
        private readonly TabUtil? _tabUtil;

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

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // CtrlキーとSキーが同時に押されたかを判定します
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // すでに実装済みの保存処理を呼び出しますわ
                SaveFile_Click(null, null);

                // イベントが処理されたことをシステムに伝え、他の動作を止めます
                e.Handled = true;
            }
        }
        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TabItem tabItem)
            {
                CloseSpecifiedTab(tabItem);
            }
        }

        private void EditorTabControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.OriginalSource is DependencyObject obj)
            {
                // クリックされた位置にある TabItem を特定します
                var tabItem = FindAncestor<TabItem>(obj);
                if (tabItem != null)
                {
                    CloseSpecifiedTab(tabItem);
                }
            }
        }

        private void CloseSpecifiedTab(TabItem tabItem)
        {
            if (tabItem.Content is ICSharpCode.AvalonEdit.TextEditor textEditor && textEditor.IsModified)
            {
                var result = MessageBox.Show(
                    $"{tabItem.Header} は変更されています。保存せずに閉じますか？",
                    "確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No) return;
            }

            EditorTabControl.Items.Remove(tabItem);
        }

        // 親要素を辿るためのヘルパーメソッドですわ
        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T ancestor) return ancestor;
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
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

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        { 
            SolutionTree.OpenFolder(SolutionTreeView);
        }

        private void SolutionTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SolutionTreeView.SelectedItem is TreeItem selectedItem && selectedItem.IsFile)
            {
                // 既に同じファイルが開かれていないかチェックし、開かれていればそのタブをアクティブにしますわ
                foreach (TabItem tab in EditorTabControl.Items)
                {
                    // ※実際にはTabItemのTagプロパティなどにパスを保存して比較するのが正確ですが、
                    // 今回は簡易的に「タイトルの一部」で判定する例です
                    if (tab.Header.ToString().Contains(selectedItem.Name))
                    {
                        EditorTabControl.SelectedItem = tab;
                        return;
                    }
                }

                // 選択されたファイルを読み込み、新しいタブとして開きます
                string content = System.IO.File.ReadAllText(selectedItem.FullPath);
                // ※ TabUtil は卿の現在の実装に合わせて呼び出してくださいませ
                TabUtil.OpenNewFileTab(selectedItem.Name, content, selectedItem.FullPath, EditorTabControl);
            }
        }
        private void SolutionButton_Click(object sender, RoutedEventArgs e)
        {
            var createControl = new Create(ResultTextBlock);
            createControl.CreateSolution();

            Window hostWindow = TerminalWindow("ソリューション作成", createControl);

            hostWindow.ShowDialog();
        }
        private void ProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var createControl = new Create(ResultTextBlock);
            createControl.CreateSolution();

            Window hostWindow = TerminalWindow("プロジェクト作成", createControl);

            hostWindow.ShowDialog();
        }
        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            Build.Create(ResultTextBlock);
        }

        private Window TerminalWindow(string title, Create control)
        {
            return new Window
            {
                Title = title,
                Content = control, // ← ここで部品をはめ込みます
                SizeToContent = SizeToContent.WidthAndHeight, // 中身に合わせて自動でサイズ調整させます
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
        }
    }

}