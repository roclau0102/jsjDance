using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/// <summary>
/// 手柄按键类
/// 2015.9.17
/// 增加了鼠标控制类，可以控制鼠标。但需要使用SetResolution设置分辨率来得到绝对坐标。或者使用相对偏移值来移动
/// </summary>
public class JoystickManager : MonoBehaviour {

    #region 需要设置的变量
    /// <summary>
    /// 按下事件
    /// </summary>    
    public event KeyDelegate KeyEvent;

   // [Tooltip("检查底层变量，及按键区别，自动设置手柄类别")]
    public bool autoCheckJoyStickType = true;

    /// <summary>
    /// 瑜珈毯/跳舞毯事件
    /// </summary>
    public event BlanketKeyDelegate BlanketEvent;

    /// <summary>
    /// 使用跳舞毯,如果为true,那么跳舞毯/手柄事件也会有响应
    /// </summary>
   // [Tooltip(" 使用跳舞毯,如果为true,那么跳舞毯/手柄事件也会有响应")]
    public bool isUseBlanket = true;

    /// <summary>
    /// 因为手柄（1P/2P）和跳舞毯的按钮过多，无法同时间使用，在需要使用键盘测试跳舞毯时，请实时改变这个值为true，否则为false
    /// </summary>
    //[Tooltip(" 因为手柄（1P/2P）和跳舞毯的按钮过多，无法同时间使用，在需要使用键盘测试跳舞毯时，请实时改变这个值为true，否则为false")]
    public bool isTestKeyboard4Blanket = false;


    /// <summary>
    /// 调试方式，可以使用键盘(电脑)或者触摸来测试（手鸡)
    /// </summary>
    public DEBUG_CONTROL_TYPE debugControlType = DEBUG_CONTROL_TYPE.KEYBOARD;

    /// <summary>
    /// 是否合并左右脚按键
    /// </summary>
   // [Tooltip("是否合并小霸王的左右脚，如80 81,230 231")]
    public bool isCombineFootKey = true;

    /// <summary>
    /// 撸动事件
    /// </summary>
    public event WavingGDelegate WavingEvent;

    /// <summary>
    /// 鼠标
    /// </summary>
    //[Tooltip("鼠标")]
    public Mouse mouse = new Mouse();


    /// <summary>
    /// 1p撸次， 多撸伤身
    /// </summary>
    static public int p1WavingTime = 0;

    /// <summary>
    /// 2p撸次， 多撸伤身
    /// </summary>    
    static public int p2WavingTime = 0;

    

    /// <summary>
    /// 是否发送按住事件
    /// </summary>
    //[Tooltip("是否发送按住事件")]
    public bool isSendHoldingEvent = true;


    #endregion

    #region 变量

    public const string DIRECTION_KEY = "direction_key";
    public const string DIRECTION_KEY_2 = "direction_key_2";
    public const string POWER = "power_d";
    public const string POWER_2 = "power_d_2";
    public const string KEYCODE0 = "key_code0";
    public const string KEYCODE1 = "key_code1";
    public const string KEYCODE2 = "key_code2";
    public const string KEYCODE3 = "key_code3";

    public bool p1WavingComplete = false;
    public bool p2WavingComplete = false;

    /// <summary>
    /// 检查底层的手柄类别时间
    /// </summary>
    float liveControlCheckTime = 3;
    
    int liveControl = -1;
    
   // [Tooltip("底层传上来的手柄类别")]
    public float joy_type = 0;
    
    /// <summary>
    /// 防止重复生成单例
    /// </summary>
    static private bool destory = false;

    /// <summary>
    /// 单例
    /// </summary>
    static private JoystickManager _instance;
    static public JoystickManager instance {
        get {
            if (_instance == null)
            {
                if (!destory)
                {                
                    GameObject go = new GameObject();
                    _instance = go.AddComponent<JoystickManager>();
                    go.name = "JoystickManager";
                    DontDestroyOnLoad(go);                
                }
                else
                {
                    Debug.Log("JoystickManager has destroy, cant create one anymore.");
                }
            }
            return _instance;
        }
    }   

    /// <summary>
    /// 按键委托
    /// </summary>
    /// <param name="player"></param>
    /// <param name="key"></param>
    /// <param name="state"></param>
    /// <param name="joysickType"></param>
    public delegate void KeyDelegate(PLAYER_INDEX player, JOYSTICK_KEY key, JOYSTICK_KEY_STATE state, JOYSTICK_TYPE joysickType);
    /// <summary>
    /// 跳舞毯委托
    /// </summary>
    /// <param name="key"></param>
    /// <param name="state"></param>
    public delegate void BlanketKeyDelegate(BLANKET_NUMBER_KEY key, JOYSTICK_KEY_STATE state);
    /// <summary>
    /// 挥手手柄委托
    /// </summary>
    /// <param name="player"></param>
    /// <param name="d"></param>
    /// <param name="joysickType"></param>
    /// <param name="wavingComplete"></param>
    public delegate void WavingGDelegate(PLAYER_INDEX player, DirectionInfo d, JOYSTICK_TYPE joysickType, out bool wavingComplete);
    
    /// <summary>
    /// 方向信息
    /// </summary>
    protected DirectionInfo directionInfo = new DirectionInfo();

    /// <summary>
    /// P1按键状态
    /// </summary>
    protected Dictionary<JOYSTICK_KEY, JOYSTICK_KEY_STATE> P1_KEY_STATE = new Dictionary<JOYSTICK_KEY, JOYSTICK_KEY_STATE>();
    /// <summary>
    /// P2按键状态
    /// </summary>
    protected Dictionary<JOYSTICK_KEY, JOYSTICK_KEY_STATE> P2_KEY_STATE = new Dictionary<JOYSTICK_KEY, JOYSTICK_KEY_STATE>();
    /// <summary>
    /// 跳舞毯按键状态
    /// </summary>
    Dictionary<BLANKET_NUMBER_KEY, JOYSTICK_KEY_STATE> BLANKET_STATE = new Dictionary<BLANKET_NUMBER_KEY, JOYSTICK_KEY_STATE>();

    /// <summary>
    /// 其他手柄控制器
    /// </summary>
    Dictionary<JOYSTICK_TYPE, OTHER_CONTORLER> joystickControlers;
    /// <summary>
    /// 其他跳舞毯
    /// </summary>
    Dictionary<BLANKET_TYPE, OTHER_CONTORLER> blanketControlers;


#if UNITY_ANDROID
    private AndroidJavaClass jc;
    public AndroidJavaObject jo;
#endif
    /// <summary>
    /// 手柄类别
    /// </summary>
    public JOYSTICK_TYPE joystickType = JOYSTICK_TYPE.WHITE_DEFAULT;

