using UnityEngine;
using System.Collections;
using System;

public class MoneyExchange : WXPanelBasic
{

    public tk2dUITextInput countTxt;
    public tk2dSlicedSprite okBtn;
    public TextMesh nowMoneyText;
    public Action exChangeComplete;

   // Color gray = new Color(0.8f, 0.8f, 0.8f);

	// Use this for initialization
	void Start () {
        maxIndex = 2;
	}

    public override void Move(int x, int y)
    {
        base.Move(x, y);
        


        switch (focusIndex)
        {
            case 0:
                countTxt.selectedStateGO.SetActive(true);
                okBtn.SetSprite("longBtn");                
                break;
            case 1:
                countTxt.selectedStateGO.SetActive(false);
                okBtn.SetSprite("longBtn2");
                break;
        }
    }

    public override void Init(DANCE_DATA data)
    {
        base.Init(data);
        nowMoneyText.text = "您最多可以兑换" +  WXProtocol.Money + "游戏币";
    }

    public override void KeyboardInput(string value)
    {
        string v = countTxt.Text;
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
        countTxt.Text = v;
    }

    public override void Enter()
    {
        switch (focusIndex)
        {
            case 0:
                showKeyboardCall(5.2f,true);
                break;
            case 1:
                if (WXProtocol.Money == 0)
                {
                    WXLoginInfoPanel.instance.Tips("您的外星币不足。请在外星平台进行充值。");
                    return;
                }

                int count = 0;
                int.TryParse(countTxt.Text, out count);
                if (count == 0)
                {
                    WXLoginInfoPanel.instance.Tips("请输入大于0的数字");
                    return;
                }

                if (count > WXProtocol.Money)
                {
                    WXLoginInfoPanel.instance.Tips("您最多只能兑换" + WXProtocol.Money + "个游戏币");
                    return;
                }

                loadingCall(true);
                WXProtocol.instance.ExchangeMoney(count, (success, err) => {
                    if (success)
                    {
                        nowMoneyText.text = "您最多可以兑换" + WXProtocol.Money + "游戏币";
                        countTxt.Text = "0";
                        WXLoginInfoPanel.instance.Tips("您成功的兑换了"+ count+"游戏币");
                        if (WXLoginInfoPanel.instance.addGameMoneyCallback != null)
                        {
                            WXLoginInfoPanel.instance.addGameMoneyCallback(count);
                        }
                        if (exChangeComplete!=null) exChangeComplete();
                    }
                    else
                    {
                        Debug.Log(err);
                        //WXLoginInfoPanel.instance.Tips(err);
                    }
                    loadingCall(false);
                });
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
