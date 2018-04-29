using UnityEngine;
using System.Collections;
using System;

public class CW : MonoBehaviour
{
    static CW _instance;
    public static CW Instance
    {
        get { return _instance; }
    }

    /*     
     开始-》轮播广告（广告在开场动画开始几秒后开始，第一张广告从下往上升起来）
     * 广告的话如果用户没有操作就一直轮播
     * 
     * 标题界面-》用户有操作就弹出二维码图
     * 用户没操作就进入待机动画-》
     * 
     * 待机动画 -> 购买图，
     * 如果用户按返回键或者没操作10几秒后进入标题界面，如果用户按确定键，则弹出大二维码图
     * -》标题界面，等待几秒后进入待机动画     
     */


    public enum STATE { 轮播广告中, 广告中按确定等待, 自动演示中 }
    public STATE state = STATE.轮播广告中;
    public GameObject[] loopTipsGOs;
    public GameObject ewm;
    public bool canShowEMW = true;
    static bool canPlayCWIdle = true;
    public bool autoPlaying = false;
    bool showingTips = false;
    TimeChecker adTimer = new TimeChecker(4);
    TimeChecker waitUserOperateTime = new TimeChecker(10);
    TimeChecker refreshStateTimer = new TimeChecker(.5f);
    public TimeChecker autoPlayTimer = new TimeChecker(30);
    int adIndex = 0;
    AndroidJavaObject activity;
    ICW_AttachScene scene;

    public Version.PLAFTFORM_ENUM version;

    public int usbState = 4;

