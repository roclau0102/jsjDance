#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Text;
using System.Net;
using System.Threading;


/// <summary>
/// 注意，目录结构需要为
/// Plugins/
/// Plugins/Android/ (此目录不列入SVN范围)
/// Plugins-os/
/// Plugins-wx/
/// Editor/JSJ/
public class PluginUtils : EditorWindow
{

    #region 变量

    public const string ASSETS_PATH = "Assets/Editor/JSJ/PlatformDataObj.asset";

    [System.Serializable]
    public enum PLATFORM_SHOW_TYPE
    {
        显示,
        隐藏
    }

    public enum PLATFORM_TYPE
    {
        None,
        WX,
        OS,
        USB
    }
    [System.Serializable]
    public enum LANG_TYPE
    {
        CN,
        EN
    }
    #endregion

    #region 命令

    [MenuItem("GAME/生成版本文件")]
    static public void CreateVersionMenu()
    {
        PlatformData d = AssetDatabase.LoadAssetAtPath(ASSETS_PATH, typeof(PlatformData)) as PlatformData;
        string packName = null;
        string xml = null;
        GetPackName(d.平台列表[0], out packName, out xml);
        CreateVersion(d.平台列表[0], packName);
    }

#if UNITY_3_5

     [MenuItem("GAME/刷新3.X版本菜单")]
    public static void Create356Menu()
    {
        string v = "using UnityEngine; \r\n using UnityEditor; \r\n public class MenuItem356{ \r\n //这个文件专门为356生成的，4以上请删除 \r\n";
        for (int i = 0; i < GameInfo.data.平台列表.Length; i++)
        {
            v += "[MenuItem(\"GAME/发布3.X游戏/" + GameInfo.data.平台列表[i].艺名 + "\")]\r\n";
            v += "public static void Create" + i + "(){ \r\n";
            v += "PluginUtils.DoStuffs(GameInfo.data.平台列表[" + i + "], GameInfo.data.APK名称);\r\n";
            v += "} \r\n";
        }
        v += "}";
        SaveFile(v, Application.dataPath + "/Editor/JSJ/MenuItem356.cs");
    }

#else
    //[MenuItem("GAME/CreateScriptObj")]
    static public void CreateScriptObject()
    {
        string p = "Assets/Editor/JSJ/PlatformData.asset";
        PlatformData sd = ScriptableObject.CreateInstance<PlatformData>();
        AssetDatabase.CreateAsset(sd, p);
    }

    /// <summary>
    /// 设置预编译参数，由于一些游戏是3.5版本的，旧版本没法使用预编译参数，所以这个功能不能用
    /// </summary>
    /// <param name="type"></param>
    static public void SetScriptingDefine(PLATFORM_TYPE type)
    {

        string s = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        string[] arr = s.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        string result = "";
        bool find = false;
        string name = "PLATFORM_" + type;
        for (int i = 0; i < arr.Length; i++)
        {
            foreach (PLATFORM_TYPE item in Enum.GetValues(typeof(PLATFORM_TYPE)))
            {
                if (arr[i] == name)
                {
                    find = true;
                    break;
                }
            }

            if (!find) result += arr[i] + ";";
        }
        result += name;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, result);
    }
