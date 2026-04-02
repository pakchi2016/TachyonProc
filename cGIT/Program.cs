using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace LocalVersionControl
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. 引数によるモード判定（デフォルトは "commit"）
            string mode = args.Length > 0 ? args[0].ToLower() : "commit";

            // 2. ヘルプ表示の判定（設定ファイルの有無に関わらず最優先で処理）
            if (mode == "-h")
            {
                ShowHelp();
                return; // ヘルプ表示後はアプリを終了
            }

            // 3. 設定ファイルからパスを取得
            string targetDirectory = GetTargetDirectoryFromIni();

            if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
            {
                Console.WriteLine("settings.ini に有効なディレクトリパスが設定されておりませんわ。");
                return;
            }

            try
            {
                if (!Repository.IsValid(targetDirectory))
                {
                    Repository.Init(targetDirectory);
                    Console.WriteLine("リポジトリを初期化いたしました。");
                }

                using (var repo = new Repository(targetDirectory))
                {
                    switch (mode)
                    {
                        case "log":
                            ShowHistory(repo);
                            break;

                        case "rollback":
                            if (args.Length < 2)
                            {
                                Console.WriteLine("ロールバック先のコミットIDを指定してくださいませ。");
                                return;
                            }
                            Rollback(repo, args[1]);
                            break;

                        case "commit":
                        default:
                            PerformCommit(repo);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"予期せぬエラーが発生いたしました: {ex.Message}");
            }
        }

        // --- 追加: ヘルプ表示機能 ---
        static void ShowHelp()
        {
            Console.WriteLine("=== LocalVersionControl 利用方法 ===");
            Console.WriteLine("設定ファイル (settings.ini) に管理対象の TargetPath を記述して実行します。\n");
            Console.WriteLine("[引数と機能]");
            Console.WriteLine("  (引数なし)    : ディレクトリ内の変更を検知し、現在の状態を記録（コミット）いたします。");
            Console.WriteLine("  log           : 過去の変更履歴（コミットIDと日時）を新しい順に表示いたします。");
            Console.WriteLine("  rollback <ID> : 指定したコミットIDの状態へ、ディレクトリ内のファイルを強制的に復元いたします。");
            Console.WriteLine("  -h            : このヘルプメッセージを表示いたします。");
        }

        // --- 既存の機能群 ---

        static void PerformCommit(Repository repo)
        {
            try
            {
                Commands.Stage(repo, "*");
                Signature author = new Signature("Kyou", "kyou@example.com", DateTimeOffset.Now);
                Commit commit = repo.Commit("自動スナップショット", author, author);
                Console.WriteLine($"現在の状態を記録いたしました。ID: {commit.Id.Sha.Substring(0, 7)}");
            }
            catch (EmptyCommitException)
            {
                Console.WriteLine("前回から変更されたファイルは存在いたしません。");
            }
        }

        static void ShowHistory(Repository repo)
        {
            Console.WriteLine("=== 変更履歴 ===");
            foreach (Commit commit in repo.Commits.Take(10))
            {
                Console.WriteLine($"[{commit.Id.Sha.Substring(0, 7)}] {commit.Author.When:yyyy/MM/dd HH:mm:ss}");
            }
        }

        static void Rollback(Repository repo, string shortSha)
        {
            Commit targetCommit = repo.Commits.FirstOrDefault(c => c.Id.Sha.StartsWith(shortSha, StringComparison.OrdinalIgnoreCase));

            if (targetCommit == null)
            {
                Console.WriteLine($"指定されたコミットID '{shortSha}' は見つかりませんわ。");
                return;
            }

            repo.Reset(ResetMode.Hard, targetCommit);
            Console.WriteLine($"ディレクトリの状態を {shortSha} の時点に復元いたしました。");
        }

        static string GetTargetDirectoryFromIni()
        {
            string iniFilePath = "settings.ini";
            if (File.Exists(iniFilePath))
            {
                foreach (var line in File.ReadAllLines(iniFilePath))
                {
                    if (line.StartsWith("TargetPath=", StringComparison.OrdinalIgnoreCase))
                    {
                        return line.Substring("TargetPath=".Length).Trim();
                    }
                }
            }
            return null;
        }
    }
}