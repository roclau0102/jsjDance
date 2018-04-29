using UnityEngine;
using UnityEngine.Video;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using System;
#endif

public class BeatGame : BaseScene
{
    #region 变量
    public GameObject ready;
    public GameObject clear;
    public GameObject p2Close;
    public SongPlayer[] players;
    public tk2dTextMesh globalTime;
    public GameObject correntProp;
    public GameObject gameOverUI;
    public BlanketBuyTips buyTipsSprite;
    public XRDS_CheckHand xrdsCheckHand;
    //public RandomVideo video;
    public VideoPlayer video;

    [HideInInspector]
    public PLAY_STATE playState = PLAY_STATE.READY;
    public enum PLAY_STATE    {   READY,     PLAYING,  PAUSE,        GAME_OVER    }
    public Pause pauseUI;
    SongInfo songInfo;
    
    public AudioSource audioSource;
    bool readyEnd = false;
    static public BeatGame instance { get; private set; }
    float updateTime = 0;
    float gameOverTime = 0;



    #endregion

    
    

	// Use this for initialization
	void Awake () {

        if (Guide.instance != null)
        {
            Guide.instance.Kill();
        }

        instance = this;
        ready.SetActive(false);
        Global.init(() =>
        {
            Debug.Log("配置文件加载完毕。");

            Global.instance.DownloadSong(Global.MUSIC_TABLE[DataUtils.songDataID], true, (v, info) =>
            {
                if (v == 1)
                {
                    this.songInfo = info;
                    Invoke("RunGame", 0.2f);
                }                
            });
        });

        pauseUI.contiuneCall = SwitchPause;
        pauseUI.loadLevel = LoadLevel;

	    switch(DataUtils.mode){
            case Global.MODE.MODE_1P:
                players[1].gameObject.SetActive(false);
                players[0].SetLifeType(DataUtils.lifeType);
                p2Close.SetActive(true);
                correntProp.SetActive(true);                
                break;
            case Global.MODE.MODE_2P:
                players[0].SetLifeType(Global.LIFE_TYPE.LV1);
                players[1].SetLifeType(Global.LIFE_TYPE.LV1);

                players[1].gameObject.SetActive(true);
                p2Close.SetActive(false);
                correntProp.SetActive(false);
                break;
        }
        audioSource = GetComponent<AudioSource>();
        initedCamera = true;

        if (Application.platform != RuntimePlatform.Android)
        {
            //DataUtils.correntPropCount = 99999;
            players[0].SetMaxHP(99999);
            players[0].AddHP(99999);
        }
	}

    void OnDestroy()
    {
        AudioListener.pause = false;
        PrefabPool.clean();
        //if (Application.platform == RuntimePlatform.Android)
        //{
        //    if (Global.IsOverlayMode())
        //    {
        //        Global.CallAndroidStatic("StaticHideVideo");
        //    }
        //}
        Resources.UnloadUnusedAssets();    
    }


    void PlayVideo(bool play=true)
    {
        //if (Application.platform == RuntimePlatform.Android)
        //{
        //    if (Global.IsOverlayMode())
        //    {
        //        Global.CallAndroidStatic("StaticGamePlayVideo", DataUtils.songVideos);
        //        video.gameObject.SetActive(false);
        //    }
        //    else
        //    {
        //        video.gameObject.SetActive(true);
        //        video.LoopPlay(DataUtils.songVideosNoPath);
        //    }
        //}

        video.url = "file://" + DataUtils.songVideos + ".mp4";
        video.Play();
        Debug.Log("play video");
    }



