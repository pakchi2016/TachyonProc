using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Recommendations;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace cSharpEditor.function
{
    public static class RoslynUtil
    {
        public static IEnumerable<string> GetCompletions(string code, int position)
        {
            var workspace = new AdhocWorkspace();

            var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);

            // エディタ自身が現在読み込んでいる安全なアセンブリ（DLL）をすべて取得しますわ
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location));

            // 1. SolutionからProjectを追加し、取得した参照リストを一括で追加(AddMetadataReferences)いたします
            var project = workspace.CurrentSolution
                .AddProject("TempProject", "TempProject", LanguageNames.CSharp)
                .WithCompilationOptions(compilationOptions)
                .WithParseOptions(parseOptions)
                .AddMetadataReferences(references); // ← ここが変更点ですのよ

            // 2. その Project に対して、入力されたコードを Document として追加いたします
            var document = project.AddDocument("TempFile.cs", code);

            // 3. Document が属する最新の Solution 状態を Workspace に適用させます
            workspace.TryApplyChanges(document.Project.Solution);

            // 4. 反映済みの Workspace から再度 Document を取得し、SemanticModel を取り出します
            var appliedDocument = workspace.CurrentSolution.GetDocument(document.Id);
            var semanticModel = appliedDocument.GetSemanticModelAsync().Result;

            // 5. Workspaceに正しく紐づいた SemanticModel を渡します
            var symbols = Recommender.GetRecommendedSymbolsAtPositionAsync(
                semanticModel,
                position,
                workspace).Result;

            return symbols.Select(s => s.Name).Distinct().OrderBy(n => n);
        }
        public static IEnumerable<string> GetMissingUsings(string code, int position)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot();

            // 1. カーソル位置の直前にある単語（トークン）を取得します
            var token = root.FindToken(position - 1);
            string targetName = token.ValueText;

            if (string.IsNullOrWhiteSpace(targetName))
                return Enumerable.Empty<string>();

            // 2. 既に記述されているusingを抽出し、二重提案を防ぎます
            var existingUsings = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(u => u.Name.ToString())
                .ToHashSet();

            var suggestedNamespaces = new HashSet<string>();

            // 3. 現在読み込まれている全てのアセンブリ(DLL)を走査しますわ
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic))
            {
                try
                {
                    foreach (var type in assembly.GetExportedTypes())
                    {
                        // ① クラス名としての完全一致（ジェネリック含む）
                        if (type.Name == targetName || type.Name.StartsWith(targetName + "`"))
                        {
                            if (!string.IsNullOrEmpty(type.Namespace) && !existingUsings.Contains(type.Namespace))
                            {
                                suggestedNamespaces.Add(type.Namespace);
                            }
                        }

                        // ② 拡張メソッドとしての検索
                        // C#の静的クラスは内部的に abstract かつ sealed として扱われます
                        if (type.IsAbstract && type.IsSealed)
                        {
                            // 対象の文字列と同じ名前を持つ public static メソッドを探します
                            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                            foreach (var method in methods)
                            {
                                // そのメソッドが「拡張メソッド」であるか（ExtensionAttributeを持つか）を判定しますわ
                                if (method.Name == targetName && method.IsDefined(typeof(ExtensionAttribute), false))
                                {
                                    if (!string.IsNullOrEmpty(type.Namespace) && !existingUsings.Contains(type.Namespace))
                                    {
                                        suggestedNamespaces.Add(type.Namespace);
                                    }
                                    break; // 同じクラス内で1つ見つかれば十分ですのでループを抜けます
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // セキュリティ等で読み込めないアセンブリは華麗にスルーいたします
                }
            }

            return suggestedNamespaces.OrderBy(n => n);
        }
        public static IEnumerable<string> GetAllSyntaxErrors(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var diagnostics = tree.GetDiagnostics();

            // 特定のIDではなく、エラーレベル(Severity)が Error なものを全て抽出いたします
            var allErrors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

            var errorMessages = new List<string>();
            foreach (var err in allErrors)
            {
                int line = err.Location.GetLineSpan().StartLinePosition.Line + 1;
                // err.Id で CSxxxx などのコードを、err.GetMessage() で本家と同じエラー文面を取得できますわ
                errorMessages.Add($"{line}行目 [{err.Id}]: {err.GetMessage()}");
            }

            return errorMessages;
        }
    }
}