#endif
    #endregion

    #region 生成版本

    static public void CreateVersion(PerPlatform per, string packName)
    {
        PLATFORM_TYPE type = per.平台;
        LANG_TYPE curLang = per.语言;
        string gameName = per.产品名称;


        string v = "using System.Collections.Generic;\r\npublic class Version{ \r\n";
        foreach (PLATFORM_TYPE item in Enum.GetValues(typeof(PLATFORM_TYPE)))
        {
            v += "static public bool Is" + item.ToString() + "(){ return " + (type == item ? "true" : "false") + "; }\r\n";
        }

        foreach (LANG_TYPE item in Enum.GetValues(typeof(LANG_TYPE)))
        {
            v += "static public bool Is" + item.ToString() + "(){ return " + (curLang == item ? "true" : "false") + "; }\r\n";
        }

        v += "public const bool SHOW_BUY_TIPS = " + (per.是否显示购买图 ? "true" : "false") + ";\r\n";

        v += "public const string packName = \"" + packName + "\";\r\n";

        v += "public const string gameName=\"" + gameName + "\";\r\n";

        v += "public enum PLAFTFORM_ENUM{";

        PerPlatform[] platforms = null;


#if UNITY_3_5
         platforms = GameInfo.data.平台列表;
         
#else
        if (platformDatas == null)
        {
            platformDatas = AssetDatabase.LoadAssetAtPath(ASSETS_PATH, typeof(PlatformData)) as PlatformData;
        }
        platforms = platformDatas.平台列表;
#endif

        for (int i = 0; i < platforms.Length; i++)
        {
            v += platforms[i].艺名.ToString();
            if (i != (platforms.Length - 1))
            {
                v += ",";
            }
        }


        v += "};\r\n";
        v += "public static PLAFTFORM_ENUM currentPlatform= PLAFTFORM_ENUM." + per.艺名.ToString() + ";\r\n";

        v += "public static Dictionary<string, string> customData = new Dictionary<string, string>(){";
        Dictionary<string, string> customData = new Dictionary<string, string>();

        if (per.自定义数据 != null)
        {
            for (int i = 0; i < per.自定义数据.Count; i++)
            {
                if (String.IsNullOrEmpty(per.自定义数据[i].name) == false)
                {
                    if (customData.ContainsKey(per.自定义数据[i].name) == false)
                    {
                        try
                        {
                            customData.Add(per.自定义数据[i].name, per.自定义数据[i].value);
                            v += "{\"" + per.自定义数据[i].name + "\",\"" + per.自定义数据[i].value + "\"}";
                        }
                        catch
                        {
                            Debug.LogWarning("自定义数据" + per.自定义数据[i].name + ", " + per.自定义数据[i].value + " 创建失败，请注意不能使用特殊符号");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("自定义数据" + per.自定义数据[i].name + " 重复的KEY");
                    }
                }
            }

        }
        v += "};";

        v += "}";

        SaveFile(v, Application.dataPath + "/plugins/Version.cs"); //save for cs.

        v = "#pragma strict \r\n";
        foreach (PLATFORM_TYPE item in Enum.GetValues(typeof(PLATFORM_TYPE)))
        {
            v += "static  public function Is" + item.ToString() + "(){ return " + (type == item ? "true" : "false") + "; }\r\n";
        }

        foreach (LANG_TYPE item in Enum.GetValues(typeof(LANG_TYPE)))
        {
            v += "static  public function Is" + item.ToString() + "(){ return " + (curLang == item ? "true" : "false") + "; }\r\n";
        }

        v += "static public var packName:String = \"" + packName + "\";\r\n";

        v += "public  static var SHOW_BUY_TIPS:boolean = " + (per.是否显示购买图 ? "true" : "false") + ";\r\n";

        v += "static public var gameName:String=\"" + gameName + "\";\r\n";

        v += "public enum PLAFTFORM_ENUM{";
        for (int i = 0; i < platforms.Length; i++)
        {
            v += platforms[i].艺名.ToString();
            if (i != (platforms.Length - 1))
            {
                v += ",";
            }
        }
        v += "};\r\n";
        v += "public static var currentPlatform:PLAFTFORM_ENUM= PLAFTFORM_ENUM." + per.艺名.ToString() + ";\r\n";


        SaveFile(v, Application.dataPath + "/plugins/VersionJS.js");
    }


    static public void SaveFile(string content, string path, Encoding encode = null)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        if (encode == null)
        {
            encode = System.Text.Encoding.Unicode;
        }

        try
        {            
            FileStream aFile = new FileStream(path, FileMode.CreateNew);
            StreamWriter sw = new StreamWriter(aFile, encode);
            sw.Write(content);
            sw.Close();
        }
        catch { }
        AssetDatabase.Refresh();
    }
    #endregion