    /// <summary>
    /// 跳舞毯类别
    /// </summary>
    public BLANKET_TYPE blanketType = BLANKET_TYPE.外星跳舞毯;

    #region 枚举起来
    /// <summary>
    /// 调试类别
    /// </summary>
    public enum DEBUG_CONTROL_TYPE {  NONE,  KEYBOARD,    TOUCH   }

    /// <summary>
    /// 玩家是几P
    /// </summary>
    public enum PLAYER_INDEX {  NONE = -1,   P1 = 0,    P2  }

    /// <summary>
    /// 方向
    /// </summary>
    public enum DIRECTION { NONE, LEFT = 0x10, RIGHT= 0x20, UP=0x01, DOWN=0x02}

    /// <summary>
    /// 手柄类别
    /// </summary>
    public enum JOYSTICK_TYPE {
        WHITE_DEFAULT,//默认白色
        BLACK_8MS,//黑色没感应器
        INFARED_RAY, //红外线        
        XBOX//XBOX
    }

    /// <summary>
    /// 跳舞毯类别
    /// </summary>
    public enum BLANKET_TYPE
    {
        外星跳舞毯,
        东升旭日,//一个不知名厂商的跳舞毯
    }

    /// <summary>
    /// 小霸王的贱位
    /// </summary>
    public enum JOYSTICK_KEY {
        KEY_UP = 1,
        KEY_DOWN = 2,
        KEY_LEFT = 4,
        KEY_RIGHT = 8,
        KEY_OK = 0x10, // 10000;
        KEY_A = 0x20,
        KEY_B = 0x100,
        KEY_C = 0x20000,
        KEY_X = 0x40,
        KEY_Y = 0x80,
        KEY_Z = 0x40000,
        KEY_J = 0x80000, //弹簧开关
        KEY_D = 0x400,
        KEY_BACK = 0x200,
        KEY_HOME = 0x800,
        KEY_MENU = 0x1000,
        KEY_VOLUME_UP = 0x2000,
        KEY_VOLUME_DOWN = 0x4000,        
    }

    /// <summary>
    /// 小霸王的瑜珈毯的数字键位 ,没有P1P2的概念,keycode不一样，分开写
    /// </summary>
    public enum BLANKET_NUMBER_KEY
    {
        KEY_1 = 0x00000001,
        KEY_3 = 0x00000002,
        KEY_5 = 0x00000004,
        KEY_6 = 0x00000008,
        KEY_80 = 0x00000010,
        KEY_81 = 0x00000020,
        KEY_10 = 0x00000040,
        KEY_11 = 0x00000080,
        KEY_13 = 0x00000100,
        KEY_15 = 0x00000200,
        KEY_16 = 0x00000400,
        KEY_18 = 0x00000800,
        KEY_20 = 0x00001000,
        KEY_21 = 0x00002000,
        KEY_230 = 0x00004000,
        KEY_231 = 0x00008000,
        KEY_25 = 0x00010000,
        KEY_26 = 0x00020000,
        KEY_28 = 0x00040000,
        KEY_30 = 0x00080000
    }    

    /// <summary>
    /// 瑜珈毯的玩家键位,2P玩家对战时可以使用,现暂时没有用到，如果需要可以从BLANKET_NUMBER_KEY强转一下。
    /// </summary>
    public enum BLANKET_KEY {
        //back
        P1_KEY_BACK = 0x00000001,
        P1_KEY_LEFT = 0x00000002,
        P1_KEY_5 = 0x00000004,
        P1_KEY_UP = 0x00000008,
        P1_KEY_A = 0x00000010,
        P1_KEY_B = 0x00000020,
        P1_KEY_DOWN = 0x00000040,
        P1_KEY_START = 0x00000080,
        P1_KEY_RIGHT = 0x00000100,
        P1_KEY_15 = 0x00000200,

        P2_KEY_BACK = 0x00000400,
        P2_KEY_LEFT = 0x00000800,
        P2_KEY_5 = 0x00001000,
        P2_KEY_UP = 0x00002000,
        P2_KEY_A = 0x00004000,
        P2_KEY_B = 0x00008000,
        P2_KEY_DOWN = 0x00010000,
        P2_KEY_START = 0x00020000,
        P2_KEY_RIGHT = 0x00040000,
        P2_KEY_15 = 0x00080000
    }

    /// <summary>
    /// 按键状态
    /// </summary>
    public enum JOYSTICK_KEY_STATE {
        NONE,
        KEY_DOWN,
        KEY_UP,
        KEY_HOLDING
    }
    #endregion

    #endregion

    #region 公共方法


    public INFARED_RAY GetInfaredRay()
    {
        return joystickControlers[JOYSTICK_TYPE.INFARED_RAY] as INFARED_RAY;
    }


