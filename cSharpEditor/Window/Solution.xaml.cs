using cSharpEditor.function;
using cSharpEditor.ioControl;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace cSharpEditor.Solution
{


    /// <summary>
    /// Create.xaml の相互作用ロジック
    /// </summary>
    public partial class Create : UserControl
    {
        private TextBlock _result;
        public Create(TextBlock result)
        {
            InitializeComponent();
            _result = result;
        }

        public void CreateSolution()
        {
            CreateObjectBlock.Text = "ソリューション作成ウインドウ";
            CreateObjectBox.Text = "作成ソリューション名：";
        }
        public void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string fullPath = SavePathBox.Text;
            string solutionName = CreateObjectText.Text;

            if (string.IsNullOrWhiteSpace(CreateObjectBox.Text) || string.IsNullOrWhiteSpace(SavePathBox.Text))
            {
                MessageBox.Show("ソリューション名と保存先を入力してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
            string solutionPath = Path.Combine(fullPath, solutionName);
            if (!Directory.Exists(solutionPath)) Directory.CreateDirectory(solutionPath);

            StreamWriter sw = new StreamWriter("solutionList", false, Encoding.UTF8);


            string command = $"dotnet new sln -o {solutionPath}";

            Build.CLIClear(_result);
            Build.Create(_result, command);

            Window.GetWindow(this).Close();
        }
        public void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // ウインドウを閉じる
            Window.GetWindow(this).Close();

        }
        public void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "フォルダを開く"
            };

            // ダイアログで「開く」が選択された場合
            if (dialog.ShowDialog() == true)
            {
                SavePathBox.Text = dialog.FolderName;

            }
        }
    }
}