#region 切换目录并发布
    

    public static void GetPackName(PerPlatform d, out string packName, out string xml)
    {
        packName = null;
        xml = null;

        if (d.ManifestXml != null)
        {
            xml = d.ManifestXml.text;
        }
        else if (d.ManifestXmlPath != null)
        {
            string path = Application.dataPath + d.ManifestXmlPath;
            if ((string)d.ManifestXmlPath == ""){

                    Debug.Log("请设置ManifestXmlPath,否则无法生成！");
                return ;
            }
            FileStream aFile = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(aFile, System.Text.Encoding.UTF8);
            xml = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(sr.ReadToEnd())); ;
        }


        if (xml == null)
        {
            Debug.LogError("请设置" + d.艺名 + "的ManifestXml属性为目标XML:" + d.ManifestXml.text);
            return;
        }

        try
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            packName = doc.SelectSingleNode("manifest").Attributes["package"].Value.ToString();
        }
        catch
        {
            Debug.LogError("manifest文件有误，无法取得包名，请使用文本编辑器打开xml修改编码为unicode并保存.");            
            return ;
        }
    }

    public static void DoStuffs(PerPlatform d, string apkName, bool openBat = false)
    {
        PLATFORM_TYPE type = d.平台;
        if (type == PLATFORM_TYPE.None)
        {
            Debug.Log("发布平台未定义！");
            return;
        }

        string packName = null;
        string xml = null;
        GetPackName(d, out packName, out xml); 
        if (packName == null || xml == null) return;

        PlayerSettings.Android.bundleVersionCode = d.数字版本号;
        PlayerSettings.applicationIdentifier = packName;
        PlayerSettings.bundleVersion = d.版本号;
        PlayerSettings.productName = d.产品名称;
        PlayerSettings.companyName = "ZHJiaShiJie";
        CreateVersion( d ,packName);
        CopyPlugin(type.ToString().ToLower(), d, xml);

        if (apkName == "" || apkName == null)
        {
            apkName = packName + "_" + d.艺名;
        }
        else
        {
            apkName = apkName + "_" + d.艺名;
        }

        string[] nobuildSceneName = null;
        if (d.不发布的场景名 != null && d.不发布的场景名.Length > 0)
        {
            nobuildSceneName = new string[d.不发布的场景名.Length];
            for (int i = 0; i < d.不发布的场景名.Length; i++)
            {
                if (d.不发布的场景名[i] == null) continue;
                nobuildSceneName[i] = d.不发布的场景名[i].name;
            }
        }
        else if (d.不发布的场景名字 != null)
        {
            nobuildSceneName = d.不发布的场景名字;
        }
#if UNITY_4_5 || UNITY_4_2
        SetScriptingDefine(d.平台);
#endif
        Copy2StreamingAsset(d.复制文件);
        BuildForAndroid(apkName, GetBuildScenes(nobuildSceneName), !openBat);
        CreateBat(apkName, packName, openBat);
        Copy2StreamingAsset(d.复制文件,false);
    }


    private static void Copy2StreamingAsset(BuildCopyAction[] files, bool copy = true)
    {
        if (files == null) return;


        for (int i = 0; i < files.Length; i++)
        {
            if (string.IsNullOrEmpty(files[i].复制到目录)) continue;

            if (Directory.Exists(Application.dataPath + "/" + files[i].复制到目录) == false)
            {
                Directory.CreateDirectory(Application.dataPath + "/" + files[i].复制到目录);
            }

            string p = AssetDatabase.GetAssetPath(files[i].文件);
            if (p == null) continue;
            string[] fileNames = p.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string fileName = fileNames[fileNames.Length - 1];
            if (copy)
            {
                File.Copy(p, Application.dataPath + "/" + files[i].复制到目录 + "/" + fileName, true);
            }
            else
            {
                File.Delete(Application.dataPath + "/" + files[i].复制到目录 + "/" + fileName);
            }
        }
        AssetDatabase.Refresh();
    }



    private static void CreateBat(string apkName, string packName, bool open)
    {
        string folder = Application.dataPath.ToLower().Replace("assets", "") + "apk/";
        string batPath = folder + apkName + ".bat";
        if (open)
        {
                string content = "\r\necho off\r\necho Pls make sure you have connect to the server...\r\nset packagename=" + packName + "\r\n";
                content += "set apkName=" + apkName + "\r\n";
                content += "echo Uninstalling...\r\nadb uninstall %packagename%\r\necho Installing apk\r\n";
                content += "adb install %apkName%.apk\r\necho Booting\r\n";
                content += "adb shell am start -n %packagename%/.MainActivity\r\n";
                content += "echo Done. Enjoy.";
                content += "pause\r\n";
                content += "exit\r\n";
                content += "adb shell am start -n " + packName + "/.MainActivity\r\n";
                content = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(content));
                SaveFile(content, batPath, Encoding.ASCII);
            try
            {
                System.Diagnostics.Process pro = new System.Diagnostics.Process();
                FileInfo file = new FileInfo(batPath);
                pro.StartInfo.WorkingDirectory = file.Directory.FullName;
                pro.StartInfo.FileName = batPath;
                pro.StartInfo.CreateNoWindow = false;
                pro.Start();
                pro.WaitForExit();
            }
            catch
            {
                Debug.Log("无法运行BAT文件进行安装，请检查BAT文件是否存在。");
            }
        }
    }

    /// <summary>
    /// 设置生成的场景
    /// </summary>
    /// <param name="noPublishSceneName"></param>
    /// <returns></returns>
    static string[] GetBuildScenes(string[] noPublishSceneName)
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
            {
                bool found = false;
                if (noPublishSceneName != null)
                {
                    string[] path = e.path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < noPublishSceneName.Length; i++)
                    {
                        if (noPublishSceneName[i] == null) continue;
                        if ((noPublishSceneName[i] + ".unity") == path[path.Length - 1])
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    names.Add(e.path);
                }
            }
        }

        string buildSceneNames = "";
        string[] result = names.ToArray();
        for (int i = 0; i < result.Length; i++)
        {
            buildSceneNames += result[i];
            if (i != (result.Length - 1)) buildSceneNames += ",";
        }
        Debug.Log("生成场景列表：" + buildSceneNames);
        return result;
    }

    /// <summary>
    /// 打包apk.并设置签名
    /// </summary>
    /// <param name="apkName"></param>
    /// <param name="scenes"></param>
    static public void BuildForAndroid(string apkName, string[] scenes, bool openFolder=true)
    {
        PlayerSettings.Android.keystoreName = Application.dataPath + "/Editor/JSJ/xbw.keystore";
        PlayerSettings.Android.keyaliasPass = "123456";
        PlayerSettings.Android.keyaliasName = "xbw";
        PlayerSettings.Android.keystorePass = "123456";
        string folder = Application.dataPath.ToLower().Replace("assets", "") + "apk/";
        string path = folder + apkName + ".apk";

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, BuildOptions.None);
        if (openFolder) System.Diagnostics.Process.Start(folder);
    }




    /// <summary>
    /// 复制pllugin-x到对应的目录
    /// </summary>
    /// <param name="enName"></param>
    /// <param name="d"></param>
    static private void CopyPlugin(string enName, PerPlatform d, string xml)
    {
        string chName = d.艺名;
        List<string> paths = new List<string>();
        List<string> files = new List<string>();
        files.Clear();
        paths.Clear();
        String pluginPath = "Assets/Plugins-" + enName + "/";
        Recursive(pluginPath, ref files, ref paths);

        AssetDatabase.DeleteAsset("Assets/Plugins/Android");
        AssetDatabase.CreateFolder("Assets/Plugins", "Android");

        paths.Clear();
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            string newpath = file.Replace("Plugins-" + enName, "Plugins/Android");
            CreateFolder(newpath, ref paths);
            AssetDatabase.CopyAsset(file, newpath);
        }
        AssetDatabase.Refresh();

        pluginPath = Application.dataPath + "/plugins/android/";
        SaveFile(xml, pluginPath + "AndroidManifest.xml");
        Debug.Log("操作完成，已切换为" + chName + "平台");
    }

    /// <summary>
    /// 生成层级目录
    /// </summary>
    /// <param name="path"></param>
    /// <param name="paths"></param>
    static public void CreateFolder(string path, ref List<string> paths)
    {
        string[] folders = path.Split(new char[] { '/' });
        string pathName = "";

        for (int i = 3; i < folders.Length - 1; i++)
        {
            string p1 = "Assets/Plugins/Android/" + pathName;
            string fullPathName = p1 + folders[i];
            if (paths.IndexOf(fullPathName) == -1)
            {
                AssetDatabase.CreateFolder(p1.Substring(0, p1.Length - 1), folders[i]);
                paths.Add(fullPathName);
            }
            pathName += folders[i] + "/";
        }
    }


    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path, ref List<string> files, ref List<string> paths)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir, ref files, ref paths);
        }
    }
