using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PowerTerminal.Command
{
    public class Intorsions
    {
        public Intorsions(PowerShell ps)
        {
            ps.AddScript("function Set-Connect {param([string]$Name,[string]$Command)}");
            ps.AddScript("function Set-Reserve {param([string]$Server,[string]$Account,[string]$Key)}");
            ps.Invoke();
            ps.Commands.Clear();
        }

        public static void DispatchCommand(MainWindow _window, PowerShell _ps)
        {
            var inputTextBox = _window.InputTextBox;
            var OutputTextBlock = _window.OutputTextBlock;

            string command = inputTextBox.Text;
            string coreCommand = Utilities.GetCoreCommand(command).ToLower();

            OutputTextBlock.Text += $"> {command}\n";
            inputTextBox.Clear();

            if (string.IsNullOrWhiteSpace(coreCommand)) return;


            // コマンドの内容に応じて適切な処理を呼び出す
            var msg = coreCommand switch
            {
                "set-reserve" => "ssh接続定義を登録します\n",
                "set-connection" => "ssh接続をおこないます\n",
                _ => null
            };

            if (string.IsNullOrWhiteSpace(msg))
            {
                // その他のコマンドの処理
                NewFunctions.RunCommand(command, OutputTextBlock, _ps);
            }
            else
            {
                OutputTextBlock.Text += msg;
            }
        }
    }
    public class NewFunctions
    {
        public static void RunCommand(string command, TextBlock OutputTextBlock, PowerShell _ps)
        {
            try
            {
                // コマンドを送信
                _ps.AddScript(command);

                // 実行し、結果を回収
                var results = _ps.Invoke();

                StringBuilder sb = new StringBuilder();

                // 正常な出力を文字列として連結
                foreach (var result in results)
                {
                    sb.AppendLine(result.ToString());
                }

                // エラーが出た場合の処理
                if (_ps.Streams.Error.Count > 0)
                {
                    foreach (var error in _ps.Streams.Error)
                    {
                        sb.AppendLine($"[Error] {error.ToString()}");
                    }
                }

                // 画面のテキストブロックに追記
                OutputTextBlock.Text += sb.ToString() + "\n";
            }
            catch (Exception ex)
            {
                // C#側の例外エラー処理
                OutputTextBlock.Text += $"[Exception] {ex.Message}\n\n";
            }
        }
        public static void GunZip()
        {

        }
    }

    public class Utilities
    {
        public static string GetCoreCommand(string command)
        {
            ReadOnlySpan<char> span = command.AsSpan().Trim();
            for(int i = 0; i < span.Length; i++)
            {
                if (span[i] == ' ')
                {
                    return span.Slice(0, i).ToString();
                }
            }
            return command;
        }
    }
}
