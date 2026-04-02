using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using getCshCode.Utility;
using System.Net.Http.Headers;

namespace getCshCode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "ソリューションファイル (*.slnx)|*.slnx|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true) solutionPath.Text = dialog.FileName;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            var path = solutionPath.Text;
            string code = string.Empty;
            string[] extension = { ".cs", ".xaml" };
            string[] strings = { "obj", "bin" };

            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("ソリューションファイルを選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string solution = Directory.GetParent(path).FullName;
            string solutionName = Path.GetFileNameWithoutExtension(path);
            string outputPath = Path.Combine(solution, "code.txt");
            if (Path.Exists(outputPath)) File.Delete(outputPath);
            using var writer = new StreamWriter(Path.Combine(solution,"code.txt"), false, Encoding.UTF8);
            writer.WriteLine($"ソリューション名: {solutionName}");
            try
            {
                foreach (var line in File.ReadLines(path))
                {
                    if (line.Contains("Project"))
                    {
                        string projectName = ioControl.GetSpan(line, '"', 1).Split('/')[0];
                        writer.WriteLine($"プロジェクト名: {projectName} ");
                        string project = Path.Combine(solution, projectName);
                        foreach (string module in Directory.GetFiles(project))
                        {
                            if (extension.Any(ext => module.EndsWith(ext)))
                            {
                                writer.WriteLine(Path.GetFileName(module));
                                writer.WriteLine("===========================");
                                foreach (var line2 in File.ReadLines(module))
                                {
                                    writer.WriteLine(line2);
                                }
                                writer.WriteLine("===========================");
                            }
                        }
                        foreach(string ProjectFolder in Directory.GetDirectories(project))
                        {
                            if (strings.Any(f => Path.GetDirectoryName(ProjectFolder) == f)) continue;
                            foreach (string module in Directory.GetFiles(ProjectFolder))
                            {
                                if(extension.Any(ext => module.EndsWith(ext)))
                                {
                                    writer.WriteLine(Path.GetFileName(module));
                                    writer.WriteLine("===========================");
                                    foreach (var line2 in File.ReadLines(module))
                                    {
                                        writer.WriteLine(line2);
                                    }
                                    writer.WriteLine("===========================") ;
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.Close();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // 何もせずにウィンドウを閉じる
            this.Close();
        }
    }
}