#endregion

#region 发布窗口/只有3.56无法使用这么高大上又吊的功能啊~

#if UNITY_3_5
    //不用窗口。直接用菜单吧
#else

    /*
 #region ScriptabledObject编辑窗口
[CanEditMultipleObjects()]
[CustomEditor(typeof(PlatformData), true)]
public class CameraExtension : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();        
        EditorGUILayout.HelpBox("Please setting carefully.", MessageType.Info);
        if (GUILayout.Button("打开发布窗口", GUILayout.Height(50)))
        {
            PluginUtils.AddWindow();
        }

        PluginUtils.sexyShow.Update();

        if (PluginUtils.sexyShow != null)
        {
            if (PluginUtils.sexyTexture != null)
            {                
                if(GUILayout.Button(PluginUtils.sexyTexture, GUILayout.MaxWidth(250), GUILayout.MaxHeight(350) )){
                    PluginUtils.sexyShow.GetSexy();
                }
            }
            else
            {
                if (GUILayout.Button("刷新"))
                {
                    PluginUtils.sexyShow.GetSexy();
                }
            }
        }
    }
}
#endregion
     */

    static PluginUtils window;
    [MenuItem("GAME/发布4.X游戏")]
    static public void AddWindow()
    {
        //创建窗口
        Rect wr = new Rect(0, 0, width, height);
        window = (PluginUtils)EditorWindow.GetWindowWithRect(typeof(PluginUtils), wr, true, "发布APK");
        window.antiAlias = 8;
        window.ShowUtility();
    }

    static PlatformData platformDatas;
    GUIStyle style;
    GUIStyle style1;
    GUIStyle style2;
    bool building = false;
    void Awake()
    {
        platformDatas = AssetDatabase.LoadAssetAtPath(ASSETS_PATH, typeof(PlatformData)) as PlatformData;

        style = new GUIStyle();
        style.normal.background = null;    //这是设置背景填充的
        style.normal.textColor = new Color(1, 1, 1);   //设置字体颜色的
        style.fontSize = 24;       //当然，这是字体颜色

        style1 = new GUIStyle();
        style1.normal.textColor = new Color(0, 1, 1);   //设置字体颜色的
        style1.fontSize = 18;       //当然，这是字体颜色


        //一些初始化设置
        if (!File.Exists(Path.GetFullPath(Application.dataPath) + "/Plugins/Version.cs"))
        {
            if (platformDatas.平台列表!=null && platformDatas.平台列表.Length > 0)
            {
                string packName = null;
                string xml = null;
                GetPackName(platformDatas.平台列表[0], out packName, out xml);
                CreateVersion(platformDatas.平台列表[0],packName);
            }
        }

        foreach (PLATFORM_TYPE item in Enum.GetValues(typeof(PLATFORM_TYPE)))
        {
            if (item == PLATFORM_TYPE.None) continue;
            string path = Path.GetFullPath(Application.dataPath) + "/plugins-" + item.ToString().ToLower();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        if (File.Exists(Application.dataPath + "/Editor/JSJ/MenuItem356.cs"))
        {
            File.Delete(Application.dataPath + "/Editor/JSJ/MenuItem356.cs");
            AssetDatabase.Refresh();
        }
        openBat1 = openBat = PlayerPrefs.GetInt("RUN_BAT", 0) == 1;

       if(sexyTexture==null)sexyShow.GetSexy();
    }

    static public  SexyShow sexyShow = new SexyShow((b) =>
    {
        if (sexyTexture == null) sexyTexture = new Texture2D(2, 2); 
        sexyTexture.LoadImage(b);
    });


    PerPlatform cur;
    private Vector2 scrollPosition;
    bool openBat = false;
    bool openBat1 = false;
    static public Texture2D sexyTexture;
    static int width = 800;
    static int height = 800;

    void OnGUI()
    {
        if (platformDatas == null) return;
        if (sexyShow.firstTime)
        {
            sexyShow.GetSexy();
        }
        sexyShow.Update();
        if (sexyShow.errorString != null)
        {
            window.ShowNotification(new GUIContent(sexyShow.errorString));
            sexyShow.errorString = null;
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxWidth(width), GUILayout.Width(width), GUILayout.Height(height), GUILayout.ExpandWidth(false));

        GUILayout.Label("@By James Zhan 2015.5.13 如未编辑发包参数，请点击以下按钮设置。设置完后即可发包", style1);
        if (!building)
        {            
            if (GUILayout.Button("编辑发布参数"))
            {
                AssetDatabase.OpenAsset(platformDatas.GetInstanceID());
            }

            openBat1 = GUILayout.Toggle(openBat1, "是否WIFI运行(需要先连接到服务器)");
            if (openBat1 != openBat)
            {
                openBat = openBat1;
                PlayerPrefs.SetInt("RUN_BAT", openBat1 ? 1 : 0);
                PlayerPrefs.Save();
            }
            

            float btnWidth = 150;
            float max = Mathf.Floor( width / btnWidth);
            int i = 0;
            for (i = 0; i < platformDatas.平台列表.Length; i++)
            {
                if (i == 0 || (i % max)==0)
                {
                    GUILayout.BeginHorizontal();
                }

                PerPlatform p = platformDatas.平台列表[i];
                if (GUILayout.Button(platformDatas.平台列表[i].艺名, GUILayout.Width(btnWidth), GUILayout.Height(30)))
                {
                    if (platformDatas.平台列表[i].ManifestXml == null || platformDatas.平台列表[i].ManifestXml.ToString() == "")
                    {
                        AssetDatabase.OpenAsset(platformDatas.GetInstanceID());
                    }
                    else
                    {
                        cur = p;
                        building = true;
                        EditorApplication.delayCall += DoCurrentCreate;
                        window.ShowNotification(new GUIContent("请骚等，正在生成...."));
                    }
                }

                if (platformDatas.平台列表[i].ManifestXml==null || platformDatas.平台列表[i].ManifestXml.ToString() == "")
                {
                    GUILayout.EndHorizontal();
                    EditorGUILayout.HelpBox("Hi, SX, you cant publish " + platformDatas.平台列表[i] .艺名+ " if you dont assign a MANIFEST xml =.=!!!", MessageType.Error);
                    GUILayout.BeginHorizontal();
                }

                if ((i%max) + 1 == max)
                {
                    GUILayout.EndHorizontal();
                }
            }
            if ((i % max) != 0) { 
                GUILayout.EndHorizontal();
            }

            if (sexyTexture != null)
            {
                GUILayout.Label(sexyShow.nowState + "," + "还有" + sexyShow.GetCount() + "张图片....");

                if (GUILayout.Button(sexyTexture))
                {
                    sexyShow.GetSexy();
                }
            }
            else
            {
                GUILayout.Label(sexyShow.nowState);
            }

        }
        else
        {
            GUILayout.Label("生成中，喝杯茶/咖啡？", style1);
            GUILayout.Space(10);
            if (GUILayout.Button("返回", GUILayout.Height(80)))
            {
                building = false;
            }
        }
        GUILayout.Space(10);
        GUILayout.EndScrollView();
    }

    void DoCurrentCreate()
    {
        DoStuffs(cur, platformDatas.APK名称, openBat);
    }
#endif
#endregion

}

