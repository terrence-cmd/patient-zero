using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Batch-mode build entry points. This is the exact contract the external
/// build manager (scripts/build-manager.ps1) calls:
///   Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWebGL
///   Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildDesktop
/// Do not rename this class or these methods.
/// </summary>
public static class BuildScript
{
    private static string[] EnabledScenes =>
        EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

    public static void BuildWebGL()
    {
        Run(new BuildPlayerOptions
        {
            scenes = EnabledScenes,
            target = BuildTarget.WebGL,
            locationPathName = "Builds/WebGL",
            options = BuildOptions.None
        });
    }

    public static void BuildDesktop()
    {
        Run(new BuildPlayerOptions
        {
            scenes = EnabledScenes,
            target = BuildTarget.StandaloneWindows64,
            locationPathName = "Builds/Desktop/PatientZero.exe",
            options = BuildOptions.None
        });
    }

    private static void Run(BuildPlayerOptions opts)
    {
        if (opts.scenes.Length == 0)
        {
            Debug.LogError("[BuildScript] No enabled scenes in EditorBuildSettings — aborting build.");
            EditorApplication.Exit(1);
            return;
        }

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] {summary.platform} build SUCCEEDED -> {opts.locationPathName} " +
                      $"({summary.totalSize} bytes, {summary.totalTime.TotalSeconds:F0}s, " +
                      $"{summary.totalErrors} errors, {summary.totalWarnings} warnings)");
        }
        else
        {
            Debug.LogError($"[BuildScript] {summary.platform} build FAILED: {summary.result} " +
                           $"({summary.totalErrors} errors). See log above.");
            EditorApplication.Exit(1);
        }
    }
}
