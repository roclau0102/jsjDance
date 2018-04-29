using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WXLoginInfoPanel : MonoBehaviour
{

    // Use this for initialization
    public PersonalInfoPanel personalPanel;
    public SwitchUserPanel switchUserPanel;
    public MoneyExchange exchangePanel;    
    public tk2dSprite personalTab;
    public tk2dSprite switchUserTab;
    public tk2dSprite exchangeTab;
    Dictionary<tk2dSprite, WXPanelBasic> panels = new Dictionary<tk2dSprite, WXPanelBasic>();
    WXPanelBasic curPanel;
    public GameObject loading;
    static public WXLoginInfoPanel instance;
    static bool isLogin = false;
    public TextMesh loadingText;
    public GameObject loginUI;    
    public System.Action nextAction;
    public Keyboard keyboard;
    public Keyboard numKeyboard;
    public bool showLoginDefault = false;
    public GameObject showBtn;

    /// <summary>
    /// 游戏数据
    /// </summary>
    static public DANCE_DATA data;


    /// <summary>
    /// 兑换钱时回调。游戏需要添加这个回调，不然兑换后加不了钱
    /// </summary>
    public Action<int> addGameMoneyCallback;

    void Awake()
    {
        panels.Add(personalTab, personalPanel);
        panels.Add(switchUserTab, switchUserPanel);
        panels.Add(exchangeTab, exchangePanel);

        instance = this;
        if (!isLogin)
        {
            loading.SetActive(true);
            WXProtocol.instance.Init("jsjdance", 50, 51, 52);
            if (WXProtocol.macUID != "noid")
            {
                WXProtocol.instance.login.Login((r) =>
                {
                    loading.SetActive(false);
                    if (r.success)
                    {
                        isLogin = true;
                        Tips("登陆成功！");
                        if (r.data != null)
                        {
                            data = DANCE_DATA.PARSE(r.data);
                        }
                        else
                        {
                            data = new DANCE_DATA();
                        }
                        personalPanel.Init(data);
                        exchangePanel.Init(data);
                    }
                    else
                    {
                        Debug.Log("登陆失败：" + r.reason);
                        if (r.reason != null)
                        {
                            Tips(r.reason);
                        }
                    }
                });
                loading.gameObject.SetActive(true);
            }            
        }
        else
        {
            personalPanel.Init(data);
            exchangePanel.Init(data);
            loading.gameObject.SetActive(false);
        }

        loginUI.gameObject.SetActive(showLoginDefault);
        keyboard.gameObject.SetActive(false);
        numKeyboard.gameObject.SetActive(false);

        foreach (var item in panels)
        {
            item.Value.loadingCall = ShowLoading;
            item.Value.showKeyboardCall = ShowKeyboard;
        }


       
        switchUserPanel.switchComplete = SwitchUserCopmlete;

        keyboard.OnPress = (str)=>{
            curPanel.KeyboardInput(str);
        };

        numKeyboard.OnPress = (str) =>{
            curPanel.KeyboardInput(str);
        };


        exchangePanel.exChangeComplete= ()=>{
            personalPanel.Init(data);
        };
        SwitchPanel(personalPanel);
        

#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = activity.GetStatic<AndroidJavaObject>("currentActivity");
        }
#endif
    }



    void ShowKeyboard(float y, bool isNum )
    {
        if (isNum)
        {
            numKeyboard.gameObject.SetActive(true);
            numKeyboard.Show();
        }
        else
        {
            keyboard.gameObject.SetActive(true);
            keyboard.Show();
        }        
    }


    void SwitchUserCopmlete(Dictionary<string,object>d)
    {
        if (data == null)
        {
            data = new DANCE_DATA();
        }
        else
        {
            data = DANCE_DATA.PARSE(d);
        }        
        personalPanel.Init(data);
        nextAction = () =>
        {
            SwitchPanel(personalPanel);
        };
    }


    void ShowLoading(bool b)
    {
        loading.SetActive(b);
    }

    void OnEnable()
    {
        JoystickManager.instance.KeyEvent += login_KeyEvent;
        JoystickManager.instance.BlanketEvent += instance_BlanketEvent;
    }

    void instance_BlanketEvent(JoystickManager.BLANKET_NUMBER_KEY key, JoystickManager.JOYSTICK_KEY_STATE state)
    {
        if (loginUI.activeSelf == false) return;
        if (state != JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN) return;

        if (keyboard.gameObject.activeSelf || numKeyboard.gameObject.activeSelf)
        {
            Keyboard k = keyboard.gameObject.activeSelf ? keyboard : numKeyboard;
            switch (key)
            {
                case JoystickManager.BLANKET_NUMBER_KEY.KEY_3:
                    k.Move(-1, 0);
                    break;
                case JoystickManager.BLANKET_NUMBER_KEY.KEY_13:
                    k.Move(1, 0);
                    break;
                case JoystickManager.BLANKET_NUMBER_KEY.KEY_6:
                    k.Move(0, -1);
                    break;
                case JoystickManager.BLANKET_NUMBER_KEY.KEY_10:
                    k.Move(0, 1);
                    break;
                case JoystickManager.BLANKET_NUMBER_KEY.KEY_11:
                    k.Enter();
                    break;                
                case JoystickManager.BLANKET_NUMBER_KEY.KEY_1:
                    if (Application.platform != RuntimePlatform.Android)
                    {
                        k.Cancel();
                    }
                    break;
            }
            return;
        }


        switch (key)
        {
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_3:                
                SwitchPanel(-1);
                break;
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_13:
                SwitchPanel(1);
                break;
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_6:
                curPanel.Move(0,-1);
                break;
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_10:
                curPanel.Move(0, 1);
                break;
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_11:
                curPanel.Enter();
                break;
            case JoystickManager.BLANKET_NUMBER_KEY.KEY_1:
                Invoke("Hide", 0.1f);
                break;
        }
    }

    void Hide()
    {
        loginUI.SetActive(false);
    }

    void OnDisable()
    {
        JoystickManager.instance.KeyEvent -= login_KeyEvent;
       JoystickManager.instance.BlanketEvent -= instance_BlanketEvent;
    }

    private void SwitchPanel(int offset)
    {
        tk2dSprite[] keys = new tk2dSprite[panels.Count];
        int index=0;
        foreach (var item in panels)
        {
            keys[index++] = item.Key;
        }

        
        for (int i = 0; i < keys.Length;i++ )
        {
            if (panels[keys[i]] == curPanel)
            {
                index = i + offset;
                if (index > keys.Length - 1)
                {
                    index = 0;
                }
                else if (index < 0)
                {
                    index = keys.Length - 1;
                }
                SwitchPanel(panels[keys[index]]);
                break;
            }
        }
    }

    private void SwitchPanel(WXPanelBasic panel)
    {
        if (curPanel == panel) return;

        foreach (var item in panels)
        {
            if (item.Value == panel)
            {
                item.Key.SetSprite("tag2");
                item.Value.Show();
                item.Value.gameObject.SetActive(true);
                curPanel = panel;
            }
            else
            {
                item.Value.Hide();
                item.Key.SetSprite("tag1");
                item.Value.gameObject.SetActive(false);
            }
        }
    }


    void login_KeyEvent(JoystickManager.PLAYER_INDEX player, JoystickManager.JOYSTICK_KEY key, JoystickManager.JOYSTICK_KEY_STATE state, JoystickManager.JOYSTICK_TYPE joysickType)
    {
        if (loginUI.activeSelf == false) return;
        if (state != JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN) return;
        if (keyboard.gameObject.activeSelf || numKeyboard.gameObject.activeSelf)
        {
            Keyboard k = keyboard.gameObject.activeSelf ? keyboard : numKeyboard;
            switch (key)
            {
                case JoystickManager.JOYSTICK_KEY.KEY_LEFT:
                    k.Move(-1, 0);
                    break;
                case JoystickManager.JOYSTICK_KEY.KEY_RIGHT:
                    k.Move(1, 0);
                    break;
                case JoystickManager.JOYSTICK_KEY.KEY_UP:
                    k.Move(0, -1);
                    break;
                case JoystickManager.JOYSTICK_KEY.KEY_DOWN:
                    k.Move(0, 1);
                    break;
                case JoystickManager.JOYSTICK_KEY.KEY_OK:
                    k.Enter();
                    break;
                case JoystickManager.JOYSTICK_KEY.KEY_BACK:
                    k.Cancel();
                    break;
            }
            return;
        }


        switch (key)
        {
            case JoystickManager.JOYSTICK_KEY.KEY_LEFT:                
                SwitchPanel(-1);
                break;
            case JoystickManager.JOYSTICK_KEY.KEY_RIGHT:
                SwitchPanel(1);
                break;
            case JoystickManager.JOYSTICK_KEY.KEY_UP:
                curPanel.Move(0, -1);
                break;
            case JoystickManager.JOYSTICK_KEY.KEY_DOWN:
                curPanel.Move(0, 1);
                break;
            case JoystickManager.JOYSTICK_KEY.KEY_OK:
                curPanel.Enter();
                break;
            case JoystickManager.JOYSTICK_KEY.KEY_BACK:
                Invoke("Hide", 0.1f);
                break;
        }
    }

       

    // Update is called once per frame
    void Update()
    {

        if (nextAction != null)
        {
            nextAction();
            nextAction = null;
        }
    }

    internal void Tips(string p)
    {        
        Debug.Log("[面板提示]"+p);
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            nextAction = () =>
            {
                currentActivity.CallStatic("StaticAlert", p);
            };
        }
#endif
    }

    internal void ShowLogin()
    {
        loginUI.gameObject.SetActive(true);
    }


#if UNITY_ANDROID
    static AndroidJavaObject currentActivity;

    public static void InitJO()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (currentActivity == null)
            {
                AndroidJavaClass activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                currentActivity = activity.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }
    }
#endif
	
}


public class DANCE_DATA
{
    int money = 0;
    int mark = 0;

    public int GetMoney()
    {
        return money;
    }

    public int GetMark()
    {
        return mark;
    }

    static public DANCE_DATA PARSE(Dictionary<string,object> data)
    {
        DANCE_DATA d = new DANCE_DATA();
        if (data.ContainsKey("money"))
        {
            d.money = int.Parse( data["money"].ToString());
        }

        if (data.ContainsKey("mark"))
        {
            d.mark = int.Parse(data["mark"].ToString());
        }
        return d;
    }

}