    void Awake()
    {

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    AndroidJavaObject jo;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            var androidJC = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            jo = androidJC.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    void FindScene()
    {
        GameObject go = GameObject.FindGameObjectWithTag("SCENE") as GameObject;

        //Debug.Log("Tag SCENE NAME: " + go.name);

        if (go != null)
        {
            MonoBehaviour[] cs = go.GetComponents<MonoBehaviour>();
            ;
            for (int i = 0; i < cs.Length; i++)
            {
                if (cs[i] is ICW_AttachScene)
                {
                    scene = cs[i] as ICW_AttachScene;

                    SetController(false);

                }
            }
        }
    }


    public void SetState(STATE s)
    {
        this.state = s;

        switch (state)
        {
            case STATE.自动演示中:
                canPlayCWIdle = false;
                //SetState(STATE.自动演示中);
                showingTips = true;
                break;

            case STATE.轮播广告中:
                DataUtils.isAutoMode = false;
                DataUtils.runingAutoMode = false;
                break;

        }

        //Log("切换到状态:" + s);
    }


    void RefreshState()
    {
        if (Application.platform != RuntimePlatform.Android) return;
        if (activity == null)
            activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                        .GetStatic<AndroidJavaObject>("currentActivity");
        usbState = activity.Get<int>("usbhid_state");
    }

    //call from android
    public void OnIdleComplete(string unuseParams)
    {
        showingTips = false;
        if (scene != null) scene.SetCanCancle(true);
        SetState(STATE.轮播广告中);
    }


    void SetTips(int showIndex)
    {
        for (int i = 0; i < loopTipsGOs.Length; i++)
        {
            loopTipsGOs[i].SetActive(i == showIndex);
        }
    }

    void SetController(bool b)
    {
        if (scene != null) scene.SetCanEnter(b);
    }


    string prevSceneName;
    public string playIdleVideoScene = "Title";

    void Update()
    {
        if (Application.loadedLevelName != prevSceneName)
        {
            prevSceneName = Application.loadedLevelName;
            FindScene();
        }


        if (refreshStateTimer.CheckTimeout())
        {
            refreshStateTimer.SaveTime();
            RefreshState();
            if (usbState == 4)
            {
                if (scene != null) scene.SetCanEnter(true);
                if (scene != null) scene.SetCanCancle(true);
                SetTips(-1);
                SetController(true);
                Destroy(gameObject);
                Debug.Log("destroy");
                return;
            }
        }
        int key = 0;

        if (Application.platform == RuntimePlatform.Android)
        {
            key = jo.Get<int>("myKeyCode");
        }


        switch (state)
        {
            case STATE.轮播广告中:

                if (playIdleVideoScene == Application.loadedLevelName)
                {
                    //if (!waitUserOperateTime.endable)
                    //{
                    //    waitUserOperateTime.SaveTime();
                    //    waitUserOperateTime.endable = true;
                    //}


                    //if (waitUserOperateTime.CheckTimeout())
                    //{
                    //    waitUserOperateTime.endable = false;

                    //    canPlayCWIdle = false;
                    //    SetState(STATE.自动演示中);
                    //    showingTips = true;

                    //    switch (Application.platform)
                    //    {
                    //        case RuntimePlatform.WindowsEditor:
                    //            Invoke("TestPlay", 3);
                    //            break;
                    //        case RuntimePlatform.Android:
                    //            var jc = new AndroidJavaClass("com.jms.cwTips");
                    //            jc.CallStatic("Call", jo);
                    //            break;
                    //    }
                    //    break;
                    //}
                }

                if (/*key != 0 &&*/ (scene == null || (scene != null && scene.CanShowEMW())))
                {
                    if (/*key != 4*/Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
                    {
                        t.SaveTime();
                        if (scene != null) scene.SetCanCancle(false);
                        if (scene != null) scene.SetCanEnter(false);
                        SetTips(-1);
                        SetState(STATE.广告中按确定等待);
                        ewm.SetActive(true);
                        break;
                    }
                }

                if (adTimer.CheckTimeout())
                {
                    adTimer.SaveTime();
                    SetTips(adIndex);
                    adIndex++;
                    if (adIndex > (loopTipsGOs.Length - 1))
                    {
                        adIndex = 0;
                    }
                }
                break;
            case STATE.广告中按确定等待:
                if (/*key == 4*/Input.GetKeyDown(KeyCode.Escape))
                {
                    Log("广告中按确定等待按下确定");
                    ewm.SetActive(false);
                    SetState(STATE.轮播广告中);
                    adIndex = 0;
                    Invoke("SetCanCancle", .5f);
                    waitUserOperateTime.SaveTime();
                }
                break;
            case STATE.自动演示中:
                //nothing 2 do;
                if (scene != null) scene.SetCanCancle(false);

                if (Input.anyKeyDown || autoPlayTimer.CheckTimeout())
                {
                    autoPlayTimer.SaveTime();
                    autoPlayTimer.endable = false;

                    //演示时间到，返回主菜单界面
                    Debug.Log("演示结束");
                    if (Application.loadedLevelName == "SongList")
                    {
                        SongListMain.instance.BackToMain();
                    }
                    else if (Application.loadedLevelName == "Video")
                    {
                        BeatGame.instance.BackToMain();
                    }
                    waitUserOperateTime.SaveTime();

                    SetState(STATE.轮播广告中);
                }

                if (adTimer.CheckTimeout())
                {
                    adTimer.SaveTime();
                    SetTips(adIndex);
                    adIndex++;
                    if (adIndex > (loopTipsGOs.Length - 1))
                    {
                        adIndex = 0;
                    }
                }
                break;
        }
    }

    void Log(string s)
    {
        Debug.Log("[" + Time.time + "]" + s);
    }

    TimeChecker t = new TimeChecker(.1f);

    void SetCanCancle()
    {
        if (scene != null) scene.SetCanCancle(true);
    }

    void TestPlay()
    {
        OnIdleComplete("DONE");
    }
}


public interface ICW_AttachScene
{
    void SetCanEnter(bool b);
    void SetCanCancle(bool b);

    bool CanShowEMW();
}


public class TimeChecker
{

    [SerializeField]
    float time;

    [SerializeField]
    float startTime = int.MinValue;


    public bool endable = true;

    /// <summary>
    /// 是否不受TimeScale影响
    /// </summary>
    public bool isNoTimeScale = false;

    public TimeChecker(float time)
    {
        this.time = time;
    }

    public void SetWaitTime(float time)
    {
        this.time = time;
        startTime = isNoTimeScale ? Time.realtimeSinceStartup : Time.time;
    }


    public bool CheckTimeout()
    {
        return ((isNoTimeScale ? Time.realtimeSinceStartup : Time.time) - startTime) > time;
    }

    public float GetPercent()
    {
        float p = ((isNoTimeScale ? Time.realtimeSinceStartup : Time.time) - startTime) / time;
        if (p > 1) p = 1;
        return p;
    }

    internal void SaveTime(float time = 0)
    {
        startTime = time == 0 ? (isNoTimeScale ? Time.realtimeSinceStartup : Time.time) : time;
    }

    internal float GetRestTime()
    {
        float t = time - ((isNoTimeScale ? Time.realtimeSinceStartup : Time.time) - startTime);
        if (t < 0) t = 0;
        return t;
    }

    /// <summary>
    /// 得到已经经过的时间
    /// </summary>
    /// <returns></returns>
    public float GetRanTime()
    {
        return (isNoTimeScale ? Time.realtimeSinceStartup : Time.time) - startTime;
    }

    internal float GetWaitTime()
    {
        return time;
    }
}
