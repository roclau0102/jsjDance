using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwitchUserPanel : WXPanelBasic
{

    public tk2dUITextInput userTextInput;
    public tk2dUITextInput pwdTextInput;
    public tk2dSlicedSprite okBtn;
    public tk2dSlicedSprite resetBtn;

    public System.Action<Dictionary<string,object>> switchComplete;
    private tk2dUITextInput curInput;

	// Use this for initialization
	void Start () {
        //userTextInput.Text = "test4";
        //pwdTextInput.Text = "test";
	}
	
	// Update is called once per frame
	void Update () {
	
	}


    public override void Enter()
    {
        base.Enter();
        switch (focusIndex)
        {
            case 0:
                curInput = userTextInput;
                showKeyboardCall(7, false);
                break;
            case 1:
                curInput = pwdTextInput;
                showKeyboardCall(6.17f, false);
                break;
            case 2:
                loadingCall(true);
                if (userTextInput.Text == "" && pwdTextInput.Text == "")
                {
                    WXProtocol.instance.login.SwitchUser(WXProtocol.macUID, (data) =>
                    {
                        if (data.success)
                        {
                            switchComplete(data.data);
                            WXLoginInfoPanel.instance.Tips("切换用户成功！");
                        }
                        else
                        {
                            WXLoginInfoPanel.instance.Tips(data.reason);
                        }
                        loadingCall(false);
                    });
                }else if (userTextInput.Text != "")
                {                   
                    WXProtocol.instance.login.SwitchUser(userTextInput.Text, pwdTextInput.Text, (data) =>
                    {
                        if (data.success)
                        {
                            switchComplete(data.data);
                            WXLoginInfoPanel.instance.Tips("切换用户成功！");
                            userTextInput.Text = "" ;
                            pwdTextInput.Text = "";
                        }
                        else
                        {
                            WXLoginInfoPanel.instance.Tips(data.reason);
                        }
                        loadingCall(false);
                    });
                }
                else
                {
                    WXLoginInfoPanel.instance.Tips("请填写用户名及密码");
                }
                break;
            case 3:
                pwdTextInput.Text = userTextInput.Text = "";
                break;
        }
        
    }

    public override void KeyboardInput(string value)
    {
        string v = curInput.Text;
        if (value == "删除")
        {
            if (v.Length > 0)
            {
                v = v.Substring(0, v.Length - 1);
            }
        }
        else
        {
            v += value;
        }
        curInput.Text = v;
    }

//    Color gray = new Color(0.8f, 0.8f, 0.8f);

    public override void Move(int x, int y)
    {
        base.Move(x, y);
        userTextInput.SetFocus(false);
        pwdTextInput.SetFocus(false);

        switch (focusIndex)
        {
            case 0:
                userTextInput.selectedStateGO.SetActive(true);
                pwdTextInput.selectedStateGO.SetActive(false);
                okBtn.SetSprite("longBtn");
                resetBtn.SetSprite("longBtn");
                break;
            case 1:
                userTextInput.selectedStateGO.SetActive(false);
                pwdTextInput.selectedStateGO.SetActive(true);
                okBtn.SetSprite("longBtn");
                resetBtn.SetSprite("longBtn");
                break;
            case 2:
                userTextInput.selectedStateGO.SetActive(false);
                pwdTextInput.selectedStateGO.SetActive(false);
                okBtn.SetSprite("longBtn2");
                resetBtn.SetSprite("longBtn");
                break;
            case 3:
                userTextInput.selectedStateGO.SetActive(false);
                pwdTextInput.selectedStateGO.SetActive(false);
                okBtn.SetSprite("longBtn");
                resetBtn.SetSprite("longBtn2");
                break;
        }
    }

    public override void Hide()
    {
        userTextInput.SetFocus(false);
        pwdTextInput.SetFocus(false);
    }
}
