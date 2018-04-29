#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_3_5
public class PlatformData
#else
public class PlatformData : ScriptableObject
#endif
{
    public string APK名称 = "";
    public PerPlatform[] 平台列表 = new PerPlatform[1];

    public PerPlatform GetData()
    {
        return this.平台列表[0];
    }

    static public PlatformData c(string apkName, params PerPlatform[] list)
    {
        PlatformData d = new PlatformData();
        d.APK名称 = apkName;
        d.平台列表 = list;
        return d;
    }
}


[System.Serializable]
public class PerPlatform
{
    public string 艺名 =  "";    

    [SerializeField]
    public string 产品名称 = "";

    [SerializeField]
    public PluginUtils.PLATFORM_TYPE  平台;
    [SerializeField]
    public PluginUtils.LANG_TYPE 语言;

    public TextAsset ManifestXml;
	

    public string 版本号 = "1.0.0";
    public int 数字版本号 = 1;
    
    /// <summary>
    /// 用逗号隔开
    /// </summary>
    public Object[] 不发布的场景名;
	
	//为了3.56设置的变量
	[HideInInspector]
	public string[] 不发布的场景名字;
	
	[HideInInspector]
    public string ManifestXmlPath;

    /// <summary>
    /// ....
    /// </summary>
    public bool 是否显示购买图 = false;

    /// <summary>
    /// 自定义数据
    /// </summary>
    public List<CustomPlatformData> 自定义数据;


    public BuildCopyAction[] 复制文件;

    /// <summary>
    /// 3.5的生成
    /// </summary>
    /// <param name="name"></param>
    /// <param name="productName"></param>
    /// <param name="type"></param>
    /// <param name="lang"></param>
    /// <param name="manifestXMLPath"></param>
    /// <param name="bundleVersion"></param>
    /// <param name="version"></param>
    /// <param name="noBuildSceneName"></param>
    /// <returns></returns>
    public static PerPlatform c(string name, string productName, PluginUtils.PLATFORM_TYPE type,
                                                PluginUtils.LANG_TYPE lang, string manifestXMLPath, string bundleVersion,
                                                int version, string[] noBuildSceneName = null, BuildCopyAction[] copy2StreamAssets = null)
    {
        PerPlatform d = new PerPlatform();
        d.语言 = lang;
        d.艺名 = name;
        d.产品名称 = productName;
        d.平台 = type;
        d.数字版本号 = version;
        d.版本号 = bundleVersion;
        d.ManifestXmlPath = manifestXMLPath;        
		d.不发布的场景名字 = noBuildSceneName;

        if (copy2StreamAssets != null)
        {
            for (int i = 0; i < copy2StreamAssets.Length; i++)
            {
                var o = AssetDatabase.LoadAssetAtPath(Application.dataPath + copy2StreamAssets[i].filePath, typeof(Object)) as Object;
                if (o != null)
                {
                    copy2StreamAssets[i].文件 = o;
                }
            }
            d.复制文件 = copy2StreamAssets;
        }       
        return d;
    } 
}

[System.Serializable]
public class CustomPlatformData
{
    public string name;
    public string value;
}

[System.Serializable]
public class BuildCopyAction
{
    /// <summary>
    /// 只需要填写类似 streamingassets 目录,或者 plugins/android 目录
    /// </summary>
    public string 复制到目录;
    public UnityEngine.Object 文件;

    [HideInInspector]
    /// <summary>
    /// for 3.5.6
    /// </summary>
    public string filePath;
}




#endif


#if UNITY_4_5_OR_NEWER
#if UNITY_EDITOR
[CanEditMultipleObjects()]
[CustomEditor(typeof(PlatformData), true)]
public class PlatformDataExtension : Editor
{

    string text = "";
    string numText = "";

    string paramName = "";
    string paramValue = "";


    public override void OnInspectorGUI()
    {
     
        base.OnInspectorGUI();
        PlatformData t = target as PlatformData;


        GUILayout.Space(30);
        EditorGUILayout.LabelField("统一所有版本号");
        text = EditorGUILayout.TextField("版本号",text );
        numText = EditorGUILayout.TextField("数字版本号",numText);
        if (GUILayout.Button("确定"))
        {
            int num = 0;
            int.TryParse(numText, out num);

            for (int i = 0; i < t.平台列表.Length; i++)
            {
                t.平台列表[i].版本号 = text;
                t.平台列表[i].数字版本号 = num;
            }
        }

        GUILayout.Space(30);
        GUILayout.Label("自定义参数 批量操作");
        paramName = EditorGUILayout.TextField("参数名", paramName);
        paramValue = EditorGUILayout.TextField("参数值", paramValue);
        int doWhat = 0;
        if (GUILayout.Button("添加参数")){
            doWhat = 1;
        }

        if (GUILayout.Button("删除参数"))
        {
            doWhat = 2;
        }

        if (GUILayout.Button("全清除"))
        {
            doWhat = 3;
        }

        if (doWhat != 0)
        {
            if (paramName == "" && doWhat!=3)
            {
                doWhat = 0;
            }

            for (int i = 0; i < t.平台列表.Length; i++)
            {
                if (doWhat == 1)
                {
                    bool found = false;
                    for (int j = 0; j < t.平台列表[i].自定义数据.Count; j++)
                    {
                        if (t.平台列表[i].自定义数据[j].name == paramName)
                        {                            
                            found = true;
                            Debug.Log(t.平台列表[i].艺名 + "自定义参数[" + paramName + "]已存在。不需要添加");
                            break;
                        }
                    }
                    if (!found)
                    {
                        t.平台列表[i].自定义数据.Add(new CustomPlatformData() { name = paramName, value = paramValue });
                    }
                }
                else if(doWhat==2)
                {
                    for (int j = 0; j < t.平台列表[i].自定义数据.Count; j++)
                    {
                        if (t.平台列表[i].自定义数据[j].name == paramName)
                        {
                            t.平台列表[i].自定义数据.RemoveAt(j);
                            break;
                        }
                    }
                }
                else if (doWhat == 3)
                {
                    t.平台列表[i].自定义数据.Clear();
                }               
            }

            paramName = "";
            paramValue = "";

        }
    }
}
#endif
#endif