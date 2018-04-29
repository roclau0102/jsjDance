/*
 * mp3格式音乐文件由于涉及版权问题，只能在移动端上播放，因此这里将mp3替换为wav
 */

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;


public class Global : MonoBehaviour {
    static public Global instance{get;private set;}
    public const string RES_HOST = "http://img.waixing.com:8080/AppFiles/apk/gamenet/zhdance/";

    /// <summary>
    /// 是否是加载自本地资源
    /// </summary>
    static public bool isAllResLocal = false;

    //Application.persistentDataPath:::   C:\Users\Administrator\AppData\LocalLow\ZHJiaShiJie\______  

    public enum MODE{        MODE_1P,        MODE_2P }
    
    public enum LIFE_TYPE { LV1, LV2, LV3, LV4 }

    public enum TIMING_JUAGE_TYPE    {        NONE = -1,        PREFECT = 0,        COOL,        GOOD,        BAD,        MISS    } 
    
    /// <summary>
    /// 音乐字典
    /// </summary>
    static public Dictionary<int, SongData> MUSIC_TABLE = new Dictionary<int, SongData>();

    /// <summary>
    /// 角色表
    /// </summary>
    static public Dictionary<int, CharacterData> CHARACTER_TABLE = new Dictionary<int, CharacterData>();

    /// <summary>
    /// 道具表
    /// </summary>
    static public Dictionary<int, PropData> PROP_TABLE = new Dictionary<int, PropData>();

    /// <summary>
    /// 屏幕高
    /// </summary>
    public const float SCREEN_HEIGHT = 10.8f;

    /// <summary>
    /// 评分模式
    /// </summary>
    public enum WIN_GRADE {  S,A,B,C,D }


    void Awake()
    {
        InitJO();
    }



