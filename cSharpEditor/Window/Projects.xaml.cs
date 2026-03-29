using cSharpEditor.Solution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace cSharpEditor.Projects
{
    /// <summary>
    /// Projects.xaml の相互作用ロジック
    /// </summary>
    public partial class Create : UserControl
    {
        public Create()
        {
            InitializeComponent();
        }
        // クラスのメンバ変数として、読み込んだJSONデータを保持しておきますわ
        private Dictionary<string, SolutionData> _solutionDict;

        // 画面のロード時（Window_Loadedなど）に呼び出してくださいませ
        public void InitializeDropDowns()
        {
            // JSONから全データを読み込みます
            _solutionDict = SolutionManager.GetAllData();

            // ソリューション名（Dictionaryのキー一覧）を1つ目のプルダウンにセットします
            SolutionComboBox.ItemsSource = _solutionDict.Keys;
        }

        // 1つ目のプルダウン（ソリューション）が変更された際の処理です
        private void SolutionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SolutionComboBox.SelectedItem is string selectedSolution)
            {
                // 選択されたソリューション名が存在するか確認します
                if (_solutionDict.TryGetValue(selectedSolution, out var solutionData))
                {
                    // そのソリューションに紐づくプロジェクト名（Projectsのキー一覧）を2つ目にセットします
                    ProjectComboBox.ItemsSource = solutionData.Projects.Keys;

                    // 中身が切り替わったので、2つ目の選択状態をリセットして空にしておきますわ
                    ProjectComboBox.SelectedIndex = -1;
                }
            }
        }
    }
}
