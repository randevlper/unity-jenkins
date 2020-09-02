using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

class JenkinsBuild {
    static string[] EnabledScenes;

    //AppName Path Build 

    public static void Build () {
        string appName = "AppName"; //ex Idol
        string targetDir = "~/Desktop"; // Folder
        string buildTargetGroup = "standalone";
        string buildTarget = "win64";
        string buildOptions = "release";

        string[] args = System.Environment.GetCommandLineArgs ();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "-executeMethod") {
                if (i + 6 < args.Length) {
                    appName = args[i + 2];
                    targetDir = args[i + 3];
                    buildTargetGroup = args[i + 4];
                    buildTarget = args[i + 5];
                    buildOptions = args[i + 6];
                } else {
                    System.Console.WriteLine (
                        "[JenkinsBuild] Incorrect Parameters for -executeMethod Format: -executeMethod Build <app name> <output dir> <build target group> <build target> ");
                    return;
                }
            }
        }

        EnabledScenes = FindEnabledEditorScenes ();
        string fullPathAndName = targetDir + System.IO.Path.DirectorySeparatorChar + appName;
            

        BuildTarget e_buildTarget;
        BuildTargetGroup e_buildTargetGroup;
        BuildOptions e_buildOptions;

        switch (buildTargetGroup) {
            case "standalone":
                e_buildTargetGroup = BuildTargetGroup.Standalone;
                break;
            default:
                System.Console.WriteLine ("[JenkinsBuild] Incorrect Parameters for buildTargetGroup: " + buildTargetGroup);
                return;
        }

        switch (buildTarget) {
            case "win32":
                e_buildTarget = BuildTarget.StandaloneWindows;
                fullPathAndName += ".exe";
                break;
            case "win64":
                e_buildTarget = BuildTarget.StandaloneWindows64;
                fullPathAndName += ".exe";
                break;
            case "linux":
                e_buildTarget = BuildTarget.StandaloneLinux64;
                fullPathAndName += ".x86_64";
                break;
            default:
                System.Console.WriteLine ("[JenkinsBuild] Incorrect Parameters for buildTarget: " + buildTarget);
                return;
        }

        switch (buildOptions)
        {
            case "release":
                e_buildOptions = BuildOptions.None;
                break;
            case "dev":
                e_buildOptions = BuildOptions.Development | BuildOptions.AllowDebugging;
                break;
            default:
            System.Console.WriteLine ("[JenkinsBuild] Incorrect Parameters for buildOptions: " + buildOptions);
                return;
        }

        BuildProject (EnabledScenes, e_buildTargetGroup, e_buildTarget, fullPathAndName, e_buildOptions);
    }

    private static string[] FindEnabledEditorScenes () {
        List<string> EditorScenes = new List<string> ();
        foreach (var scene in EditorBuildSettings.scenes) {
            EditorScenes.Add (scene.path);
        }
        return EditorScenes.ToArray ();
    }

    private static void BuildProject (string[] scenes,
        BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, string targetDir, BuildOptions buildOptions) {
        System.Console.WriteLine ("[JenkinsBuild] Building:" + targetDir + " buildTargetGroup:" +
            buildTargetGroup.ToString () + " buildTarget:" + buildTarget.ToString ());
        // https://docs.unity3d.com/ScriptReference/EditorUserBuildSettings.SwitchActiveBuildTarget.html

        bool switchResult = EditorUserBuildSettings.SwitchActiveBuildTarget (
            buildTargetGroup, buildTarget);

        if (switchResult) {
            System.Console.WriteLine ("[JenkinsBuild] Successfully changed Build Target to: " +
                buildTarget.ToString ());
        } else {
            System.Console.WriteLine ("[JenkinsBuild] Unable to change Build Target to: " +
                buildTarget.ToString () + " Exiting...");
            return;
        }

        // https://docs.unity3d.com/ScriptReference/BuildPipeline.BuildPlayer.html
        BuildReport buildReport = BuildPipeline.BuildPlayer (scenes, targetDir, buildTarget, buildOptions);
        BuildSummary buildSummary = buildReport.summary;
        if (buildSummary.result == BuildResult.Succeeded) {
            System.Console.WriteLine ("[JenkinsBuild] Build Success: Time:" +
                buildSummary.totalTime + " Size:" + buildSummary.totalSize + " bytes");
        } else {
            System.Console.WriteLine ("[JenkinsBuild] Build Failed: Time:" +
                buildSummary.totalTime + " Total Errors:" + buildSummary.totalErrors);
        }
    }
}