    /// <summary>
    /// WWW加载的应用程序内容路径
    /// </summary>
    /// <returns></returns>
    public static string AppContentPathOfWWW()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                return  "file:///" + AppContentPath(); 
            case RuntimePlatform.IPhonePlayer:
                return AppContentPath();                
        }
        string path = AppContentPath();
        //path = path.Replace("/", "\\");
        path = "file:///" + path;
        return path;
    }

    /// <summary>
    /// 得到streamassets path 可以传文件路径进来，如 a/b.txt
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string AppStreamPath(string filePath="")
    {
        string path = Path.Combine(Application.streamingAssetsPath, filePath);
        #if UNITY_EDITOR
            //path = path.Replace("/", "\\");
            path = "file:///" + path;
        #endif
            return path;
    }

    public static void ShowTips(string s)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.Log("TIPS:"+s);
            return;
        }
        Global.CallAndroidStatic("StaticAlert", s);
    }


    /// <summary>
    /// 应用程序内容路径
    /// </summary>
    public static string AppContentPath()
    {
        string path = string.Empty;
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                //path = "jar:file://" + Application.dataPath + "!/assets/";
                path = Application.persistentDataPath + "/jsjdance/";
                break;
            case RuntimePlatform.IPhonePlayer:
                path = Application.dataPath + "/Raw/";
                break;
            default:
                path =  Application.persistentDataPath + "/";                
                break;
        }
        return path;
    }


    static AndroidJavaObject currentActivity;

    public static void InitJO()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (currentActivity == null)
                    {
                        AndroidJavaClass activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        currentActivity = activity.GetStatic<AndroidJavaObject>("currentActivity");
                    }
                }
        #endif
    }



	
	public static bool GetAndroidBool(string name){
		if (Application.platform != RuntimePlatform.Android) return false;
        InitJO();
        if(currentActivity==null)return false  ;   
		return currentActivity.Get<bool>(name); 
	}

    public static void CallAndroidStatic(string functionName)
    {
        if (Application.platform != RuntimePlatform.Android) return;
        InitJO();
#if UNITY_ANDROID
        if(currentActivity!=null)currentActivity.CallStatic(functionName);
#endif
    }

    public static void CallAndroidStatic(string functionName, params object[] data)
    {
        if (Application.platform != RuntimePlatform.Android)
        {            
            return;
        }
        InitJO();
#if UNITY_ANDROID
        if(currentActivity!=null)currentActivity.CallStatic(functionName, data);
#endif
    }

    /// <summary>
    /// 初始化开场视频
    /// </summary>
    public static void InitOpen(Action callback)
    {
        if (!File.Exists(AppContentPath() + "open.mp4"))
        {
            JLoader.instance.Load(AppStreamPath("open.mp4"), null, (type,info) =>
            {
                if (type == JLoader.DOWNLOAD_TYPE.SUCCESS)
                {
                    Directory.CreateDirectory(AppContentPath());
                    File.WriteAllBytes(AppContentPath() + "open.mp4", info.www.downloadHandler.data);
                    callback();
                }
                else
                {
                    Debug.Log(type);
                }
            });                                                                                                                                                                                                                            
        }
        else
        {
            callback();
        }
    }


    /// <summary>
    /// 初始化开场视频
    /// </summary>
    public static void InitVidoes(Action callback, string fileName)
    {
        if (!File.Exists(AppContentPath() + fileName))
        {
            JLoader.instance.Load(AppStreamPath(fileName), null, (type, info) =>
            {
                if (type == JLoader.DOWNLOAD_TYPE.SUCCESS)
                {
                    Directory.CreateDirectory(AppContentPath());
                    File.WriteAllBytes(AppContentPath() + fileName, info.www.downloadHandler.data);
                    callback();
                }
                else
                {
                    Debug.Log(type);
                }
            });
        }
        else
        {
            callback();
        }
    }



    public static void ExitGame()
    {
        if (Application.platform == RuntimePlatform.Android)
        {            
            new AndroidJavaObject("java.lang.System").CallStatic("exit", 0);                    
        }
    }



    public static GameModeSettings gameModeSetting;


    /// <summary>
    /// 初始化所有通用类
    /// </summary>
    public static void init(Action callback,bool playMusic=true)
    {
        if (instance == null)
        {            
            GameObject go = new GameObject();
            instance = go.AddComponent<Global>();
            instance.name = "GLOBAL";
            DontDestroyOnLoad(go);
            Application.targetFrameRate =60;
            TableManager.Init();
            DataUtils.instance.transform.parent = go.transform;
            Sounder.instance.transform.parent = go.transform;
            if (playMusic) Sounder.instance.Play("背景音乐", true);

            

            int v = DataUtils.AddLoginTime();
            if (v == 1)
            {
                if (Version.currentPlatform == Version.PLAFTFORM_ENUM.WX_SHOW)
                {
                    DataUtils.AddMoney(99999999);
                }else  if ( Version.currentPlatform == Version.PLAFTFORM_ENUM.WX_SHOW_NO_REGIST || Version.currentPlatform == Version.PLAFTFORM_ENUM.TEL)
                {
                    DataUtils.AddMoney(3000);
                }                
            }
        }
        instance.LoadMusicConfig(() => {
            Debug.Log("加载配件文件完成");
            string contentPath = AppContentPath();
            Directory.CreateDirectory(contentPath);

            if (File.Exists(contentPath + "2.mp4") && File.Exists(contentPath + "3.mp4") && File.Exists(contentPath + "6.mp4") && File.Exists(contentPath + "7.mp4"))
           {
               Debug.Log("视频文件已解压");
               callback();
               return;
           }

           string[] files;
            
            if(isAllResLocal){
                files = new string[] { "1.mp4", "2.mp4", "3.mp4", "4.mp4" , "5.mp4", "6.mp4", "7.mp4", "8.mp4" };
            }else{
                files = new string[]{ "2.mp4","3.mp4","6.mp4","7.mp4" };
            }
            
           int succesCount = 0;
           for (int i = 0; i < files.Length; i++)
           {
               JLoader.instance.Load(AppStreamPath(files[i]), files[i], (type, info) =>
               {
                   if (type == JLoader.DOWNLOAD_TYPE.SUCCESS)
                   {
                       File.WriteAllBytes(contentPath + ((string)info.userData), info.www.downloadHandler.data);
                       succesCount++;
                       if (succesCount == files.Length)
                       {
                           callback();
                       }
                   }
               });
           }
        });
    }

    
	

    