#region 福利加载类
#if UNITY_3_5 
//nothing 2 do
#else
public class SexyShow
{
    enum STATE
    {
        NONE,
        Loading,
        LoadComplete,
        CallBackComplete
    }

    public bool firstTime = true;
    STATE state = STATE.NONE;
    byte[] textureBytes;
    float h;
    static public List<string> items;
    public string errorString = null;
    public System.Action<byte[]> callback;
    public string nowState = "";

    public int random = -1;
    public int maxCount = -1;
    public const string ASSETS_PATH = "assets/editor/jsj/temp.asset";

    public SexyShow(System.Action<byte[]> call)
    {
        callback = call;
    }

    public void GetSexy()
    {
        if (state != STATE.NONE) return;
        state = STATE.Loading;
        ImgData d = AssetDatabase.LoadAssetAtPath(ASSETS_PATH, typeof(ImgData)) as ImgData;
        if (d == null)
        {
            d = ScriptableObject.CreateInstance<ImgData>();
            AssetDatabase.CreateAsset(d, ASSETS_PATH);
        }

        if (d == null) return;   
        items = d.imgs; 
        Thread t = new Thread(GetImgBytes);
        t.Start();        
        firstTime = false;
    }

   

    void GetImgBytes()
    {
        nowState = "加载图片地址,第一次加载需要5到8秒。";
        if (items.Count == 0)
        {
            string uri = "http://pic.sogou.com/pics/channel/getAllRecomPicByTag.jsp?category=%E7%BE%8E%E5%A5%B3&tag=";
            uri+="%E5%85%A8%E9%83%A8&tag=&start=0&len=10000";
            string jsonDataStr = GetRomoteString(uri);
            if (jsonDataStr != null && jsonDataStr.Length > 0)
            {
                nowState = "解析图片地址，大概需要5秒，请稍等。";
                jsonDataStr = jsonDataStr.Replace("\r", "").Replace("\n", "");
                Dictionary<string, object> jsonResult = MiniJSON.Json.Deserialize(jsonDataStr) as Dictionary<string, object>;
                List<System.Object> temp = jsonResult["all_items"] as List<System.Object>;
                

                for(int i=0;i<temp.Count;i++){
                    if (temp[i] is List<System.Object>)
                    {
                        var temp1 = temp[i] as List<System.Object>;
                        for(int j=0;j< temp1.Count;j++ ){
                            var oo = temp1[j] as Dictionary<string, System.Object>;
                            if (oo == null)
                            {
                                break;
                            }
                            items.Add(oo["pic_url"] as string);
                        }
                    }
                    else { 
                        var oo = temp[i] as Dictionary<string, System.Object>;
                        if (oo == null)
                        {
                            break;
                        }
                        items.Add(oo["pic_url"] as string);
                    }
                }
            }
        }

        if (items.Count == 0)
        {
            errorString = "图片地址加载失败 =.=，没福利了";
            return;
        }
        maxCount = items.Count;
        random = -1;
        while (random == -1)
        {
            //wait....
        }
        string o = items[random] ;
        items.RemoveAt(random);

        if (items.Count == 0)
        {
            GetImgBytes();
            return;
        }
        nowState = "加载图片...";
        textureBytes = GetRomoteByte(o);
        state = STATE.LoadComplete;
        nowState = "加载完成，点击可切换。";
    }