    public int GetKeyPower(DIRECTION direction, PLAYER_INDEX p)
    {
        if (joystickType != JOYSTICK_TYPE.WHITE_DEFAULT) return 0;
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            return UnityEngine.Random.Range(1, 100);
        }

#if UNITY_ANDROID
        string after = "";
        if (p == PLAYER_INDEX.P2) after = "_2";
        switch (direction)
        {
            case DIRECTION.LEFT:
                return jo.Get<int>("power_l" + after);
            case DIRECTION.RIGHT:
                return jo.Get<int>("power_r" + after);
            case DIRECTION.UP:
                return jo.Get<int>("power_u" + after);
            case DIRECTION.DOWN:
                return jo.Get<int>("power_d" + after);
        }
#endif
        return 0;
    }



    /// <summary>
    /// 禁掉鼠标
    /// </summary>
    public void DisableMouse() {
        #if UNITY_ANDROID
        if (jo != null) jo.Call("Disable_mouse");
#endif
    }


    /// <summary>
    /// 得到撸次
    /// </summary>
    /// <returns></returns>
    public int GetWavingTime(PLAYER_INDEX p) {
        return p== PLAYER_INDEX.P1?p1WavingTime:p2WavingTime;
    }

    public void AddWavingTime(PLAYER_INDEX p)
    {
        if (p == PLAYER_INDEX.P1)
        {
            p1WavingTime++;
        }
        else
        {
            p2WavingTime++;
        }
    }


    /// <summary>
    /// 重新再撸
    /// </summary>
    static public void ResetWavingTime() {
        p1WavingTime = 0;
        p2WavingTime = 0;
    }

    /// <summary>
    /// 1P/2P是否按下某个键
    /// </summary>
    /// <param name="key"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool GetKey(PLAYER_INDEX p, JOYSTICK_KEY key, bool needHolding = true) {
        var d = p == PLAYER_INDEX.P1 ? P1_KEY_STATE : P2_KEY_STATE;
        if (d.ContainsKey(key)) {
            return needHolding ? (d[key] == JOYSTICK_KEY_STATE.KEY_HOLDING) : (d[key] == JOYSTICK_KEY_STATE.KEY_DOWN);
        }
        return false;
    }



    /// <summary>
    /// 跳舞毯是否按下某个键
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetKey(BLANKET_NUMBER_KEY key, bool needHolding = true) {
        if (BLANKET_STATE.ContainsKey(key)) {
            return needHolding ? (BLANKET_STATE[key] == JOYSTICK_KEY_STATE.KEY_HOLDING) : (BLANKET_STATE[key] == JOYSTICK_KEY_STATE.KEY_DOWN);
        }
        return false;
    }


    List<int> pressList = new List<int>();


    /// <summary>
    /// 获得踩下的键位,返回一个List<int>
    /// </summary>
    /// <returns></returns>
    public List<int> GetHoldingBlanket() { 
        pressList.Clear();
        int key = 0;
        foreach (KeyValuePair<BLANKET_NUMBER_KEY, JOYSTICK_KEY_STATE> item in BLANKET_STATE) {
            if (item.Value == JOYSTICK_KEY_STATE.KEY_HOLDING) {
                int.TryParse(item.Key.ToString().Replace("KEY_", ""), out key);
                //------------------------------------合KEY判断-----------------------------------------------------
                if (isCombineFootKey && (key == 80 || key == 81 || key==230 || key==231))
                {
                    if (key==80 || key==81) { 
                        key = 8;
                    }
                    else if (key == 230 || key == 231)
                    {
                        key = 23;
                    }
                    if (pressList.IndexOf(key)!=-1) continue;
                }
                //------------------------------------合KEY判断-----------------------------------------------------
                pressList.Add(key);
            }
        }
        return pressList;
    }

    /// <summary>
    /// 通过数字检测是否踩下某键
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public JOYSTICK_KEY_STATE GetBlanketKeyState(int v) {
        switch (v) {
            case 8:
                return GetBlanketKeyState(BLANKET_NUMBER_KEY.KEY_80);
            case 23:
                return GetBlanketKeyState(BLANKET_NUMBER_KEY.KEY_230);
            default:
                try {
                    BLANKET_NUMBER_KEY vv = (BLANKET_NUMBER_KEY)Enum.Parse(typeof(BLANKET_NUMBER_KEY), "KEY_" + v);
                    return GetBlanketKeyState(vv);
                }
                catch {
                    return JOYSTICK_KEY_STATE.NONE;
                }
        }
    }


    /// <summary>
    /// 根据KEY位返回按下或者弹起的状态
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public JOYSTICK_KEY_STATE GetBlanketKeyState(BLANKET_NUMBER_KEY key) {
        if (isCombineFootKey) {
            switch (key) {
                case BLANKET_NUMBER_KEY.KEY_80:
                case BLANKET_NUMBER_KEY.KEY_81:
                    return (BLANKET_STATE[BLANKET_NUMBER_KEY.KEY_81] == JOYSTICK_KEY_STATE.KEY_DOWN ||
                        BLANKET_STATE[BLANKET_NUMBER_KEY.KEY_80] == JOYSTICK_KEY_STATE.KEY_DOWN) ? JOYSTICK_KEY_STATE.KEY_DOWN : JOYSTICK_KEY_STATE.KEY_UP;
                case BLANKET_NUMBER_KEY.KEY_230:
                case BLANKET_NUMBER_KEY.KEY_231:
                    return (BLANKET_STATE[BLANKET_NUMBER_KEY.KEY_230] == JOYSTICK_KEY_STATE.KEY_DOWN ||
                        BLANKET_STATE[BLANKET_NUMBER_KEY.KEY_231] == JOYSTICK_KEY_STATE.KEY_DOWN) ? JOYSTICK_KEY_STATE.KEY_DOWN : JOYSTICK_KEY_STATE.KEY_UP;
            }
        }

        if (BLANKET_STATE.ContainsKey(key)) {
            return BLANKET_STATE[key];
        }
        return JOYSTICK_KEY_STATE.KEY_UP;
    }


    #endregion
    
    #region 系统事件

    void Awake() {        
        #if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.Call("GET_LINK_STATUS");            
        }
        #endif

        //添加手柄按键状态的字典
        foreach (JOYSTICK_KEY item in Enum.GetValues(typeof(JOYSTICK_KEY))) {
            P1_KEY_STATE.Add(item, JOYSTICK_KEY_STATE.KEY_UP);
            P2_KEY_STATE.Add(item, JOYSTICK_KEY_STATE.KEY_UP);
        }

        //添加跳舞毯按键状态的字典
        foreach (BLANKET_NUMBER_KEY item in Enum.GetValues(typeof(BLANKET_NUMBER_KEY))) {
                BLANKET_STATE.Add(item, JOYSTICK_KEY_STATE.KEY_UP);    
        }

        ///除了小霸王以外的控制类
        joystickControlers = new Dictionary<JOYSTICK_TYPE,OTHER_CONTORLER>{
            {JOYSTICK_TYPE.INFARED_RAY,  new INFARED_RAY()}            
        };

        //除了小霸王跳舞毯之外的控制类
        blanketControlers = new Dictionary<BLANKET_TYPE,OTHER_CONTORLER>{
            {BLANKET_TYPE.东升旭日,  new 东升旭日_CONTOLER()}            
        };
    }





    void OnDestroy()
    {
        destory = true;
        Debug.Log("JoystickManager Destroy");
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(gameObject);
        }