#region 视频/音频加载
    /// <summary>
    /// 下载封面
    /// </summary>
    /// <param name="data"></param>
    /// <param name="callback"></param>
    public void DownloadCover(SongData data, Action<object>callback)
    {
        string n = data.cover + ".png";
        string p;
        if (data.local)
        {
            p = AppStreamPath(Path.Combine("music", n));
        }else if (File.Exists(AppContentPath() + n))
        {
            p = AppContentPathOfWWW() + n;
        }
        else
        {
            p = Path.Combine(RES_HOST, Path.Combine("music", n));
        }

        Debug.Log("download cover:" + p);
        JLoader.instance.Load(p, null,(type, info) => {
            switch (type)
            {
                case JLoader.DOWNLOAD_TYPE.SUCCESS:
                    Debug.Log("download cover: success");
                    callback(info.www.downloadHandler.data);
                    string outfile = AppContentPath() + n;
                    File.WriteAllBytes(outfile, info.www.downloadHandler.data);
                    break;
                case JLoader.DOWNLOAD_TYPE.FAILED:
                    Debug.Log(info.www.url+ "加载失败~");
                    break;
            }
        });       
    }
    

    /// <summary>
    /// 下载歌曲
    /// </summary>
    /// <param name="callback"></param>
    public JLoaderInfo[] DownloadSong(SongData data, bool isGetInfo, Action<float, SongInfo> callback)
    {
        //如果文件都存在本地
        SongInfo info = new SongInfo();
        info.data = data;
                
        bool readLocal = false;
        string persistentDataPath = AppContentPath();

        if (!data.local)
        {
            if ((File.Exists(persistentDataPath + data.enName + ".mp3")|| File.Exists(persistentDataPath + data.enName + ".wav")) &&
            File.Exists(persistentDataPath + data.enName + "_easy.txt") &&
            File.Exists(persistentDataPath + data.enName + "_mid.txt") &&
            File.Exists(persistentDataPath + data.enName + "_hard.txt") &&
            File.Exists(persistentDataPath + data.cover + ".png"))
            {
                readLocal = true;
            }
        }

        //string[] downloadFileName = {  data.enName + ".mp3",
        //                         data.enName + "_easy.txt",
        //                         data.enName + "_mid.txt",
        //                        data.enName + "_hard.txt",
        //                        data.cover + ".png"
        //                            };
        string[] downloadFileName = {  data.enName + ".wav",
                                 data.enName + "_easy.txt",
                                 data.enName + "_mid.txt",
                                data.enName + "_hard.txt",
                                data.cover + ".png"
                                    };

        float[] percents = new float[downloadFileName.Length];
        bool failed = false;
        JLoaderInfo[] infos = new JLoaderInfo[downloadFileName.Length];
        for (int i = 0; i < downloadFileName.Length;i++ )
        {
            string n = downloadFileName[i];
            string p;
            if(readLocal){
                p = AppContentPathOfWWW() +  n;
            }
            else
            {
                if (!data.local)
                {
                    p = RES_HOST + "music/" + n;
                }
                else
                {
                    p = AppStreamPath("music/" + n);
                }
            }
            float[] pp = percents;
            Debug.Log("加载" + p);
            infos[i] = JLoader.instance.Load(p, i, (type, jinfo) =>
            {
                if (failed) return;
                 switch (type)
                 {
                     case JLoader.DOWNLOAD_TYPE.SUCCESS:
                         percents[(int)jinfo.userData] = 1;
                         if (isGetInfo) { 
                                if (p.IndexOf(".wav") != -1)
                                {
                                    //info.songClip = jinfo.www.GetAudioClip(false);
                                    info.songClip = DownloadHandlerAudioClip.GetContent(jinfo.www);
                                }
                                else if (p.IndexOf("_easy") != -1)
                                {
                                    info.ParseEasy(jinfo.www.downloadHandler.text);
                                }
                                else if (p.IndexOf("_mid") != -1)
                                {
                                    info.ParseMid(jinfo.www.downloadHandler.text);
                                }
                                else if (p.IndexOf("_hard") != -1)
                                {
                                    info.ParseHard(jinfo.www.downloadHandler.text);
                                }
                                else if (p.IndexOf(".png") != -1)
                                {
                                    info.bytes = jinfo.www.downloadHandler.data;
                                }
                            }

                         if (!readLocal && data.local==false)
                            {                                
                                string outfile = AppContentPath() + n;
                                string dir = Path.GetDirectoryName(outfile);
                                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                                File.WriteAllBytes(outfile, jinfo.www.downloadHandler.data);
                            }     
                         break;
                     case JLoader.DOWNLOAD_TYPE.FAILED:
                         failed = true;
                         Debug.Log(p + " 加载失败~" + jinfo.www.error);
                         callback(-1,null); 
                         break;
                 }
                 pp[(int)jinfo.userData] = jinfo.www.downloadProgress;
                 bool allDone = true;
                 float allPer = 0;
                 for (int j = 0; j < percents.Length; j++)
                 {
                     if (allDone && percents[j] != 1)
                     {
                         allDone = false;
                     }
                     if (percents[j] != -1)
                     {
                         allPer += percents[j];
                     }
                 }
                 if (allPer == -1 * downloadFileName.Length)
                 {
                     callback(-1, null);
                 }
                 else
                 {
                     callback(allPer / (float)percents.Length, info);
                 }
            });
        }
        return infos;
    }


    

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="callback"></param>
    public void LoadMusicConfig(System.Action callback)
    {
        if (MUSIC_TABLE.Count>0)
        {
            callback();
            return;
        }


        if (isAllResLocal)
        {
            string dataPath = AppStreamPath("data.txt");
            JLoader.instance.Load(dataPath, null, (type1, info1) =>
            {
                switch (type1)
                {
                    case JLoader.DOWNLOAD_TYPE.SUCCESS:
                        string text = System.Text.UTF8Encoding.Unicode.GetString(info1.www.downloadHandler.data);
                        TableManager.GetInstance().Load<SongData>(text, ref Global.MUSIC_TABLE);
                        if (callback != null) callback();
                        break;
                }
            });
            return;
        }


        Debug.Log("加载..." + RES_HOST + "data.txt");
        
        JLoader.instance.Load(RES_HOST + "data.txt", null, (type, info) =>
        {
            string str;
            
            switch (type)
            {
                case JLoader.DOWNLOAD_TYPE.SUCCESS:
                    str = Encoding.Unicode.GetString(info.www.downloadHandler.data);
                    //str = info.www.downloadHandler.text;
                    Debug.Log(str);
                    if (string.IsNullOrEmpty(str)||!str.StartsWith("id"))
                    {
                        Debug.LogWarning("发生网络异常，加载本地data表");
                        goto case JLoader.DOWNLOAD_TYPE.FAILED;
                    }
                    TableManager.GetInstance().Load<SongData>(str, ref Global.MUSIC_TABLE);
                    if (callback!=null) callback();
                    break;
                case JLoader.DOWNLOAD_TYPE.FAILED:
                    string dataPath = AppStreamPath("data.txt");
                    JLoader.instance.Load(dataPath, null, (type1, info1) =>
                    {
                        switch (type1)
                        {
                            case JLoader.DOWNLOAD_TYPE.SUCCESS:
                                string text = System.Text.UTF8Encoding.Unicode.GetString(info1.www.downloadHandler.data);
                                TableManager.GetInstance().Load<SongData>(text, ref Global.MUSIC_TABLE);
                                if (callback != null) callback();
                                break;
                        }
                    });
                    break;
                case JLoader.DOWNLOAD_TYPE.PROGRESS:
                    //Debug.Log("data.txt progress:" + info.www.progress);
                    break;
            }
        });
    }
   
    /// <summary>
    /// 是否加载视频成功
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool CheckVideoDownload(CharacterData data)
    {
        return File.Exists(AppContentPath() + data.id + ".mp4");
    }



    /// <summary>
    /// 下载视频
    /// </summary>
    /// <param name="data"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public JLoaderInfo DownloadVideo(CharacterData data, Action<float> callback)
    {
        string netPath = RES_HOST + "video/"+  data.id;
        Directory.CreateDirectory(AppContentPath() + "video/");
        return JLoader.instance.Load(netPath, null, (type, info) =>
        {            
            switch (type)
            {
                case JLoader.DOWNLOAD_TYPE.PROGRESS:
                    callback(info.result.progress);
                    break;
                case JLoader.DOWNLOAD_TYPE.FAILED:
                    callback(-1);                    
                    break;
                case JLoader.DOWNLOAD_TYPE.SUCCESS:                    
                    File.WriteAllBytes(AppContentPath() + data.id + ".mp4", info.www.downloadHandler.data);
                    callback(1);
                    break;
            }            
        });
    }

    /// <summary>
    /// 下载MP3
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    internal bool CheckMp3Download(SongData data)
    {
        if (data.local || isAllResLocal) return true;
        string path = AppContentPath();
        bool exist=((File.Exists( path+ data.enName + ".mp3")|| File.Exists(path + data.enName + ".wav")) &&
           File.Exists(path + data.enName + "_easy.txt") &&
           File.Exists(path + data.enName + "_mid.txt") &&
           File.Exists(path + data.enName + "_hard.txt") && 
           File.Exists(path + data.cover + ".png") 
           );
        return exist;
    }

    

    /// <summary>
    /// 封面texture
    /// </summary>
    static Texture2D texture = new Texture2D(1, 1);

    
    
    /// <summary>
    /// 是否已经下载过封面
    /// </summary>
    /// <param name="songData"></param>
    /// <returns></returns>
    public static bool IsDownloadCover(SongData songData)
    {
        if (songData.local)
        {
            return true;
        }
        string path = AppContentPath() + songData.cover + ".png";
        return File.Exists(path);
    }

    /// <summary>
    /// 得到歌曲封面texture
    /// </summary>
    /// <param name="songData"></param>
    /// <returns></returns>
    public static Texture2D GetSongCoverTexture(SongData songData)
    {
        string path = AppContentPath() + songData.cover + ".png";
        if (File.Exists(path)) {
            texture.LoadImage(File.ReadAllBytes(path));
            return texture;
        }
        return null;
    }



