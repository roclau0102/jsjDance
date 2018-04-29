using UnityEngine;
using System.Collections;
using System;

public class SongCard : MonoBehaviour {

    public TextMesh text;
    public tk2dSprite bar;
    public GameObject hover;
    public GameObject loadingBar;
    public tk2dSprite bg;
    public tk2dSprite buyIocn;

	// Use this for initialization
	void Start () {
        loadingBar.SetActive(false);
        CheckBuy();
	}

    public void CheckBuy()
    {
        if (data == null) return;
        if (data.local)
        {
            buyIocn.gameObject.SetActive(false);
            return;
        }

        bool isDownload = Global.instance.CheckMp3Download(data);
        if (isDownload)
        {
            buyIocn.gameObject.SetActive(false);
        }
        else
        {
            bool b = DataUtils.GetSongBuy(data);
            buyIocn.gameObject.SetActive(!b);
        }        
        bar.scale = new Vector3(0, 1, 1);
    }

    
    static public Action cancelCallback;

    public void Cancel()
    {
        if (infos == null) return;
        for (int i = 0; i < infos.Length; i++)
        {
            JLoader.instance.Remove(infos[i]);
        }
        loadingBar.SetActive(false);
        isDownloaded = false;
        cancelCallback = null;  
    }


    static public bool IsDownloading()
    {
        return cancelCallback != null;
    }

    JLoaderInfo[] infos;

    public void Press(System.Action<bool> success)
    {
        if (!Global.instance.CheckMp3Download(data))
        {
            loadingBar.SetActive(true);
            bar.scale = new Vector3(0, 1, 1);

            cancelCallback = Cancel;

            infos = Global.instance.DownloadSong(data, false, (v, info) =>
            {
                if (v != -1)
                {
                    bar.scale = new Vector3(48.5f * v, 1, 1);
                    if (v == 1)
                    {
                        SongCard.cancelCallback = null;
                        loadingBar.SetActive(false);
                        isDownloaded = true;
                        RefreshBg();
                        bar.scale = new Vector3(48.5f, 1, 1);
                        if (success != null)
                        {
                            success(true);
                        }                        
                        infos = null;
                    }
                }
                else
                {
                    SongCard.cancelCallback = null;
                    infos = null;
                    loadingBar.SetActive(false);
                    success(false);
                    SongListMain.instance.Alert("无网络或者空间不足。");
                }
            });
        }
        else
        {
            success(true);
        }

    }


    bool isHover = false;

    public void SetHover(bool b)
    {
        if (isHover != b)
        {
            isHover = b;
        }
        else
        {
            return;
        }

        if (!isDownloaded) { 
            b=false;
        }

        if (isDownloaded)
        {
            GetComponent<Animation>().Stop();
            GetComponent<Animation>().Play(b ? "SongCardZoomIn" : "SongCardZoomOut");            
        }
    }

    public SongData data{get;private set;}

    public void SetData(SongData sd)
    {
        data = sd;
        text.text = sd.cnName;
        SetDownload(Global.instance.CheckMp3Download(data));
    }


    internal void RefreshBg()
    {
        SetDownload(isDownloaded);
    }


    public bool isDownloaded = false;
    public byte[] coverBytes;
    public bool resetPositionNow=false;
    


    void OnDestroy()
    {
        coverBytes = null;
    }


    public void SetDownload(bool b)
    {
        isDownloaded = b;
        if (b)
        {
            switch (DataUtils.difficult)
            {
                case SongInfo.DIFF_LEVEL.EASY:
                    bg.SetSprite("歌名条简单");
                    break;
                case SongInfo.DIFF_LEVEL.HARD:
                    bg.SetSprite("歌名条困难");
                    break;
                case SongInfo.DIFF_LEVEL.MID:
                    bg.SetSprite("歌名条普通");
                    break;
            }
        }
        else
        {
            bg.SetSprite("歌名条下载");
        }
        CheckBuy();
        if (isDownloaded)
        {
            if(isHover){
                GetComponent<Animation>().Stop();
                GetComponent<Animation>().Play( "SongCardZoomIn" );
            }
            
        }
    }



    public int index { get {
        return data.index;
    } }

    
}