    public void Update()
    {
        if (state == STATE.LoadComplete)
        {
            ImgData d = AssetDatabase.LoadAssetAtPath(ASSETS_PATH, typeof(ImgData)) as ImgData;
            ImgData newd = ScriptableObject.Instantiate(d) as ImgData;
            newd.hideFlags = HideFlags.DontSave;
            newd.imgs = items;     
            newd.hideFlags = 0;
            AssetDatabase.CreateAsset(newd, ASSETS_PATH);
            AssetDatabase.SaveAssets();

            state = STATE.NONE;
            if (callback != null) callback(textureBytes);
        }

        if (maxCount != -1)
        {
            random = UnityEngine.Random.Range(0, maxCount);
            maxCount = -1;
        }
    }

    string GetRomoteString(string path)
    {
        try
        {
            CNNWebClient client = new CNNWebClient();
            client.Timeout = 5;
            return client.DownloadString(path);
        }
        catch
        {
            errorString = "图片地址加载失败 =.=，没福利了";
        }
        return null;
    }

    Byte[] GetRomoteByte(string path)
    {        
        try
        {
            CNNWebClient client = new CNNWebClient();
            client.Timeout = 5;
            return client.DownloadData(path);
        }
        catch
        {
            errorString = "加载图片失败 =.=，没福利了";
        }
        return null;
    }