#endregion

    public static void GoAutoMode()
    {
        DataUtils.runingAutoMode = true;
        ShowTips("自动模式开启，按任意键取消");
    }


    public static void CancelAutoMode()
    {
        DataUtils.isAutoMode = false;
        DataUtils.runingAutoMode = false;
        ShowTips("取消自动模式");
    }

    internal static bool IsOverlayMode()
    {
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android) {
            InitJO();
            return currentActivity.Get<bool>("overlayMode");
        }
        else
        {            
            return false;
        }        
#else
        return true;
#endif
    }
}

#region 音乐类
public class SongInfo
{
    public AudioClip songClip;
    public SongTime[] easy;
    public SongTime[] mid;
    public SongTime[] hard;
    public SongData data;

    public byte[] bytes { get; set; }

    /// <summary>
    /// 难度
    /// </summary>
    public enum DIFF_LEVEL    {        EASY,        MID,        HARD    }

    /// <summary>
    /// 方向
    /// </summary>
    public enum DIRECTION    {        DOWN = 2,        LEFT = 4,        RIGHT = 6,       UP = 8,        NONE=0    }

    /// <summary>
    /// 长按数据类型
    /// </summary>
    public enum LONG_PRESS_TYPE    {        NONE=-1,        END=0,        START    }

    public void ParseEasy(string str)
    {
        easy = Parse(str);
        Debug.Log("[容易]最大分数：" + GetMaxMark(easy));
    }

