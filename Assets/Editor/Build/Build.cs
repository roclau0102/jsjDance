/*
 *Author by roc. All rights reserved. 
 */

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Build : Editor
{
    [MenuItem("Build/Android/AB")]
    public static void BuildABForAndroid()
    {
        DirectoryInfo sourceDir = new DirectoryInfo(Application.dataPath + "/Videos");
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        string[] assets = new string[] { "Assets/Videos/2.mp4", "Assets/Video/3.mp4", "Assets/Video/6.mp4", "Assets/Video/7.mp4" };
        buildMap[0].assetNames = assets;
        buildMap[0].assetBundleName = "videobundle";
        Directory.CreateDirectory(Application.streamingAssetsPath + "/videos");
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath + "/videos", buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
    }

    [MenuItem("Build/Android/APK")]
    public static void BuildAPK()
    {
        PlayerSettings.Android.keystorePass = "123456";
        PlayerSettings.Android.keyaliasPass = "123456";
        PlayerSettings.Android.forceSDCardPermission = true;
        
        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.allowDebugging = true;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = CollectScenes();
        string apkPathDir = Path.Combine(Application.dataPath, "/../Build/Android/");
        if (Directory.Exists(apkPathDir))
            Directory.CreateDirectory(apkPathDir);
        Debug.Log(apkPathDir);
        options.locationPathName = Path.Combine(apkPathDir, PlayerSettings.productName + ".apk");
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(options);

        System.Diagnostics.Process.Start("explorer.exe", apkPathDir.Replace('/', '\\'));
        Debug.Log("Builded apk successfully.");
    }

    static string[] CollectScenes()
    {
        List<string> scenePaths = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!string.IsNullOrEmpty(scene.path))
            {
                scenePaths.Add(scene.path);
                Debug.Log(scene.path);
            }
        }
        return scenePaths.ToArray();
    }
}
