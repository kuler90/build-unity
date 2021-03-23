using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;

namespace kuler90
{
    public static class BuildCommand
    {
        private static string originalDefines;

        public static void Build()
        {
            ParseCommandLineArguments(out var args);

            HandleVersion(args);
            HandleDefines(args);
            var target = HandleTarget(args);
            var buildPath = HandleBuildPath(args);
            var buildOptions = HandleBuildOptions(args);
            var scenes = HandleScenesList(args);

            var buildReport = BuildPipeline.BuildPlayer(scenes, buildPath, target, buildOptions);
            int code = (buildReport.summary.result == BuildResult.Succeeded) ? 0 : 1;

            ResetDefines();
            EditorApplication.Exit(code);
        }

        private static BuildTarget HandleTarget(Dictionary<string, string> args)
        {
            var target = (BuildTarget)Enum.Parse(typeof(BuildTarget), args["buildTarget"], true);
            if (target == BuildTarget.Android)
            {
                if (args.ContainsKey("androidKeystoreName"))
                {
#if UNITY_2019_1_OR_NEWER
                    PlayerSettings.Android.useCustomKeystore = true;
#endif
                    PlayerSettings.Android.keystoreName = args["androidKeystoreName"];
                }
                if (args.ContainsKey("androidKeystorePass"))
                {
                    PlayerSettings.Android.keystorePass = args["androidKeystorePass"];
                }
                if (args.ContainsKey("androidKeyaliasName"))
                {
                    PlayerSettings.Android.keyaliasName = args["androidKeyaliasName"];
                }
                if (args.ContainsKey("androidKeyaliasPass"))
                {
                    PlayerSettings.Android.keyaliasPass = args["androidKeyaliasPass"];
                }
            }
            return target;
        }

        private static string HandleBuildPath(Dictionary<string, string> args)
        {
            var buildPath = args["buildPath"];
            EditorUserBuildSettings.buildAppBundle = buildPath.EndsWith(".aab");
            return buildPath;
        }

        private static string[] HandleScenesList(Dictionary<string, string> args)
        {
            return EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(s => s.path).ToArray();
        }

        private static BuildOptions HandleBuildOptions(Dictionary<string, string> args)
        {
            var buildOptions = BuildOptions.None;
            if (args.ContainsKey("buildOptions"))
            {
                buildOptions = args["buildOptions"]
                    .Split(',')
                    .Select(stringValue => Enum.TryParse<BuildOptions>(stringValue.Trim(), true, out var enumValue) ? enumValue : BuildOptions.None)
                    .Aggregate((f1, f2) => f1 | f2);
            }
            return buildOptions;
        }

        private static void HandleVersion(Dictionary<string, string> args)
        {
            if (args.ContainsKey("buildVersion"))
            {
                PlayerSettings.bundleVersion = args["buildVersion"];
            }
            if (args.ContainsKey("buildNumber"))
            {
                int buildNumber = int.Parse(args["buildNumber"]);
                PlayerSettings.Android.bundleVersionCode = buildNumber;
                PlayerSettings.iOS.buildNumber = buildNumber.ToString();
                PlayerSettings.macOS.buildNumber = buildNumber.ToString();
                PlayerSettings.tvOS.buildNumber = buildNumber.ToString();
            }
        }

        private static void HandleDefines(Dictionary<string, string> args)
        {
            if (args.ContainsKey("buildDefines"))
            {
                originalDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                string newDefines = originalDefines + ";" + args["buildDefines"];
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);
            }
        }

        private static void ResetDefines()
        {
            if (originalDefines != null)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, originalDefines);
                originalDefines = null;
            }
        }

        private static void ParseCommandLineArguments(out Dictionary<string, string> result)
        {
            result = new Dictionary<string, string>();
            string[] args = Environment.GetCommandLineArgs();

            // Extract flags with optional values
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {

                // Parse flag
                bool isFlag = args[current].StartsWith("-");
                if (!isFlag) continue;
                string flag = args[current].TrimStart('-');

                // Parse optional value
                bool flagHasValue = next < args.Length && !args[next].StartsWith("-");
                string value = flagHasValue ? args[next].TrimStart('-') : "";

                result.Add(flag, value);
            }
        }

        [PostProcessBuild(45)]
        // https://github.com/googlesamples/unity-jar-resolver/issues/328
        static void OnPostProcessPodfile(BuildTarget target, string path)
        {
            if (target == BuildTarget.iOS)
            {
                string podfilePath = Path.Combine(path, "podfile");
                if (File.Exists(podfilePath))
                {
                    string text = File.ReadAllText(podfilePath);
                    text = text.Replace("https://github.com/CocoaPods/Specs.git", "https://cdn.cocoapods.org");
                    File.WriteAllText(podfilePath, text);
                }
            }
        }
    }
}