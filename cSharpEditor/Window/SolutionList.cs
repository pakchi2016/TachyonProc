using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace cSharpEditor.Solution
{
    // --- 1. JSONの構造を表現するデータクラスですわ ---
    public class SolutionData
    {
        public string FullPath { get; set; }

        // プロジェクト名をキー、パスを値として子要素に保持します
        public Dictionary<string, string> Projects { get; set; } = new Dictionary<string, string>();
    }

    // --- 2. 記録・更新を担う管理クラスですわ ---
    public class SolutionManager
    {
        private static readonly string JsonFilePath = "solutionList.json";

        // 内部で使用する読み込み用の共通メソッドです
        private static Dictionary<string, SolutionData> LoadJson()
        {
            if (!File.Exists(JsonFilePath)) return new Dictionary<string, SolutionData>();
            string json = File.ReadAllText(JsonFilePath);
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, SolutionData>();

            return JsonSerializer.Deserialize<Dictionary<string, SolutionData>>(json) ?? new Dictionary<string, SolutionData>();
        }

        // 内部で使用する保存用の共通メソッドです
        private static void SaveJson(Dictionary<string, SolutionData> data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(JsonFilePath, json);
        }

        // 【呼び出し用】ソリューションを作成した際に実行してくださいませ
        public static void AddSolution(string solutionName, string solutionPath)
        {
            var data = LoadJson();
            if (!data.ContainsKey(solutionName))
            {
                data[solutionName] = new SolutionData { FullPath = solutionPath };
                SaveJson(data);
            }
        }

        // 【呼び出し用】プロジェクトを作成した際に実行してくださいませ
        public static void AddProject(string targetSolutionName, string projectName, string projectPath)
        {
            var data = LoadJson();
            if (data.ContainsKey(targetSolutionName))
            {
                data[targetSolutionName].Projects[projectName] = projectPath;
                SaveJson(data);
            }
            else
            {
                System.Windows.MessageBox.Show("指定されたソリューションが存在しませんわ。");
            }
        }
        public static Dictionary<string, SolutionData> GetAllData()
        {
            return LoadJson();
        }

    }
}
