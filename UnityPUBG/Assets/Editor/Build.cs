using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        string projectRoot = Directory.GetCurrentDirectory();
        string outputDirectory = Path.Combine(projectRoot, "android");
        Directory.CreateDirectory(outputDirectory);

        string outputPath = Path.Combine(outputDirectory, "BALA-E-FOGO.apk");
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new Exception("Nenhuma cena habilitada em EditorBuildSettings.");

        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Android,
            BuildTarget.Android
        );

        PlayerSettings.productName = "BALA E FOGO";
        PlayerSettings.applicationIdentifier = "com.binhozer09.balaefogo";
        PlayerSettings.Android.targetArchitectures =
            AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Build Android falhou: " + report.summary.result);

        Debug.Log("APK gerado em: " + outputPath);
    }
}