    internal string GetCount()
    {
        if (items!=null)
        {
            return items.Count.ToString();
        }
        return "0";
    }

    internal void DelCache()
    {
        ImgData d = AssetDatabase.LoadAssetAtPath(ASSETS_PATH, typeof(ImgData)) as ImgData;
        ImgData newd = ScriptableObject.Instantiate(d) as ImgData;
        newd.hideFlags = HideFlags.DontSave;
        newd.imgs.Clear();
        newd.hideFlags = 0;
        AssetDatabase.CreateAsset(newd, ASSETS_PATH);
        AssetDatabase.SaveAssets();
        items.Clear();
    }
}


public class CNNWebClient : WebClient
{

    private int _timeOut = 10;

    /// <summary>
    /// 过期时间
    /// </summary>
    public int Timeout
    {
        get
        {
            return _timeOut;
        }
        set
        {
            if (value <= 0)
                _timeOut = 10;
            _timeOut = value;
        }
    }

    /// <summary>
    /// 重写GetWebRequest,添加WebRequest对象超时时间
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    protected override WebRequest GetWebRequest(Uri address)
    {
        HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
        request.Timeout = 1000 * Timeout;
        request.ReadWriteTimeout = 1000 * Timeout;
        return request;
    }
}
#endif
#endregion
#endif