#endif
    }


    void OnGUI()
    {
        //其他控制器的更新
        if (joystickControlers.ContainsKey(joystickType))
        {
            joystickControlers[joystickType].Update();
        }

        //其他跳舞毯的更新
        if (isUseBlanket && blanketControlers.ContainsKey(blanketType))
        {
            blanketControlers[blanketType].Update();
        }
    }

    public bool isEnable = true;
    // Update is called once per frame
    void Update() {       

        if(mouse.enable)mouse.Update();
        #if UNITY_ANDROID            
            if (Application.platform == RuntimePlatform.Android) 
            {
                CheckLiveControl();
                CheckJoystickKey();
                CheckWaving();
                if (isUseBlanket)
                {
                    CheckBlanket();
                }

                //返回只返回1P.2P无视，否则会返回两次
               P1_KEY_STATE[JOYSTICK_KEY.KEY_BACK] =  GetKeyState( PLAYER_INDEX.P1, P1_KEY_STATE[JOYSTICK_KEY.KEY_BACK], Input.GetKey(KeyCode.Escape), JOYSTICK_KEY.KEY_BACK);
                
                //按下ＨＯＭＥ，或者ＭＥＮＵ，退出游戏
               if (P1_KEY_STATE[JOYSTICK_KEY.KEY_HOME] != JOYSTICK_KEY_STATE.KEY_UP ||
                  P2_KEY_STATE[JOYSTICK_KEY.KEY_HOME] != JOYSTICK_KEY_STATE.KEY_UP ||
                   Input.GetKeyDown(KeyCode.Menu))
               {
                   ExitGame();
               }
            }
    #endif
           
        //调试类别
        switch (debugControlType) {
            case DEBUG_CONTROL_TYPE.KEYBOARD:
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) CheckKeyboard();
                break;
            case DEBUG_CONTROL_TYPE.TOUCH:
                if (Application.platform == RuntimePlatform.Android) CheckTouch();
                break;
        }
    }


    /// <summary>
    /// 1P震动
    /// </summary>
    /// <param name="time"></param>
    void Vibrate_1(string time)
    {
        #if UNITY_ANDROID
        if(jo!=null)jo.Call("StartVibrate", "0", time);
        #endif
    }

    /// <summary>
    /// 2P震动
    /// </summary>
    /// <param name="time"></param>
    void Vibrate_2(String time)
    {
        #if UNITY_ANDROID
        if (jo != null) jo.Call("StartVibrate", "1", time);
        #endif
    }


    public void ExitGame()
    {
        #if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            new AndroidJavaObject("java.lang.System").CallStatic("exit", 0);
        }        
        #endif
    }


#if UNITY_ANDROID
    /*
    void OnApplicationPause(bool b) {
        if (Application.platform == RuntimePlatform.Android) {
            //ExitGame();            
            Debug.Log("游戏暂停:" + b);
        }
    }*/
#endif
    #endregion
    
    #region 输入逻辑

    /// <summary>
    /// 触摸只检测了挥动。随意加了一下，没有加上其他支持。
    /// </summary>
    private void CheckTouch() {
        if (WavingEvent == null) return;
        Touch curtouch = Input.GetTouch(0);
        if (curtouch.phase == TouchPhase.Moved) {
            Vector3 touchDeltaPosition = curtouch.deltaPosition;
            if (touchDeltaPosition.magnitude > 5) {
                if (touchDeltaPosition.x > 5) {
                    directionInfo.SetValue((int)DIRECTION.RIGHT, PLAYER_INDEX.P1); 
                    WavingEvent(PLAYER_INDEX.P1, directionInfo, joystickType, out p1WavingComplete);
                }
                else if (touchDeltaPosition.x < 5) {
                    directionInfo.SetValue((int)DIRECTION.LEFT, PLAYER_INDEX.P1);
                    WavingEvent(PLAYER_INDEX.P1, directionInfo, joystickType, out p1WavingComplete);
                }
                else {
                    directionInfo.SetValue((int)DIRECTION.NONE, PLAYER_INDEX.P1);
                    WavingEvent(PLAYER_INDEX.P1, directionInfo, joystickType, out p1WavingComplete);
                }
            }
        }
    }

    /// <summary>
    /// 往哪个方向撸
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    DIRECTION GetDirection(int key) {
        if ((key & 0x10) == 0x10) {
            return DIRECTION.LEFT;
        }
        else if ((key & 0x20) == 0x20) {
            return DIRECTION.RIGHT;
        }else if ((key & 0x01) == 1)
        {
            return DIRECTION.UP;
        }else if ((key & 0x02) == 2)
        {
            return DIRECTION.DOWN;
        }
        return DIRECTION.NONE;
    }

    /// <summary>
    /// 检查跳舞毯
    /// </summary>
    void CheckBlanket() {       
        int joy_key2 = GetKeycode(KEYCODE2);
        int joy_key3 = GetKeycode(KEYCODE3);
        joy_key2 = joy_key2 | joy_key3;

        int v;
        bool isDown = false;
        JOYSTICK_KEY_STATE state;
        JOYSTICK_KEY_STATE newState = JOYSTICK_KEY_STATE.NONE;

        foreach (BLANKET_NUMBER_KEY item in Enum.GetValues(typeof(BLANKET_NUMBER_KEY))) {
            state = BLANKET_STATE[item];
            v = (int)item;
            isDown = (joy_key2 & v) != 0;
            newState = JOYSTICK_KEY_STATE.KEY_UP;
            switch (state) {

                case JOYSTICK_KEY_STATE.KEY_UP:
                    if (isDown) {
                        if (BlanketEvent != null) BlanketEvent(item, JOYSTICK_KEY_STATE.KEY_DOWN);
                        newState = JOYSTICK_KEY_STATE.KEY_HOLDING;
                    }
                    break;
                case JOYSTICK_KEY_STATE.KEY_HOLDING:
                    if (!isDown) {
                        if (BlanketEvent != null) BlanketEvent(item, JOYSTICK_KEY_STATE.KEY_UP);
                        newState = JOYSTICK_KEY_STATE.KEY_UP;
                    }
                    else {
                        if (isSendHoldingEvent) {
                            if (BlanketEvent != null) BlanketEvent(item, JOYSTICK_KEY_STATE.KEY_HOLDING);
                        }
                        newState = JOYSTICK_KEY_STATE.KEY_HOLDING;
                    }
                    break;
            }
            if (newState != state) {
                BLANKET_STATE[item] = newState;
            }
        }
    }

    public void CallJavaFunction(string name)
    {
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android) { 
            jo.Call(name);
        }
