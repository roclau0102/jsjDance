using UnityEngine;
using System.Collections;
using System;


/// <summary>
/// 2016/1/15　更改逻辑为只要显示就代表不是我司手柄．按任意键直接退出
/// 2016/2/20   添加手柄逻辑.为兼容红外手柄,弹图时按下任意键也要能进游戏.
/// </summary>
public class CheckHand : MonoBehaviour
{

    #region 需要设置的参数
    /// <summary>
    /// 返回回调
    /// </summary>
    static public Action failedCallback;

    /// <summary>
    /// 成功回调
    /// </summary>
    static public Action doneCallback;

    /// <summary>
    /// 是否可以打开链接
    /// </summary>
    //static bool canOpenLink = false;

    /// <summary>
    /// 是否直接测试失败
    /// </summary>
    public bool testFailed = false;
    /// <summary>
    /// 强制关闭
    /// </summary>
    public bool forceClose = false;

    #endregion
    #region 静静的变态量
    static bool isWX = false;

    static int devicecheck = -1;
    static int test_VendorId;
    static bool test_checkFlag;
    static bool mode_flag;
    static public CHECK_STAET state = CHECK_STAET.CHECKING;
    #endregion

    #region 变量
    int width;
    int height;

    static bool keepChecking = true;
    static public bool show = false;
    

    int joy_key2;
    int joy_key3;
    float testtime;

    float startTime;
    #endregion

#if UNITY_ANDROID
    static private AndroidJavaObject jo;
#endif



    public enum CHECK_STAET
    {
        CHECKING,
        CHECK_FAILED,
        WAIT_FOR_BLANKET_KEY,
        DONE
    }

    public static CheckHand instance;


    public enum TYPE
    {
        JOYSTICK,
        BLANKET
    }

    
    public TYPE type = TYPE.BLANKET;


    void Awake()
    {

        Debug.Log("CHECK HAND,AWAKE");

        gameObject.SetActive(false);
    #if UNITY_ANDROID
            if (Application.platform != RuntimePlatform.Android) return;

            jo = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");

            Debug.Log("CHECK HAND,type:" + type + ",isWX:" + isWX);
            if (type == TYPE.BLANKET)
            {
                //跳舞毯
                if (isWX)
                {
                    //外星版
                    mode_flag = jo.Get<bool>("mode_flag");
                    //Debug.Log("CHECK HAND, mode_flag:" + mode_flag);
                    if (mode_flag)
                    {   //外星带驱动版本则禁用脚本。
                        state = CHECK_STAET.WAIT_FOR_BLANKET_KEY;
                        show = true;
                        return;
                    }
                }
            }
            else
            {
                if (isWX) //手柄版本的WX平台不需要弹购买图
                {
                    state = CHECK_STAET.DONE;
                    show = false;
                }
            }

            devicecheck = jo.Get<int>("check_device");
            test_VendorId = jo.Get<int>("Test_VendorId");
            test_checkFlag = jo.Get<bool>("test_flag");

            Debug.Log("CHECK HAND, devicecheck:" + devicecheck + ",test_VendorId" + test_VendorId +
                               ",test_checkFlag:" + test_checkFlag);
            if (test_checkFlag)
            {
                if (test_VendorId != 0x1bcf)
                {
                    //Vid ！= 0x1bcf，肯定不是我司的手柄
                    state = CHECK_STAET.CHECK_FAILED;
                    show = true;
                    keepChecking = false;
                }
                else if (devicecheck == 0)
                {
                    jo.Call("DeviceCheck"); //重新调用一次
                    Debug.Log("CHECK HAND:DeviceCheck");
                }
            }
            else
            {
                //Test_checkFlag == false ，说明最初jo2.Call("DeviceCheck");调用失败，这里重新调用    	
                jo.Call("DeviceCheck");
                test_VendorId = jo.Get<int>("Test_VendorId");
                Debug.Log("CHECK HAND:DeviceCheck, test:" + test_VendorId);
                if (test_VendorId != 0x1bcf)
                {
                    //Vid ！= 0x1bcf，肯定不是我司的手柄
                    show = true;
                    keepChecking = false;
                    state = CHECK_STAET.CHECK_FAILED;
                    Debug.Log("CHECK HAND FAILED: NOT MINE JOYSTICK");
                }
            }
    #endif
             gameObject.SetActive(false);
    }

    Texture2D texture;

    /// <summary>
    /// 初始化需要吊用一下．
    /// @param goLinkWhenEnter 是否按确定时弹网页
    /// </summary>
    static public void Init(TYPE showType, Texture2D showTexture, bool wx)
    {  
        isWX = wx;
        //canOpenLink = isWX;

        if (instance == null)
        {
            GameObject go = new GameObject();
            go.name = "CheckHand";
            DontDestroyOnLoad(go);
            instance = go.AddComponent<CheckHand>();
        }
        Debug.Log("showTexture:"+showTexture);
        instance.Init(showType, showTexture);
    }

