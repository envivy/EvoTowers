using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EvoTowers.EditorTools
{
    public static class LubanTableGenerator
    {
        private const string ToolDirectory = "Tools/Luban";
        private const string ScriptName = "Gen.bat";

        [MenuItem("EvoTowers/Luban/Generate Tables")]
        public static void GenerateTables()
        {
            DirectoryInfo projectDirectory = Directory.GetParent(Application.dataPath);
            if (projectDirectory == null)
            {
                EditorUtility.DisplayDialog("Luban", "Cannot resolve Unity project root.", "OK");
                return;
            }

            string projectRoot = projectDirectory.FullName;
            string toolPath = Path.Combine(projectRoot, ToolDirectory);
            string scriptPath = Path.Combine(toolPath, ScriptName);

            if (!File.Exists(scriptPath))
            {
                EditorUtility.DisplayDialog("Luban", $"Generator script not found:\n{scriptPath}", "OK");
                return;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{scriptPath}\"",
                    WorkingDirectory = toolPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                UnityEngine.Debug.Log($"Luban table generation completed.\n{output}");
                AssetDatabase.Refresh();
                return;
            }

            UnityEngine.Debug.LogError($"Luban table generation failed with exit code {process.ExitCode}.\n{output}\n{error}");
            EditorUtility.DisplayDialog("Luban", "Table generation failed. Check the Console for details.", "OK");
        }

        [MenuItem("EvoTowers/Luban/Open Luban Folder")]
        public static void OpenLubanFolder()
        {
            DirectoryInfo projectDirectory = Directory.GetParent(Application.dataPath);
            if (projectDirectory == null)
            {
                return;
            }

            string projectRoot = projectDirectory.FullName;
            EditorUtility.RevealInFinder(Path.Combine(projectRoot, ToolDirectory));
        }
    }
}
