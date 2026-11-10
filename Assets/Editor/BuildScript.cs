using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// WebGL build script for Puzzle Rivals deployment.
/// Can be called via Unity CLI with: 
///   -executeMethod BuildScript.PerformWebGLBuild
/// or from the Unity Editor menu.
/// </summary>
public static class BuildScript
{
    /// <summary>
    /// Build method for GitHub Actions deployment.
    /// Outputs to WebGL-Deploy-Staging folder next to the project.
    /// </summary>
    public static void PerformWebGLBuild()
    {
        string outputPath = "WebGL-Deploy-Staging";
        BuildWebGL(outputPath);
    }

    /// <summary>
    /// Legacy build method for local deployment.
    /// </summary>
    public static void Build()
    {
        string output = Path.Combine(GetProjectRoot(), "WebGL-Deploy-Staging");
        BuildWebGL(output);
    }

    /// <summary>
    /// Main build logic for WebGL.
    /// </summary>
    /// <param name="output">Output directory path</param>
    private static void BuildWebGL(string output)
    {
        Directory.CreateDirectory(output);
        
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && File.Exists(s.path))
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new Exception("No enabled scenes found in EditorBuildSettings.");

        Debug.Log($"PR_WEBGL_BUILD_START scenes={scenes.Length} output={output}");
        Debug.Log($"PR_WEBGL_BUILD_UNITY={Application.unityVersion}");
        Debug.Log($"PR_WEBGL_BUILD_COMPRESSION={PlayerSettings.WebGL.compressionFormat}");

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(output, "index.html"),
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"PR_WEBGL_BUILD_RESULT={report.summary.result}");
        Debug.Log($"PR_WEBGL_BUILD_ERRORS={report.summary.totalErrors}");
        Debug.Log($"PR_WEBGL_BUILD_WARNINGS={report.summary.totalWarnings}");
        Debug.Log($"PR_WEBGL_BUILD_SIZE={report.summary.totalSize}");
        
        if (report.summary.result != BuildResult.Succeeded || report.summary.totalErrors > 0)
        {
            throw new Exception($"WebGL build failed: {report.summary.result} errors={report.summary.totalErrors}");
        }
    }

    /// <summary>
    /// Get the project root directory (works in Editor and CLI mode).
    /// </summary>
    private static string GetProjectRoot()
    {
        string path = Application.dataPath;
        // Remove "Assets" from the path
        if (path.EndsWith("/Assets"))
            path = path.Substring(0, path.Length - 6);
        else if (path.EndsWith("\\Assets"))
            path = path.Substring(0, path.Length - 6);
        return path;
    }
}