    public void ParseMid(string str)
    {
        mid = Parse(str);
        Debug.Log("[中等]最大分数：" + GetMaxMark(mid));
    }

    public void ParseHard(string str)
    {
        hard = Parse(str);
        Debug.Log("[难]最大分数：" + GetMaxMark(hard));
    }

    static public int GetMaxMark(SongTime[] sts)
    {
        int mark = 0;
        int c = 0;
        for (int i = 0; i < sts.Length; i++)
        {
            if (sts[i] is LongSongTime)
            {
                if(c>1)mark += SongPlayer.GetMark(c, Global.TIMING_JUAGE_TYPE.PREFECT);
                c++;
            }

            if (c > 1) mark += SongPlayer.GetMark(c, Global.TIMING_JUAGE_TYPE.PREFECT);

            c++;
        }
        return mark;
    }

    /// <summary>
    /// 处理数据
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    SongTime[] Parse(string str)
    {
        string[] arr =str.Split(new char[]{'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
        List<SongTime> list = new List<SongTime>();
        for (int i = 0; i < arr.Length; i++)
        {
            string[] per = arr[i].Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
            string[] direction = per[1].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            for (int j = 0; j < direction.Length; j++)
            {
                SongTime time = null;                
                switch(direction[j].Length){
                    case 1:
                        time = new SongTime();
                        time.direction = (DIRECTION)(int.Parse(direction[j]));
                        break;
                    case 2:
                        time = new LongSongTime();
                        time.direction = (DIRECTION)(int.Parse(direction[j].Substring(0,1)));
                        string mode = direction[j].Substring(1, 1);
                        (time as LongSongTime).type = (LONG_PRESS_TYPE)(int.Parse(mode));
                        break;
                    case 3:
                        Debug.Log("这里估计有问题~~");
                        Debug.Break();
                        break;
                }
                list.Add(time);
                time.showTime = float.Parse(per[0]);
            }
        }

        List<SongTime> deleteList = new List<SongTime>();
        for (int i = 0, c = list.Count; i < c; i++)
        {
            LongSongTime lst = list[i] as LongSongTime;
            if (lst!=null && lst.type == LONG_PRESS_TYPE.START)
            {
                bool found = false;
                for (int j = i+1; j < c; j++)
                {
                    LongSongTime elst = list[j] as LongSongTime;
                    if (elst != null && elst.type == LONG_PRESS_TYPE.END && deleteList.IndexOf(elst)==-1 && elst.direction == lst.direction )
                    {
                        float timeDiff = elst.showTime - lst.showTime;
                        lst.PressTime = timeDiff;
                        deleteList.Add(elst);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Debug.LogWarning("没有结束箭头。请检查数据~~");                    
                }
            }
        }

        for (int i = 0, c = deleteList.Count; i < c; i++)
        {
            list.Remove(deleteList[i]);
        }
        deleteList.Clear();
        deleteList = null;
        SongTime[] result = list.ToArray();
        return result;
    }

}

/// <summary>
/// 长按时间
/// </summary>
public class LongSongTime: SongTime
{
    public SongInfo.LONG_PRESS_TYPE type = SongInfo.LONG_PRESS_TYPE.NONE;
    public float PressTime;
}

/// <summary>
/// 普通箭头时间
/// </summary>
public class SongTime
{
    public SongInfo.DIRECTION direction;    
    public float showTime;
    public  GameObject arrow;
}
#endregion