    private void Init(TYPE showType, Texture2D showTexture)
    {
        this.type = showType;
        this.texture = showTexture;
    }

    internal static void Show()
    {
        if (state == CHECK_STAET.DONE)
        {
            if (doneCallback != null) doneCallback();
        }
        else
        {
            instance.gameObject.SetActive(true);
        }
    }


    public static bool SUCCESS()
    {
        if (instance != null)
        {
            if (instance.gameObject.activeSelf == false) instance.gameObject.SetActive(true);
        }
        return state == CHECK_STAET.DONE;
    }


    public static bool Showing()
    {
        return instance != null && instance.gameObject.activeSelf;
    }




    void OnEnable()
    {
        width = Screen.width;
        height = Screen.height;
        mode_flag = true;
        keepChecking = true;
        testtime = 0;

        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (state != CHECK_STAET.CHECKING) return;
            //PC端的测试　
            //直接测试失败
            if (testFailed)
            {
                state = CHECK_STAET.CHECK_FAILED;
                show = true;
                Debug.Log("CHECK HAND TEST FAILED");
            }
            else
            {
                state = CHECK_STAET.WAIT_FOR_BLANKET_KEY;
                show = true;
            }
            return;
        }
        else if (Application.platform != RuntimePlatform.Android)
        {
            return;
        }
    }


    float checkTime = 0;



    void Update()
    {


#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
//            joy_key0 = jo.Get<int>("key_code0");
  //          joy_key1 = jo.Get<int>("key_code1");
            joy_key2 = jo.Get<int>("key_code2");
            joy_key3 = jo.Get<int>("key_code3");
            devicecheck = jo.Get<int>("check_device");
        }
#endif

        if (Time.time - checkTime > 1)
        {
            Debug.Log(state);
            checkTime = Time.time;
        }
        
        switch (state)
        {
            case CHECK_STAET.CHECKING:
                if (keepChecking)
                {
                    if (devicecheck == 1)
                    {
                        state = CHECK_STAET.WAIT_FOR_BLANKET_KEY;
                        keepChecking = false;
                        show = true;
                    }
                    else
                    {
                        if (testtime > 10f)
                        { //超过１0秒,超时
                            show = true;
                            keepChecking = false;
                            state = CHECK_STAET.CHECK_FAILED;
                            Debug.Log("CHECK HAND TIMEOUT");
                        }
                        else
                            testtime += Time.deltaTime;
                    }
                }
                break;
            case CHECK_STAET.WAIT_FOR_BLANKET_KEY:
                if (type == TYPE.JOYSTICK)
                {
                    //为了防止红外手柄木有办法玩游戏
                    //手柄的游戏跳出购买图以后，用户按了任意键还是要进游戏, 那还做个毛线检查,直接都能玩就得了
                    if (Input.anyKeyDown)
                    {
                        state = CHECK_STAET.DONE;
                    }
                }
                else
                {
                    if (joy_key2 != 0 || joy_key3 != 0 || forceClose)
                    {
                        state = CHECK_STAET.DONE;
                        show = false;
                    }
                    else
                    {
                        CheckKey();
                    }
                }
                break;
            case CHECK_STAET.CHECK_FAILED:
                if (type == TYPE.JOYSTICK)
                {
                    //为了防止红外手柄木有办法玩游戏
                    //手柄的游戏跳出购买图以后，用户按了任意键还是要进游戏, 那还做个毛线检查,直接都能玩就得了
                    if (Input.anyKeyDown)
                    {
                        state = CHECK_STAET.DONE;
                    }
                }
                else
                {
                    CheckKey();
                }
                break;
            case CHECK_STAET.DONE:
                if (doneCallback != null)
                {
                    doneCallback();
                    gameObject.SetActive(false);
                }
                break;
        }
    }


    /// <summary>
    /// ONGUI逻辑
    /// </summary>
    void OnGUI()
    {
        GUI.depth = -1000;
        if (show)
        {
            GUI.DrawTexture(new Rect(222 * width / 1280, 120 * height / 800, 835 * width / 1280, 557 * height / 800), texture);
        }
        //GUILayout.Label(canOpenLink + "," + joy_key0 + "," + joy_key1);
    }

    /// <summary>
    /// 确定键跳链接,返回键返回上个场景
    /// </summary>
    void CheckKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (failedCallback != null)failedCallback();
            gameObject.SetActive(false);
            return;
        }/*
        else if (canOpenLink)
        {
            if ((joy_key0 & 0x10) != 0 || (joy_key1 & 0x10) != 0 || Input.GetKeyDown(KeyCode.Return))
            {
                CheckHand.OpenURL();
            }
        }*/
    }



    /// <summary>
    ///  打开淘宝的ＵＲＬ
    /// </summary>
    internal static void OpenURL()
    {
        Application.OpenURL("http://detail.tmall.com/item.htm?id=15097926519");
    }


}