#endif
    }

    
    /// <summary>
    /// 检测撸动,撸动时，力量数值持续变化，当回撸时被硬件清零。
    /// 这里做的是，当p1WareComplete的时候，不回调，检测power力为零时，重新检测是否撸动。
    /// </summary>
    void CheckWaving(bool fakeWaving = false, PLAYER_INDEX p = PLAYER_INDEX.NONE) {
        if (WavingEvent == null) return;

        int d1 = GetKeycode(DIRECTION_KEY);
        int d2 = GetKeycode(DIRECTION_KEY_2);
       

        if (fakeWaving) {
            switch (p) {
                case PLAYER_INDEX.P1:
                    d1 = (int)DIRECTION.DOWN;
                    break;
                case PLAYER_INDEX.P2:
                    d2 = (int)DIRECTION.DOWN;
                    break;
            }
        }


        if (d1 != 0)
        {
            directionInfo.SetValue(d1, PLAYER_INDEX.P1);
            WavingEvent(PLAYER_INDEX.P1, directionInfo, joystickType, out p1WavingComplete);
            if (p1WavingComplete)
            {
                CallJavaFunction("ResetP1Waving");
                p1WavingTime++;
            }
        }

        if (d2 != 0)
        {
            directionInfo.SetValue(d2, PLAYER_INDEX.P2);
            WavingEvent(PLAYER_INDEX.P2, directionInfo, joystickType, out p2WavingComplete);
            if (p2WavingComplete)
            {
                CallJavaFunction("ResetP2Waving");
                p2WavingTime++;
            }
        }
    }


    /// <summary>
    /// 检测抚摸
    /// </summary>
    private void CheckKeyboard() {
        if (!isTestKeyboard4Blanket) {
            KeyCode left = KeyCode.None;
            KeyCode right = KeyCode.None;
            KeyCode keyCode = KeyCode.None;
            for (int i = 0; i < 2; i++) {
                var states = i == 0 ? P1_KEY_STATE : P2_KEY_STATE;
                foreach (JOYSTICK_KEY item in Enum.GetValues(typeof(JOYSTICK_KEY))) {
                    var d = i == 0 ? KeyboardSetting.P1_JOYSTICK_KEY_TO_KEYBOARD : KeyboardSetting.P2_JOYSTICK_KEY_TO_KEYBOARD;
                    if (!d.ContainsKey(item)) continue;
                    KeyCode k = d[item];
                    if (Input.GetKeyDown(k)) {
                        if (KeyEvent!=null) KeyEvent((PLAYER_INDEX)i, item, JOYSTICK_KEY_STATE.KEY_DOWN,  joystickType);
                        states[item] = JOYSTICK_KEY_STATE.KEY_DOWN;
                    }
                    else if (Input.GetKeyUp(k)) {
                        if (KeyEvent != null) KeyEvent((PLAYER_INDEX)i, item, JOYSTICK_KEY_STATE.KEY_UP, joystickType);
                        states[item] = JOYSTICK_KEY_STATE.KEY_UP;
                    }
                    else if (Input.GetKey(k)) {
                        if (isSendHoldingEvent) {
                            if (KeyEvent != null) KeyEvent((PLAYER_INDEX)i, item, JOYSTICK_KEY_STATE.KEY_HOLDING, joystickType);
                            states[item] = JOYSTICK_KEY_STATE.KEY_HOLDING;
                        }
                    }
                    
                }
                keyCode = i == 0 ? ((KeyCode)KeyboardSetting.P1_ACTION_TO_KEYBOARD.WAVE) : ((KeyCode)KeyboardSetting.P2_ACTION_TO_KEYBOARD.WAVE);
                if (Input.GetKeyDown(keyCode)) {
                    DIRECTION d = DIRECTION.NONE;
                    left = i == 0 ? ((KeyCode)KeyboardSetting.P1_ACTION_TO_KEYBOARD.WAVE_LEFT) : ((KeyCode)KeyboardSetting.P2_ACTION_TO_KEYBOARD.WAVE_LEFT);
                    right = i == 0 ? ((KeyCode)KeyboardSetting.P1_ACTION_TO_KEYBOARD.WAVE_RIGHT) : ((KeyCode)KeyboardSetting.P2_ACTION_TO_KEYBOARD.WAVE_RIGHT);

                    if (Input.GetKey(left)) {
                        d = DIRECTION.LEFT;                        
                    }
                    else if (Input.GetKey(right)) {
                        d = DIRECTION.RIGHT;
                    }
                    directionInfo.SetValue((int)d, (PLAYER_INDEX)i);                    

                    if (i == 0) {
                        if (WavingEvent != null) WavingEvent((PLAYER_INDEX)i, directionInfo, joystickType, out p1WavingComplete);
                    }
                    else
                    {
                        if (WavingEvent != null) WavingEvent((PLAYER_INDEX)i, directionInfo, joystickType, out p1WavingComplete);
                    }
                }
            }
        }
        else {
            if (isUseBlanket && isTestKeyboard4Blanket) {
                KeyCode v = KeyCode.None;
                foreach (BLANKET_NUMBER_KEY item in Enum.GetValues(typeof(BLANKET_NUMBER_KEY))) {
                    v = KeyboardSetting.BLANKET_TO_KEYBOARD[item];
                    if (Input.GetKeyDown(v)) {
                        BLANKET_STATE[item] = JOYSTICK_KEY_STATE.KEY_DOWN;
                        if (BlanketEvent != null) BlanketEvent(item, JOYSTICK_KEY_STATE.KEY_DOWN);
                    }
                    else if (Input.GetKeyUp(v)) {
                        BLANKET_STATE[item] = JOYSTICK_KEY_STATE.KEY_UP;
                        if (BlanketEvent != null) BlanketEvent(item, JOYSTICK_KEY_STATE.KEY_UP);
                    }
                    else if (Input.GetKey(v)) {
                        BLANKET_STATE[item] = JOYSTICK_KEY_STATE.KEY_HOLDING;
                        if (isSendHoldingEvent && BlanketEvent != null) BlanketEvent(item, JOYSTICK_KEY_STATE.KEY_HOLDING);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Break();
            }
        }
    }


    /// <summary>
    /// 检测上左左右，ABAB
    /// </summary>
    void CheckJoystickKey() {
        Dictionary<JOYSTICK_KEY, JOYSTICK_KEY_STATE> d;
        int joyKey;
        JOYSTICK_KEY_STATE newState;
        JOYSTICK_KEY_STATE state;
        int value;
        bool isDown;

        for (int i = 0; i < 2; i++) {
            joyKey = GetKeycode(i==0?KEYCODE0:KEYCODE1);
            d = i == 0 ? P1_KEY_STATE : P2_KEY_STATE;

            foreach (JOYSTICK_KEY item in Enum.GetValues(typeof(JOYSTICK_KEY))) {
                if (item == JOYSTICK_KEY.KEY_BACK) continue;
                state = d[item];
                value = (int)item;
                isDown = (joyKey & value) != 0;
                newState = GetKeyState( (PLAYER_INDEX)i, state, isDown, item);
                if (newState != state) d[item] = newState;
            }
            if (joystickType == JOYSTICK_TYPE.INFARED_RAY) break;
            //红外１２Ｐ是一样的，所以区分不开,　当１Ｐ按下时，２Ｐ也会有反应．所以处理按钮事件的时候就不需要区分１，２Ｐ了
        }
    }

    /// <summary>
    /// 处理按钮状态及回调
    /// </summary>
    /// <param name="i"></param>
    /// <param name="state"></param>
    /// <param name="isDown"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    JOYSTICK_KEY_STATE GetKeyState(PLAYER_INDEX i, JOYSTICK_KEY_STATE state, bool isDown, JOYSTICK_KEY item)
    {
        JOYSTICK_KEY_STATE newState = JOYSTICK_KEY_STATE.KEY_UP;
        switch (state)
        {
            case JOYSTICK_KEY_STATE.KEY_UP:
                if (isDown)
                {
                    if (item == JOYSTICK_KEY.KEY_J) //key_j是黑色简化版手柄的挥动按键。如果是这个手柄的话，这里就直接回调挥动事件替代调按钮事件。
                    {
                        if (WavingEvent != null)
                        {
                            CheckWaving(true, i);
                        }
                        //j键按下，代表是黑色手柄
                        if (liveControl != 0 && autoCheckJoyStickType)
                        {
                            joystickType = JOYSTICK_TYPE.BLACK_8MS;
                        }
                    }
                    else
                    {
                        if (KeyEvent != null) KeyEvent(i, item, JOYSTICK_KEY_STATE.KEY_DOWN, joystickType);
                    }
                    newState = JOYSTICK_KEY_STATE.KEY_DOWN;
                }
                break;
            case JOYSTICK_KEY_STATE.KEY_DOWN:
                newState = JOYSTICK_KEY_STATE.KEY_HOLDING;
                if (isSendHoldingEvent && item != JOYSTICK_KEY.KEY_J && KeyEvent != null) KeyEvent(i, item, JOYSTICK_KEY_STATE.KEY_HOLDING, joystickType);
                break;
            case JOYSTICK_KEY_STATE.KEY_HOLDING:
                if (!isDown)
                {
                    if (item != JOYSTICK_KEY.KEY_J && KeyEvent != null) KeyEvent(i, item, JOYSTICK_KEY_STATE.KEY_UP, joystickType);
                    newState = JOYSTICK_KEY_STATE.KEY_UP;
                }
                else
                {
                    if (isSendHoldingEvent && item != JOYSTICK_KEY.KEY_J && KeyEvent != null) KeyEvent(i, item, JOYSTICK_KEY_STATE.KEY_HOLDING, joystickType);
                    newState = JOYSTICK_KEY_STATE.KEY_HOLDING;
                }
                break;
        }
        return newState;
    }

    
    /// <summary>
    /// 检查手柄参数.如果是0则使用红外来干，只需要检查外星的手柄
    /// </summary>
    private void CheckLiveControl()
    {
        if (!autoCheckJoyStickType) return;
         liveControlCheckTime +=Time.deltaTime;         
         int newLiveControl = liveControl;
         if(liveControlCheckTime >3)
         {
             liveControlCheckTime = 0;
#if UNITY_ANDROID
             if (Application.platform == RuntimePlatform.Android) {                 
                    jo.Call("GET_LINK_STATUS");
                    newLiveControl = jo.Get<int>("live_ctrl");
                    joy_type = jo.Get<int>("joy_type"); 
             }
#endif
         }
         if (newLiveControl != liveControl) //状态变化
         {
             liveControl = newLiveControl;
             switch(liveControl){
                 case 0: 
                     joystickType = JOYSTICK_TYPE.INFARED_RAY;
                     break;
                 default:
                     joystickType = JOYSTICK_TYPE.WHITE_DEFAULT;
                     break;
             }
         }
    }
    #endregion
    
    #region 各种兼容

    /// <summary>
    /// 通用ＫＥＹＣＯＤＥ检测
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
     public int GetKeycode(string name)
    {
        #if UNITY_ANDROID
            switch (name)
            {
                case KEYCODE2:
                case KEYCODE3:
                    //跳舞毯
                    switch (blanketType)
                    {
                        case BLANKET_TYPE.外星跳舞毯:
                            //不处理，下面会返回底层的值
                            break;
                        default:
                            if (blanketControlers.ContainsKey(blanketType))
                            {
                                int v = blanketControlers[blanketType].GetKeyCode(name);
                                if (v != -1)
                                {
                                    return v;
                                }
                            }
                            else
                            {
                                Debug.LogWarning("没有对应的跳舞毯处理器");
                            }
                            break;
                    }
                   
                    break;
                default:
                    //各种手柄值
                    if (joystickControlers.ContainsKey(joystickType)) //特殊手柄
                    {
                        var controler = joystickControlers[joystickType];
                        int keyCode = controler.GetKeyCode(name);
                        if (keyCode != -1)
                        {
                            return keyCode;
                        }
                    }
                    break;
            }

            //取底层值
            if (Application.platform == RuntimePlatform.Android)
            {
                return jo.Get<int>(name);
            }
            else
            {
                return 0;
            }        
        #else
        return 0;
        #endif
    }
    #endregion
}


#region 挥动类
/// <summary>
/// 挥动方向类
/// </summary>
public class OneDirectionInfo
{
    public JoystickManager.DIRECTION d;
    public int power;
}

/// <summary>
/// 方向信息
/// </summary>
public class DirectionInfo
{
    int v;
    OneDirectionInfo one = new OneDirectionInfo();
    JoystickManager.PLAYER_INDEX playerIndex;

    public void SetValue(int v, JoystickManager.PLAYER_INDEX playerIndex)
    {
        this.v = v;
        this.playerIndex = playerIndex;
        if (Application.platform == RuntimePlatform.WindowsEditor) //纯粹为了测试
        {
            foreach (JoystickManager.DIRECTION item in Enum.GetValues(typeof(JoystickManager.DIRECTION)))
            {
                if ((v & (int)item) != 0)
                {
                    one.d = item;
                    break;
                }
            }
        }
    }

    public bool IsCircle()
    {
        return IsRight() && IsLeft() && IsUp() && IsDown();
    }

 
    public OneDirectionInfo GetMaxPowerDirection()
    {
        one.power = -1;
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            one.power = 100; //强制设置成100
            return one;
        }
        foreach (JoystickManager.DIRECTION item in Enum.GetValues(typeof(JoystickManager.DIRECTION)))
        {
            if ((v & (int)item) != 0)
            {
                int p = JoystickManager.instance.GetKeyPower(item, playerIndex);
                 if (p > one.power)
                 {
                     one.power = p;
                     one.d = item;
                 }
            }
        }
       return one;
    }

    public bool IsLeft()
    {
        return (v & (int)JoystickManager.DIRECTION.LEFT) == (int)JoystickManager.DIRECTION.LEFT;
    }

    public bool IsRight()
    {
        return (v & (int)JoystickManager.DIRECTION.RIGHT) == (int)JoystickManager.DIRECTION.RIGHT;
    }

    public bool IsUp()
    {
        return (v & (int)JoystickManager.DIRECTION.UP) == (int)JoystickManager.DIRECTION.UP;
    }

    public bool IsDown()
    {
        return (v & (int)JoystickManager.DIRECTION.DOWN) == (int)JoystickManager.DIRECTION.DOWN;
    }    
}
#endregion

