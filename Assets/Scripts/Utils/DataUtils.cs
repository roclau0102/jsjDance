using UnityEngine;
using System.Collections;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DataUtils : MonoBehaviour {

    static DataUtils _instance;

    static public Action<int> ChangeMoneyCallback;
    public static int SONG_MONEY = 100;

    //=========================选择数据=======================================
    /// <summary>
    /// 视频地址，2,3,6,7 自带，其他需要加载
    /// </summary>                
    static public string songVideos
    {
        get
        {
            if (mode == Global.MODE.MODE_2P)
            {
                int[] ids = new int[] { 2, 3, 6, 7 };
                return Global.AppContentPath() + ids[UnityEngine.Random.Range(0, ids.Length)];
            }
            return Global.AppContentPath() +  characterID;
        }
    }


    static public string songVideosNoPath
    {
        get
        {
            if (mode == Global.MODE.MODE_2P)
            {
                int[] ids = new int[] { 2, 3, 6, 7 };
                return  ids[UnityEngine.Random.Range(0, ids.Length)].ToString();
            }
            return  characterID.ToString();
        }
    }

    /// <summary>
    /// 生命级别
    /// </summary>
    static public Global.LIFE_TYPE lifeType = Global.LIFE_TYPE.LV1;

    /// <summary>
    /// 纠正道具数量
    /// </summary>
    static public int correntPropCount = 0;

    /// <summary>
    /// 玩家模式
    /// </summary>
    static public Global.MODE mode{
         get{
             return (Global.MODE)PlayerPrefs.GetInt("gameMode", (int)Global.MODE.MODE_1P);
         }
        set
        {
            PlayerPrefs.SetInt("gameMode",  (int)value);
            PlayerPrefs.Save();
        }
    }


    static public void RandomCharacter()
    {
        System.Collections.Generic.List<int> random = new System.Collections.Generic.List<int>();
        foreach (System.Collections.Generic.KeyValuePair<int, CharacterData> v in Global.CHARACTER_TABLE)
        {
            switch (v.Value.id)
            {
                case 2:
                case 3:
                case 6:
                case 7:
                    random.Add(v.Value.id);
                    break;
                default:
                    if( Global.instance.CheckVideoDownload(v.Value) && DataUtils.GetCharacterIsUnLock(v.Value.id)){
                        random.Add(v.Value.id);
                    }
                    break;
            }
        }
        DataUtils.characterID = random[UnityEngine.Random.Range(0, random.Count)];
    }
        

    /// <summary>
    /// 从1开始,玩家角色
    /// </summary>
    static public int characterID
    {
        get
        {
            return PlayerPrefs.GetInt("characterID", 2);//默认是2
        }
        set
        {
            PlayerPrefs.SetInt("characterID", value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 歌曲难度
    /// </summary>
    static public SongInfo.DIFF_LEVEL difficult
    {
        get
        {
            return (SongInfo.DIFF_LEVEL)PlayerPrefs.GetInt("songDifficult", 1);
        }
        set
        {
            PlayerPrefs.SetInt("songDifficult", (int)value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 歌曲ID
    /// </summary>
    static public int songDataID
    {
        get
        {
            return PlayerPrefs.GetInt("songDataID",1);
        }
        set
        {
            PlayerPrefs.SetInt("songDataID", value);
            PlayerPrefs.Save();
        }
    }

   

    /// <summary>
    /// 是否是自动模式
    /// </summary>
    static public bool isAutoMode = false;
    //=========================选择数据=======================================

    static public bool runingAutoMode = false;



    static public PlayerScoreData p1ScoreData;
    static public PlayerScoreData p2ScoreData;



    static public DataUtils instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject();
                go.name = "DataUtils"; 
                _instance = go.AddComponent<DataUtils>();
            }
            return _instance;
        }
    }


    /// <summary>
    /// 同步率
    /// </summary>
    /// <param name="sd"></param>
    /// <param name="diff"></param>
    /// <returns></returns>
    static public float GetMusicRightPercent(SongData sd, SongInfo.DIFF_LEVEL diff)
    {
        return PlayerPrefs.GetFloat("song-" + sd.id+ "-" + diff, 0) ;
    }

    /// <summary>
    /// 保存同步率
    /// </summary>
    /// <param name="sd"></param>
    /// <param name="diff"></param>
    /// <param name="p"></param>
    static public void SaveMusicRightPercent(SongData sd, SongInfo.DIFF_LEVEL diff, float p)
    {
        p = ((int)(p * 1000f)) / 10f;
        PlayerPrefs.SetFloat("song-" + sd.id + "-" + diff, p);
    }


    /// <summary>
    /// 得到分数
    /// </summary>
    /// <returns></returns>
    static public int GetScore()
    {
        return PlayerPrefs.GetInt("Score", 0);
    }

    

    /// <summary>
    /// 加分
    /// </summary>
    /// <param name="c"></param>
    static public void AddScore(int c)
    {
        long longV = GetScore() + c;
        int v = GetScore() + c;
        if (longV > int.MaxValue)
        {
            v = int.MaxValue;
        }
        else
        {
            v = (int)longV;
        }        
        PlayerPrefs.SetInt("Score", v);
        PlayerPrefs.Save();        
    }


    /// <summary>
    /// 减钱
    /// </summary>
    /// <returns></returns>
    static public int GetMoney()
    {
        int v= PlayerPrefs.GetInt("Money", 0);
        return v;
    }


    /// <summary>
    /// 加钱
    /// </summary>
    /// <param name="c"></param>
    static public void AddMoney(int c)
    {
        int v = GetMoney() + c;
        PlayerPrefs.SetInt("Money", v);
        PlayerPrefs.Save();
        if (ChangeMoneyCallback != null)
        {
            ChangeMoneyCallback(v);
        }
    }

    static internal bool GetSongBuy(SongData s)
    {
        if (s == null)
        {
            return false;
        }
        return s.price==0 || PlayerPrefs.GetInt(s.id + "_buy", 0) == 1;
    }

    static internal void SetSongBuy(SongData data)
    {
        PlayerPrefs.SetInt(data.id + "_buy", 1) ;
        PlayerPrefs.Save();
    }

    static internal bool GetCharacterIsUnLock(int i)
    {
        return PlayerPrefs.GetInt(i + "_charcter", 0) ==1;
    }

    static internal void SetCharacterIsUnLock(int i)
    {
        PlayerPrefs.SetInt(i + "_charcter", 1);
        PlayerPrefs.Save();
    }

    static public void CleanUserData()
    {
        for (int i = 0; i < 100; i++)
        {
            if (PlayerPrefs.GetInt(i + "_buy", 0) == 1)
            {
                PlayerPrefs.SetInt(i + "_buy", 0);
            }
            if (PlayerPrefs.GetInt(i + "_charcter", 0) == 1)
            {
                PlayerPrefs.SetInt(i + "_charcter", 0);
            }                        
        }
        DataUtils.AddMoney(-DataUtils.GetMoney());
    }

    internal static int AddLoginTime()
    {
        int v = PlayerPrefs.GetInt("LoginTime", 0);
        v++;
        PlayerPrefs.SetInt("LoginTime", v);
        return v;
    }
}

