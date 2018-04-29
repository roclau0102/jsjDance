using UnityEngine;
using System.Collections;

public class PersonalInfoPanel : WXPanelBasic
{
    public tk2dUITextInput userTextInput;
    public tk2dUITextInput pwdTextInput;
    public TextMesh wxMoneyText;
    public tk2dSlicedSprite okBtn;
    public tk2dSlicedSprite resetBtn;


	// Use this for initialization
	void Start () {
        maxIndex = 4;
        
	}
	
	// Update is called once per frame
	void Update () {
        if (refreshText)
        {
            refreshText = false;
            wxMoneyText.text = "外星币：" + WXProtocol.Money;
            userTextInput.Text = WXProtocol.userName.ToString();
            pwdTextInput.Text = WXProtocol.userPwd.ToString();
        }
	}

    public override void Init(DANCE_DATA data)
    {
        base.Init(data);
        refreshText = true;
    }

    bool refreshText = false;

    void InitData()
    {
        refreshText = true;        
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
            if (curInput == pwdTextInput && curInput.Text.Length > 12) return;
            v += value;
        }
        curInput.Text = v;
    }


    tk2dUITextInput curInput;
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
                okBtn.SetSprite("longBtn");;
                
                resetBtn.SetSprite("longBtn");;
                break;
            case 1:
                userTextInput.selectedStateGO.SetActive(false);
                pwdTextInput.selectedStateGO.SetActive(true);
                okBtn.SetSprite("longBtn");;
                resetBtn.SetSprite("longBtn");;
                break;
            case 2:
                userTextInput.selectedStateGO.SetActive(false);
                pwdTextInput.selectedStateGO.SetActive(false);
                okBtn.SetSprite("longBtn2");;
                resetBtn.SetSprite("longBtn");;
                break;
            case 3:
                userTextInput.selectedStateGO.SetActive(false);
                pwdTextInput.selectedStateGO.SetActive(false);
                okBtn.SetSprite("longBtn");;
                resetBtn.SetSprite("longBtn2");;
                break;
        }
    }

    public override void Hide()
    {
        base.Hide();        
    }


    public override void Cancel()
    {
        base.Cancel();
    }

   
    public override void Enter()
    {
        switch (focusIndex)
        {
            case 0:
                showKeyboardCall(7,false);
                curInput = userTextInput;
                break;
            case 1:
                showKeyboardCall(6.17f, false);
                curInput = pwdTextInput;
                break;
            case 2:
                if (userTextInput.Text != WXProtocol.userName || pwdTextInput.Text!= WXProtocol.userPwd)
                {
                    if (userTextInput.Text == "")
                    {
                        WXLoginInfoPanel.instance.Tips("请输入用户名");
                        return;
                    }

                    if (pwdTextInput.Text.Length < 6 || pwdTextInput.Text.Length>12)
                    {
                        WXLoginInfoPanel.instance.Tips("密码请设置为6-12位");
                        return;
                    }

                    loadingCall(true);
                    WXProtocol.instance.login.ChangeUserAndPwd(userTextInput.Text, pwdTextInput.Text, (d) =>
                    {
                        loadingCall(false);
                        if (d.success)
                        {
                            WXLoginInfoPanel.instance.Tips("修改用户名密码成功！");
                        }
                        else
                        {
                            WXLoginInfoPanel.instance.Tips(d.reason);
                        }
                    });
                }
                else
                {
                    WXLoginInfoPanel.instance.Tips("你可以修改用户名密码后按OK键更改");
                }
                break;
            case 3:
                refreshText = true;
                break;
        }
    }
}