#region 其他手柄/跳舞毯类


/// <summary>
/// 其他控制器接口
/// </summary>
public interface OTHER_CONTORLER
{
    void Update();

    /// <summary>
    /// 返回－１代表不处理这个keycode
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    int GetKeyCode(string name);
}

public class 东升旭日_CONTOLER : OTHER_CONTORLER
{
//    Dictionary<JoystickManager.JOYSTICK_KEY, JoystickManager.JOYSTICK_KEY_STATE> state = new Dictionary<JoystickManager.JOYSTICK_KEY, JoystickManager.JOYSTICK_KEY_STATE>();
    static public Dictionary<int, JoystickManager.BLANKET_NUMBER_KEY> keyboard2Joystick = new Dictionary<int, JoystickManager.BLANKET_NUMBER_KEY>
    {       
        {330, JoystickManager.BLANKET_NUMBER_KEY.KEY_3},
        {331, JoystickManager.BLANKET_NUMBER_KEY.KEY_10},
        {332, JoystickManager.BLANKET_NUMBER_KEY.KEY_6},        
        {333, JoystickManager.BLANKET_NUMBER_KEY.KEY_13},        
        {334, JoystickManager.BLANKET_NUMBER_KEY.KEY_16},
        {335, JoystickManager.BLANKET_NUMBER_KEY.KEY_26},
        {338, JoystickManager.BLANKET_NUMBER_KEY.KEY_1},
        {339, JoystickManager.BLANKET_NUMBER_KEY.KEY_11},
        {342, JoystickManager.BLANKET_NUMBER_KEY.KEY_18},
        {343, JoystickManager.BLANKET_NUMBER_KEY.KEY_25},
        {344, JoystickManager.BLANKET_NUMBER_KEY.KEY_21},
        {345, JoystickManager.BLANKET_NUMBER_KEY.KEY_28}      
    };

