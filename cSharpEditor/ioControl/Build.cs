using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Windows.Controls;

namespace cSharpEditor.ioControl
{
    public class Build
    {
        public static void CLIClear(TextBlock tb)
        {
            tb.Text = "";
        }
        public static void Create(TextBlock tb, string command="Get-ChildItem -Path D:\\")
        {
            using (var _ps = PowerShell.Create())
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
                tb.Text += sb.ToString() + "\n";
            }
        }
    }
}