    /// <summary>
    /// 开始游戏
    /// </summary>
    public void RunGame()
    {
        if (playState == PLAY_STATE.PAUSE)
        {
            playState = PLAY_STATE.PLAYING;
        }
        else
        {
            audioSource.clip = songInfo.songClip;
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                if (Version.currentPlatform.ToString().IndexOf("WX_XRDS") != -1) //旭日东升用自己的 checkhand
                {
                    xrdsCheckHand.doneCall = CheckHandCallback;
                    xrdsCheckHand.failedCall = () =>
                    {
                        syncDoAction = () =>
                        {
                            LoadLevel("SongList", false, false);
                        };
                    };
                    xrdsCheckHand.gameObject.SetActive(true);
                }
                else
                {
                    //不显示购买
                    if (Version.SHOW_BUY_TIPS)
                    {
                        CheckHand.doneCallback = CheckHandCallback;
                        CheckHand.failedCallback = () =>
                        {
                            syncDoAction = () =>
                            {
                                LoadLevel("SongList", false, false);
                            };
                        };
                        CheckHand.Show();
                    }
                    else
                    {
                        CheckHandCallback();
                        return;
                    }
                } 
            }
            else
            {
                CheckHandCallback();
            }
        } 
    }

   System.Action syncDoAction = null;

    void CheckHandCallback()
    {
        CameraFadeIn();
        Sounder.instance.FadeOut();
        ready.SetActive(true);
        SetReady();
        PlayVideo();
    }


    void SetReady()
    {
        if (DataUtils.mode == Global.MODE.MODE_2P)
        {
        }

        for (int i = 0; i < players.Length; i++)
        {
            players[i].index = i;
            players[i].StartPlay(songInfo, DataUtils.difficult);
            switch(i){
                case 0:
                    DataUtils.p1ScoreData = players[i].scoreData;
                    break;
                case 1:
                    DataUtils.p2ScoreData = players[i].scoreData;
                    break;
            }            
        }        
        audioSource.Play();
        playState = PLAY_STATE.PLAYING;
    }


    
	// Update is called once per frame
	void FixedUpdate () {

        if (syncDoAction != null)
        {
            syncDoAction();
            syncDoAction = null;
            return;
        }

        updateTime += Time.fixedDeltaTime;
        if (updateTime > 1)
        {
            ShowRestTime();
            updateTime = 0;
        }
        switch (playState)
        {
            case PLAY_STATE.GAME_OVER:
                if (gameOverTime == 0)
                {
                    gameOverTime = Time.time;
                }
                else if (Time.time - gameOverTime > 5)
                {
                    JumpNext();
                    gameOverTime = float.MaxValue;
                }
                break;
        }
	}

    private void GameOver(bool failed=false)
    {
        playState = PLAY_STATE.GAME_OVER;        

        if (failed)
        {
            gameOverUI.SetActive(true);
            Sounder.instance.Play("游戏失败音效");
        }
        else
        {
            clear.SetActive(true);
        }
    }

    internal void ArrowComplete()
    {        
        if(playState == PLAY_STATE.GAME_OVER)return;        
        Sounder.instance.Play("游戏完成音效"); 
        Sounder.instance.Play("观众欢呼声");

        if(DataUtils.mode == Global.MODE.MODE_1P && !DataUtils.isAutoMode){
            DataUtils.p1ScoreData.CountRightPercent(songInfo.data); //保存同步率
        }
        GameOver(false);
    }


    void JumpNext()
    {
        string levelName;
        if (DataUtils.mode == Global.MODE.MODE_1P && DataUtils.isAutoMode)
        {
            levelName = "SongList";
        }
        else
        {
            levelName = "Score";
        }
        DataUtils.correntPropCount = 0;
        DataUtils.lifeType = Global.LIFE_TYPE.LV1;
        LoadLevel(levelName);
    }

    protected override void LoadLevel(string name, bool goFront = true, bool needfadeOut=true)
    {
        base.LoadLevel(name, goFront, needfadeOut);
        Sounder.instance.Play("背景音乐", true);
    }
    
    

    /// <summary>
    /// 刷新时间
    /// </summary>
    void ShowRestTime()
    {
        if (audioSource.clip == null) return;
        int restSeconeds = (int)(this.audioSource.clip.length - this.audioSource.time);
        int min = Mathf.FloorToInt(restSeconeds / 60f);
        int sec = restSeconeds % 60;
        string minS = sec < 10 ? ("0" + sec) : sec.ToString();
        globalTime.text = min + ":" + minS;

        if (!readyEnd && (audioSource.clip.length - this.audioSource.time < 5))
        {
            readyEnd = true;
            PlayLast5SMovie();
        }
       
        /*
        if (restSeconeds==0)
        {
             GameOver();
        }*/
    }

    /// <summary>
    /// 播放最后五秒视频然后结束
    /// </summary>
    internal void PlayLast5SMovie()
    {
        //if (Global.IsOverlayMode())
        //{
        //    Global.CallAndroidStatic("StaticGameLastVideo");
        //}
        //else
        //{
        //    video.PlayLast5();
        //    //video.Play();
        //}
    }

    /// <summary>
    /// 玩家完了
    /// </summary>
    public void PlayerDead(SongPlayer p)
    {
                
        bool gameOver = true;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].alive)
            {
                gameOver = false;
            }
        }

        if (gameOver)
        {
            for (int i = 0; i < players.Length; i++)
            {
                players[i].lose.gameObject.SetActive(false);
                players[i].timingJudge.gameObject.SetActive(false);
            }
            GameOver(true);
            audioSource.Stop();
            PlayLast5SMovie();
        }
        else
        {
            Sounder.instance.Play("一人失败音效");
            p.lose.SetActive(true);
            p.timingJudge.gameObject.SetActive(false);
        }
    }

    internal void SwitchPause()
    {
        if (playState == PLAY_STATE.PLAYING)
        {
            playState = PLAY_STATE.PAUSE;
            if (!pauseUI.gameObject.activeSelf)
            {
                pauseUI.Show();
            }
            playState = PLAY_STATE.PAUSE;
            AudioListener.pause = true;
        }
        else if (playState == PLAY_STATE.PAUSE)
        {
            pauseUI.gameObject.SetActive(false);
            playState = PLAY_STATE.PLAYING;
            AudioListener.pause = false;
        }
    }

    float keyTime = 0;
    protected override void Move(int x, int y, BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (keyState == JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN && pauseUI.gameObject.activeSelf)
        {
            if ((Time.time - keyTime) < 0.3f) return;
            keyTime = Time.time;
            pauseUI.Move(x);
            Sounder.instance.Play("按键音效");
            if (pauseUI.gameObject.activeSelf == false)
            {
                SwitchPause();
            }
            return;
        }

        //测试版本
        if (type == INPUT_TYPE.JOYSTICK )
        {
            return;        
        }

        SongInfo.DIRECTION d = SongInfo.DIRECTION.NONE;
        if (x > 0)
        {
            d = SongInfo.DIRECTION.RIGHT;
        }
        else if (x < 0)
        {
            d = SongInfo.DIRECTION.LEFT;
        }
        else if (y < 0)
        {
            d = SongInfo.DIRECTION.UP;
        }
        else if (y > 0)
        {
            d = SongInfo.DIRECTION.DOWN;
        }
        if (d == SongInfo.DIRECTION.NONE) return;
        int index = (int)player;
        if (playState == PLAY_STATE.PLAYING)
        {
            players[index].PressKey(d, keyState);
        } 
    }

    

    protected override void PressEnter(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (playState == PLAY_STATE.GAME_OVER)
        {
            Sounder.instance.Play("按键音效");
            JumpNext();
        }
        else
        {
            if (pauseUI.gameObject.activeSelf)
            {
                pauseUI.Press();
                Sounder.instance.Play("返回按键");
            }
        }
    }



    protected override void Cancel(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        //if (type == INPUT_TYPE.BLANKET && Application.platform== RuntimePlatform.Android) return;
       
        Sounder.instance.Play("返回按键");
        if (playState == PLAY_STATE.GAME_OVER)
        {
            JumpNext();
        }
        else
        {
            SwitchPause();
        }
    }

    /// <summary>
    /// 为创维版本演示添加的方法，结束自动演示，返回主菜单
    /// </summary>
    public void BackToMain()
    {
        LoadLevel("Title");
    }
    
}


/*
#if UNITY_EDITOR
[CanEditMultipleObjects()]
[CustomEditor(typeof(BeatGame), true)]
public class GlobalExtension : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BeatGame g = target as BeatGame;
        if (g.gameObject.activeSelf==false) return;

        if (Application.isPlaying) {

            if (g.playState != BeatGame.PLAY_STATE.PLAYING) { 
                if (GUILayout.Button("开始游戏", GUILayout.Height(50)))
                {
                    g.RunGame();
                }
            }
            else
            {
                if (GUILayout.Button("暂时游戏", GUILayout.Height(50)))
                {
                    BeatGame.instance.SwitchPause();
                }
            }
        }
    }
}
#endif
*/