    public void Update()
    {
        if (Event.current == null) return;
        int kc = (int)Event.current.keyCode;

        if (Event.current.isKey)
        {            
            if (Event.current.type == EventType.KeyDown)
            {                
                SetKey(kc, Event.current.type);
            }
            else if (Event.current.type == EventType.KeyUp)
            {
                SetKey(kc, Event.current.type);
            }
        }
    }

    int keyCode2;
    int keyCode3;

    private void SetKey(int kc, EventType type )
    {        
        if (keyboard2Joystick.ContainsKey(kc) == false) return;
        int bk = (int)keyboard2Joystick[kc];
        switch (kc)
        {
                case 338:
                case 339:
                case 332:
                case 330:
                case 333:
                case 331:                   
                    if(type == EventType.KeyDown){
                        keyCode2 = keyCode2 | bk;
                    }else{
                        keyCode2 = keyCode2 & (~bk);
                    }                    
                    break;
            case 334:
            case 335:
            case 344:
            case 343:
            case 342:
            case 345:
                    if (type == EventType.KeyDown){
                        keyCode3 = keyCode3 | bk;
                    }else{
                        keyCode3 = keyCode3 & (~bk);
                    } 
                    break;
            }
    }

    public int GetKeyCode(string name)
    {
        switch (name)
        {
            case JoystickManager.KEYCODE2:
                return keyCode2;
            case JoystickManager.KEYCODE3:
                return keyCode3;
        }
        return -1;
    }
}


public class INFARED_RAY : OTHER_CONTORLER
{

    Dictionary<JoystickManager.JOYSTICK_KEY, JoystickManager.JOYSTICK_KEY_STATE> state = new Dictionary<JoystickManager.JOYSTICK_KEY, JoystickManager.JOYSTICK_KEY_STATE>();
    static public Dictionary<JoystickManager.JOYSTICK_KEY, KeyCode> keyboard2Joystick = new Dictionary<JoystickManager.JOYSTICK_KEY, KeyCode>
    {
        {JoystickManager.JOYSTICK_KEY.KEY_UP, KeyCode.UpArrow},
        {JoystickManager.JOYSTICK_KEY.KEY_DOWN, KeyCode.DownArrow},
        {JoystickManager.JOYSTICK_KEY.KEY_LEFT, KeyCode.LeftArrow},
        {JoystickManager.JOYSTICK_KEY.KEY_RIGHT, KeyCode.RightArrow},        
        {JoystickManager.JOYSTICK_KEY.KEY_OK, KeyCode.Joystick1Button0},
        {JoystickManager.JOYSTICK_KEY.KEY_BACK, KeyCode.Escape},
        {JoystickManager.JOYSTICK_KEY.KEY_A, KeyCode.Menu},
        {JoystickManager.JOYSTICK_KEY.KEY_B, KeyCode.B},
        {JoystickManager.JOYSTICK_KEY.KEY_C, KeyCode.C},
        {JoystickManager.JOYSTICK_KEY.KEY_X, KeyCode.X},
        {JoystickManager.JOYSTICK_KEY.KEY_Z, KeyCode.Z},
        {JoystickManager.JOYSTICK_KEY.KEY_D, KeyCode.D}        
    };

    

    
    /// <summary>
    /// 红外线手柄
    /// </summary>
    public INFARED_RAY()
    {
        foreach (JoystickManager.JOYSTICK_KEY item in Enum.GetValues(typeof(JoystickManager.JOYSTICK_KEY)))
        {
            state.Add(item, JoystickManager.JOYSTICK_KEY_STATE.KEY_UP);
        }
    }

    public int keyCode0=0;
    public int keyCode1 = 0;
    public int direction0 = 0;
    public int direction1 = 0;
    bool enterFlag = false;
    /// <summary>
    /// 红外的回车是330可能和某些跳舞毯重叠了，设置这个变量为false可以不检查330这个键值
    /// </summary>
    public bool checkEventEnter = true;

