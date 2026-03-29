using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace cSharpEditor.function
{
    public class SolutionTree
    {
        // --- MainWindow.xaml.cs のクラス内に以下を追加いたしますわ ---

        public static void OpenFolder(TreeView solutionTreeView)
        {
            // フォルダ選択ダイアログを呼び出します
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "ソリューションやプロジェクトのフォルダを選択してくださいませ"
            };

            if (dialog.ShowDialog() == true)
            {
                string folderPath = dialog.FolderName;

                // 選択されたフォルダを起点にツリーを構築いたします
                var rootNode = BuildTree(folderPath);

                // 構築した階層データをUIのツリービューに流し込みますわ
                solutionTreeView.ItemsSource = new System.Collections.ObjectModel.ObservableCollection<TreeItem> { rootNode };
            }
        }

        // フォルダの階層を潜りながらデータを組み立てる再帰関数です
        private static TreeItem BuildTree(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            var item = new TreeItem
            {
                Name = dirInfo.Name,
                FullPath = dirInfo.FullName,
                IsFile = false
            };

            // 1. サブディレクトリの探索
            foreach (var dir in dirInfo.GetDirectories())
            {
                // obj、bin、および .git などの隠しフォルダは探索対象から除外いたします
                if (dir.Name.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                    dir.Name.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
                    dir.Name.StartsWith("."))
                {
                    continue;
                }

                var subNode = BuildTree(dir.FullName);

                // 中身（対象ファイルやさらに下のフォルダ）が存在する場合のみ追加いたしますわ
                if (subNode.Children.Count > 0)
                {
                    item.Children.Add(subNode);
                }
            }

            // 2. ファイルの探索と抽出
            string[] allowedExtensions = { ".cs", ".xaml" };
            foreach (var file in dirInfo.GetFiles())
            {
                if (allowedExtensions.Contains(file.Extension.ToLower()))
                {
                    item.Children.Add(new TreeItem
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        IsFile = true
                    });
                }
            }

            return item;
        }
    }
}
