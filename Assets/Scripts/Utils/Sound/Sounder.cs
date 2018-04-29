using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;
#endif

[ExecuteInEditMode]
public class Sounder : MonoBehaviour {
    public SoundInfo[] list;
    static private Sounder _instance=null;
    
    
    public enum FADE_MODE {
        NONE,
        FADE_IN,
        FADE_OUT
    };

    FADE_MODE mode = FADE_MODE.NONE;

    static public Sounder instance{
        get{
            if (_instance == null)
            {
                GameObject go = new GameObject();
                go.name = "Sounder";
                _instance= go.AddComponent<Sounder>();
                go.AddComponent<AudioSource>();
                go.AddComponent<AudioSource>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    void Awake()
    {        
        SounderData s = Resources.Load("SounderData") as SounderData;
        list = s.list;
    }

    void Update()
    {
        switch (mode)
        {
            case FADE_MODE.NONE:
                break;
            case FADE_MODE.FADE_IN:
                if (s != null && s[1].volume <1)
                {
                    s[1].volume += 0.05f;
                    if (s[1].volume >= 1)
                    {
                        mode = FADE_MODE.NONE;
                    }
                }
                break;
            case FADE_MODE.FADE_OUT:
                if (s != null && s[1].volume > 0)
                {
                    s[1].volume -= 0.05f;
                    if (s[1].volume <= 0)
                    {
                        mode = FADE_MODE.NONE;
                        StopBg();
                    }
                }
                break;
        }

        if (bgPlaying)
        {
            if (!s[1].isPlaying)
            {
                SetNextPlay(bgInfo, s[1],true);
            } 
        }
    }

    public void StopBg()
    {
        if (s == null) return;
        s[1].Stop();
        bgInfo = null;
        bgPlaying = false;
    }

    bool bgPlaying = false;

    SoundInfo bgInfo;
    AudioSource[] s;

    /// <summary>
    /// 如果isBG
    /// TRUE，会连续接着播放背景长音
    /// FALSE, 是音效
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isBG"></param>
    public void Play(string name,  bool isBG=false){        
        if (s == null) s = GetComponents<AudioSource>();
        int sIndex = isBG ? 1 : 0;        
        if (isBG)
        {
            mode = FADE_MODE.FADE_IN;
            s[sIndex].volume = 0;
            bgPlaying = true;
        }
        else
        {
            s[sIndex].ignoreListenerPause = true;
        }
        for (int i = 0; i < list.Length; i++)
        {
            SoundInfo info = list[i];
            if (info.name == name)
            {
                if (isBG) bgInfo = info;
                SetNextPlay(info, s[sIndex], isBG);
                return;
            }
        }
    }

    void SetNextPlay(SoundInfo info, AudioSource source, bool isBG)
    {
        AudioClip clip = null;        
        if (info.isRandom)
        {
            info.index = UnityEngine.Random.Range(0, info.clips.Length);
            clip = info.clips[info.index];
        }
        else
        {
            int v = info.index;
            info.index++;
            clip = info.clips[v];            
            if (info.index >= info.clips.Length)
            {
                info.index = 0;
            }
        }
        if (isBG)
        {
            source.clip = clip;
            source.Play();
        }
        else
        {
            source.PlayOneShot(clip);
        }        
    }

    public void FadeOut()
    {
        if (s == null) return;
        mode = FADE_MODE.FADE_OUT;
    }
}

[System.Serializable]
public class SoundInfo{
    public string name;
    public AudioClip[] clips;
    public int index = -1;
    public bool isRandom = false;
}




#region EDITOR
#if UNITY_EDITOR

public class SounderExtend :Editor{
     [MenuItem("GAME/声音/记录选择(覆盖)")]
    static public void CoverSounderRecordOption()
    {
        SetSounderRecords(false);
    }

    [MenuItem("GAME/声音/记录选择(添加)")]
     static public void AddSounderRecordOption()
    {
        SetSounderRecords(true);
    }

     static void SetSounderRecords(bool add)
     {
         string file = "/Resources/SounderData.asset";
         if (!Directory.Exists(Application.dataPath + "/Resources/"))
         {
             Directory.CreateDirectory(Application.dataPath + "/Resources/");
         }
         SounderData sd = null;
         if (!File.Exists(Application.dataPath + file))
         {
             sd = ScriptableObject.CreateInstance<SounderData>();
             AssetDatabase.CreateAsset(sd, "Assets" + file);
             AssetDatabase.SaveAssets();
         }
         else
         {
             sd = Resources.Load("SounderData") as SounderData;
         }


         object[] arr = Selection.GetFiltered(typeof(AudioClip), SelectionMode.DeepAssets);
         Dictionary<string, List<AudioClip>> dict = new Dictionary<string, List<AudioClip>>();

         for (int i = 0; i < arr.Length; i++)
         {
             if (!(arr[i] is AudioClip)) continue;
             string name = System.Text.RegularExpressions.Regex.Replace((arr[i] as AudioClip).name, @"\d", "");
             if (!dict.ContainsKey(name))
             {
                 dict.Add(name, new List<AudioClip>());
             }
             dict[name].Add((arr[i] as AudioClip));
         }


         List<SoundInfo> infos = new List<SoundInfo>();
         SoundInfo info;
         foreach (KeyValuePair<string, List<AudioClip>> data in dict)
         {
             data.Value.Sort(delegate(AudioClip a1, AudioClip a2)
             {
                 int i1 = 0, i2 = 0;
                 int.TryParse(Regex.Replace(a1.name, @"[^\d.\d]", ""), out i1);
                 int.TryParse(Regex.Replace(a2.name, @"[^\d.\d]", ""), out i2);
                 return i1.CompareTo(i2);
             });
             info = new SoundInfo();
             info.name = data.Key;
             info.clips = data.Value.ToArray();
             infos.Add(info);
         }

         if (infos.Count == 0)
         {
             Debug.LogError("请在库里选择音频文件。它们将被自动添加到Sounder的LIST里，保存好预制每个场景拖一个就KO了");
             return;
         }

         if (add)
         {
             if(sd.list!=null){
                 infos.AddRange(sd.list);
             }
         }
         var resultArr = infos.ToArray();
         Array.Reverse(resultArr);
         sd.list = resultArr;
         AssetDatabase.SaveAssets();
         Debug.Log("完成");
     }
}
#endif
#endregion