    public void Update()
    {
        int key = 0;
        foreach (KeyValuePair<JoystickManager.JOYSTICK_KEY, KeyCode> item in keyboard2Joystick)
        {
            key = (int)item.Key;
            if (Input.GetKey(item.Value))
            {
                keyCode0 = keyCode0 | key;
                keyCode1 = keyCode1 | key;      
            }
            else
            {
                keyCode0 = keyCode0 & (~key);
                keyCode1 = keyCode1 & (~key);         
            }
        }

        if (!Input.GetKeyDown(keyboard2Joystick[ JoystickManager.JOYSTICK_KEY.KEY_BACK ] ))
        {
            key = (int)JoystickManager.JOYSTICK_KEY.KEY_BACK;
            if (Input.GetKey(KeyCode.Mouse1))
            {
                keyCode0 = keyCode0 | key;
                keyCode1 = keyCode0 | key;
            }
            else
            {
                keyCode0 = keyCode0 & (~key);
                keyCode1 = keyCode1 & (~key); 
            }
        }

        if (checkEventEnter)
        {
            if (Event.current.isKey)
            {
                if (Event.current.type == EventType.KeyDown)
                {
                    KeyCode k = Event.current.keyCode;
                    if ((k == (KeyCode)10) || (k == (KeyCode)330))
                        enterFlag = true;    //因为 Input.GetKey(KeyCode.Return)得不到正确的返回值，所以这边增加 enter_flag变量特殊处理。     
                }
                if (Event.current.type == EventType.KeyUp)
                {
                    KeyCode k = Event.current.keyCode;
                    if ((k == (KeyCode)10) || (k == (KeyCode)330))
                        enterFlag = false;
                    k = 0;
                }
            }          
        }
        

        if (enterFlag)   //因为KeyCode.Return读不到回车键，所以这边特别处理。
        {
            keyCode0 = keyCode0 | (int)JoystickManager.JOYSTICK_KEY.KEY_OK;
            keyCode1 = keyCode1 | (int)JoystickManager.JOYSTICK_KEY.KEY_OK;            
        }
        else
        {
            keyCode0 = keyCode0 & (~((int)JoystickManager.JOYSTICK_KEY.KEY_OK));
            keyCode1 = keyCode1 & (~((int)JoystickManager.JOYSTICK_KEY.KEY_OK));            
        }


        if (Input.GetKey(KeyCode.Alpha1))
        {
            if (direction0 == 0)
            {
                int i = UnityEngine.Random.Range(0, 2);
                
                if (i == 0)
                    direction0 |= 0x10;
                else
                    direction0 |= 0x20;
                i = UnityEngine.Random.Range(0, 2);
                if (i == 0)
                    direction0 |= 0x04;
            }
        }
        else
        {
            direction0 = 0;
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            if (direction1 == 0)
            {
                int i = UnityEngine.Random.Range(0, 2);
                if (i == 0)
                    direction1 |= 0x10;
                else
                    direction1 |= 0x20;
                i = UnityEngine.Random.Range(0, 2);
                if (i == 0)
                    direction1 |= 0x04;
            }
        }
        else
        {
            direction1 = 0;
        }
    }

    public int GetKeyCode(string name)
    {
        switch (name)
        {
            case JoystickManager.KEYCODE0:
                return keyCode0;
            case JoystickManager.KEYCODE1:
                return keyCode1;
            case JoystickManager.DIRECTION_KEY:
                return direction0;
            case JoystickManager.DIRECTION_KEY_2:
                return direction1;
        }
        //红外只处理12P的按钮和方向；
        return -1;
    }
}
 #endregion



#region 鼠标操作类

public class Mouse
{
    /// <summary>
    /// 1p每帧鼠标偏移值
    /// </summary>
    public Vector2 p1PosOffsetPerFrame = Vector2.zero;
    /// <summary>
    /// 2p每帧鼠标偏移值
    /// </summary>
    public Vector2 p2PosOffsetPerFrame = Vector2.zero;

    /// <summary>
    /// 1P绝对坐标
    /// </summary>
    public Vector2 p1Position = Vector2.zero;
    /// <summary>
    /// 2P绝对坐标
    /// </summary>
    public Vector2 p2Position = Vector2.zero;

    Vector2 resolution;

  //  [Tooltip("是否打开，默认是否")]
    public bool enable = false;

    public Mouse()
    {
        SetResolution(new Vector2(1920, 1080));
    }

    public void SetResolution(Vector2 v)
    {
        resolution = v;
        p1Position.Set(resolution.x / 2, resolution.y / 2);
        p2Position.Set(resolution.x / 2, resolution.y / 2);
    }

    public void ResetMouseOnCenter()
    {
        p1Position.Set(resolution.x / 2, resolution.y / 2);
        p2Position.Set(resolution.x / 2, resolution.y / 2);
    }

    float CheckGyroData(float v)
    {
        if (Math.Abs(v) < 5)
        {
            v = 0;
        }
        else
        {
            v = float.Parse( v.ToString("f3"));
        }
        return v;
    }

    public void Update()
    {
        if (JoystickManager.instance.joystickType == JoystickManager.JOYSTICK_TYPE.WHITE_DEFAULT)
        {
            float x = 0, y = 0;
            for (int i = 0; i < 2; i++)
            {
                JoystickManager.PLAYER_INDEX p = (JoystickManager.PLAYER_INDEX)i;                
                if (Application.platform == RuntimePlatform.Android)
                {
#if UNITY_ANDROID
                    /*int index = p == JoystickManager.PLAYER_INDEX.P1 ? 1 : 2;
                    x = JoystickManager.instance.GetKeycode("mouse_x_" + index + "p");
                    y = JoystickManager.instance.GetKeycode("mouse_y_" + index + "p");
                   JoystickManager.instance.jo.Call("cleanP" + index + "MousePosInfo");
                                                                              */
                    float zgyro =CheckGyroData( JoystickManager.instance.jo.Get<float>("z_gyro"));
                    float xgyro = CheckGyroData(JoystickManager.instance.jo.Get<float>("x_gyro"));

                    x = -zgyro*.4f;
                    y = xgyro*.4f;
#endif
                }
                else
                {
                    x = (int)(Input.GetAxisRaw("Horizontal")*5);
                    y = (int)(Input.GetAxisRaw("Vertical") * 5);
                }

                if(p == JoystickManager.PLAYER_INDEX.P1){
                    p1PosOffsetPerFrame.Set(x, y);
                    p1Position += p1PosOffsetPerFrame;                    
                    CheckBound(ref p1Position);
                }else{
                    p2PosOffsetPerFrame.Set(x, y);
                    p2Position += p2PosOffsetPerFrame;
                    CheckBound(ref p2Position);
                }
            }
        }
    }


    void CheckBound(ref Vector2 position)
    {
        if (position.x < 0)
        {
            position.x = 0;
        }
        else if (position.x > resolution.x)
        {
            position.x = resolution.x;
        }

        if (position.y < 0)
        {
            position.y = 0;
        }
        else if (position.y > resolution.y)
        {
            position.y = resolution.y;
        }
    }

}
#endregion