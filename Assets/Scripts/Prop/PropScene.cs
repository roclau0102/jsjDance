using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PropScene : BaseScene {
    public PropCard[] propCards;
    
    public GameObject okBtnFrame;
    
    public Alert alert;

    public CharacterCard[] cards;
    public CharacterName nameSprite;
    public BuyConfirm confirm;
    public GameObject lockSprite;
    public Model model;

    
    bool downloading = false;
    JLoaderInfo info;
    public GameObject selectEff;

    int characterIndex;
    //bool run = false;
    bool inProp = true;

	// Use this for initialization
	void Start () {
        okBtnFrame.SetActive(true);
        DataUtils.isAutoMode = false;
        DataUtils.runingAutoMode = false;

        curItem = okBtn;
        //初始化设置
        Global.init(() =>
        {
            for (int i = 0; i < propCards.Length; i++)
            {
                if (Global.PROP_TABLE.ContainsKey(i + 1))
                {
                    propCards[i].SetData(Global.PROP_TABLE[i + 1]);
                    switch(i){
                        case 0:
                        case 1:
                        case 2:
                            propCards[i].SetBuy(DataUtils.correntPropCount==((i+1)*5)); //纠正道具
                            break;
                        case 3:
                        case 4:
                        case 5:
                            propCards[i].SetBuy(DataUtils.lifeType == (Global.LIFE_TYPE)(i - 2));//血量道具
                            break;
                    }                    
                }               
            }
            
            //===================character

            
            for (int i = 0; i < cards.Length; i++)
            {
                CharacterCard card = cards[i];
                card.data = Global.CHARACTER_TABLE[i + 1];
                card.gameObject.SetActive(true);
                card.SetIndex(i);
            }

            //随机操作丢到了DataUtils里去了..从SongList过来之前已经随机好了.确保每次进游戏都是选的不同的人.
            int r = DataUtils.characterID;
            characterIndex = r - 1;
            SetCharacter(r-1);

            if (Guide.instance != null)
            {
                Guide.instance.Show();
            }
        });
	}
    
    void SelectCharacter(int index)
    {
        this.characterIndex = index;
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].SetSelect(i == index);            
        }        
        if (index>0)lockSprite.SetActive(cards[index].isLock);
    }


    private void DownloadVideo()
    {
        if (downloading) return;

        CharacterCard card = cards[characterIndex];
        card.undownload.gameObject.SetActive(false);

        if (Global.isAllResLocal)
        {
            downloading = false;
            if (confirm.gameObject.activeSelf) confirm.gameObject.SetActive(false);
            SetCharacter(characterIndex, true);
            return;
        }

        if (!Global.instance.CheckVideoDownload(card.data))
        {
            downloading = true;
            info = Global.instance.DownloadVideo(card.data, (per) =>
            {
                if (per == -1)
                {
                    downloading = false;
                    alert.Show("网络异常或磁盘容量不足");
                    Sounder.instance.Play("BAD音效");
                    if (confirm.gameObject.activeSelf) confirm.gameObject.SetActive(false);
                }
                else if (per == 1)
                {
                    downloading = false;
                    alert.Show("加载成功！");
                    if (confirm.gameObject.activeSelf) confirm.gameObject.SetActive(false);
                    
                }
                card.UpdateProgress(per);
            });
        }
        else
        {
            SetCharacter(characterIndex, true);
        }
    }

    
    void SetCharacter(int index, bool press = false)
    {
        DataUtils.characterID = index+1;
        model.SetIndex(index+1, press);
        nameSprite.SetName(index);        
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].SetFocus(i == characterIndex);
        }
        
        if (press)
        {            
            selectEff.gameObject.SetActive(true);
            Sounder.instance.Play("选中人物音效");
        }
        else
        {
            Sounder.instance.Play("按键音效");
        }
    }




    float pressTime = 0;


    public void  FocusOnItem(BasePropItem item, int x, int y)
    {
        
        BasePropItem next = null;
        switch(x){
            case -1:
                next = item.leftItem;
                break;
            case 1:
                next = item.rightItem;
                break;
        }

        switch (y)
        {
            case -1:
                next = item.upItem;
                break;
            case 1:
                next = item.downItem;
                break;
        }

        if (next == curItem) return;

        if (curItem != null)
        {
            if (curItem is CharacterCard)
            {
                (curItem as CharacterCard).SetSelect(false);
            }
            else if (curItem is PropCard)
            {
                (curItem as PropCard).SetSelect(false);
            }
        }
        curItem = next;
        okBtnFrame.gameObject.SetActive(false);
        if (curItem is CharacterCard)
        {
            (curItem as CharacterCard).SetSelect(true);
            SelectCharacter((curItem as CharacterCard).index);
            inProp = false;
            UnselectProp();
        }
        else if (curItem is PropCard)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].SetSelect(false);
            }

            (curItem as PropCard).SetSelect(true);
            inProp = true;
        }
        else if (curItem == okBtn)
        {
            okBtnFrame.gameObject.SetActive(true);
        }
    }

    public BasePropItem propItem;
    public BasePropItem characterItem;

    public BasePropItem curItem;

    public override void Move(int x, int y, BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (Time.time - pressTime < 0.3f) return;

        pressTime = Time.time;
        
        if (downloading) return;
        Sounder.instance.Play("按键音效");
        if (alert.gameObject.activeSelf)
        {            
            return;
        }

        if (confirm.gameObject.activeSelf)
        {
            confirm.Move(x);
            return;
        }

        if (curItem==null || curItem == okBtn)
        {
            okBtnFrame.SetActive(false);
            FocusOnItem(okBtn, x, y);
        }
        else
        {
            FocusOnItem(curItem, x, y);
        }
    }


    void UnselectProp()
    {
        for (int i = 0; i < propCards.Length; i++)
        {
            propCards[i].SetSelect(false);
        }
    }



    bool TryBuyItem(int price, int lastPrice)
    {        
        int nowMoney = lastPrice + DataUtils.GetMoney();
        if (nowMoney  >= price)
        {
            DataUtils.AddMoney(-price + lastPrice);
            return true;
        }
        return false;
    }





    public override void PressEnter(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {        
        if (keyState != JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN) return;


        if (alert.gameObject.activeSelf)
        {
            alert.gameObject.SetActive(false);
            return;
        }


        if (curItem == okBtn)
        {
            if (DataUtils.isAutoMode)
            {
                Global.GoAutoMode();
            }

            //加载下个场景
            EnterGame();
            return;
        }


        //=====================================CHARACTER===================
        if (inProp == false)
        {        
            if (downloading)
            {
                if (confirm.gameObject.activeSelf)
                {
                    confirm.Press();
                    return;
                }
                return;
            }
            if (keyState != JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN) return;
            if (confirm.gameObject.activeSelf)
            {
                confirm.Press();
                return;
            }
            else if (alert.gameObject.activeSelf)
            {
                alert.gameObject.SetActive(false);
                return;
            }


            switch (characterIndex)
            {
                case 1:
                case 2:
                case 5:
                case 6:
                    //直接进入游戏
                    SetCharacter(characterIndex, true);
                    return;
            }

            CharacterCard card = cards[characterIndex];
            int money = DataUtils.GetMoney();

            bool moneyEnough = money >= card.data.price;
            if (card.isLock)
            {
                confirm.Show(() =>
                {
                    if (moneyEnough)
                    {
                        Sounder.instance.Play("消费音效");
                        DataUtils.SetCharacterIsUnLock(card.data.id);
                        DataUtils.AddMoney(-card.data.price);
                        card.isLock = false;
                        card.RefreshLock();
                        lockSprite.SetActive(cards[characterIndex].isLock);
                        DownloadVideo();
                    }
                    else
                    {
                        Sounder.instance.Play("BAD音效");
                        alert.Show("您的音乐币不足");
                    }
                }, "花费" + card.data.price + "音乐币解锁");
            }
            else
            {
                DownloadVideo();
            }
        }
        else
        {            
            PropData lastSelectData = null;

            PropCard c = curItem as PropCard;
            if (c == null) return;
            switch (c.data.id)
            {
                case 1:
                case 2:
                case 3:
                    //纠正ID从1开始
                    if (DataUtils.isAutoMode) return;
                    if (c.GetIsBuy())
                    {
                        c.SetBuy(false);
                        DataUtils.AddMoney(c.data.price);
                        DataUtils.correntPropCount =0;
                        Sounder.instance.Play("购买道具音效取消");
                        return;
                    }
                
                    if (DataUtils.correntPropCount > 0)
                    {
                        lastSelectData = Global.PROP_TABLE[DataUtils.correntPropCount / 5]; //ID1,2,3就是纠正道具
                    }

                    if (TryBuyItem(c.data.price, lastSelectData == null ? 0 : lastSelectData.price))
                    {
                        c.SetBuy(true);
                        DataUtils.correntPropCount =  c.data.id * 5;
                        if (lastSelectData != null)
                        {
                            propCards[lastSelectData.id - 1].SetBuy(false);
                        }
                        Sounder.instance.Play("购买道具音效");
                    }
                    else
                    {
                        alert.Show("您的音乐币不足");
                        Sounder.instance.Play("BAD音效");
                    } 
                    break;
                case 4:
                case 5:
                case 6:
                    //456
                    if (DataUtils.isAutoMode) return;
                    if (c.GetIsBuy())
                    {
                        c.SetBuy(false);
                        DataUtils.AddMoney(c.data.price);
                        DataUtils.lifeType = Global.LIFE_TYPE.LV1;
                        Sounder.instance.Play("购买道具音效取消");
                        return;
                    }

                    if (DataUtils.lifeType > Global.LIFE_TYPE.LV1)
                    {
                        lastSelectData = Global.PROP_TABLE[(int)DataUtils.lifeType + 3]; //ID 4,5,6就是血量
                    }

                    if (TryBuyItem(c.data.price, lastSelectData==null?0:lastSelectData.price))
                    {
                        DataUtils.lifeType = (Global.LIFE_TYPE)c.data.id - 3;
                        c.SetBuy(true);
                        if(lastSelectData!=null)propCards[lastSelectData.id - 1].SetBuy(false);
                        Sounder.instance.Play("购买道具音效");
                    }
                    else
                    {
                        alert.Show("您的音乐币不足");
                        Sounder.instance.Play("BAD音效");
                    }
                    break;
                case 7:
                    c.SetBuy(!c.GetIsBuy());
                    DataUtils.isAutoMode = c.GetIsBuy();
                    if (DataUtils.isAutoMode)
                    {
                        Sounder.instance.Play("购买道具音效");
                        CancelBuy();                    
                    }
                    else
                    {
                        Sounder.instance.Play("购买道具音效取消");
                    }

                    for (int i = 0; i < propCards.Length; i++)
                    {
                        if (propCards[i]!=c)
                        {
                            propCards[i].SetEnable(!DataUtils.isAutoMode);
                        }
                    }
                    break;
            }
        }
    }

    public void EnterGame()
    {
        if (DataUtils.mode == Global.MODE.MODE_1P)
        {
            LoadLevel("Video");
        }

        Sounder.instance.Play("选中歌曲下一页");
    }

    void CancelBuy()
    {
        PropData lastSelectData;
        if (DataUtils.lifeType > Global.LIFE_TYPE.LV1)
        {
            lastSelectData = Global.PROP_TABLE[(int)DataUtils.lifeType + 3]; //ID 4,5,6就是血量
            propCards[lastSelectData.id - 1].SetBuy(false);
            DataUtils.AddMoney(lastSelectData.price);
            DataUtils.lifeType = Global.LIFE_TYPE.LV1;
        }

        if (DataUtils.correntPropCount > 0)
        {
            lastSelectData = Global.PROP_TABLE[DataUtils.correntPropCount / 5]; //ID1,2,3就是纠正道具
            propCards[lastSelectData.id - 1].SetBuy(false);
            DataUtils.correntPropCount = 0;
            DataUtils.AddMoney(lastSelectData.price);
        }

    }

    public override void Cancel(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        Sounder.instance.Play("返回按键");       
        if (keyState != JoystickManager.JOYSTICK_KEY_STATE.KEY_DOWN) return;

        if (alert.gameObject.activeSelf)
        {
            alert.gameObject.SetActive(false);
            return;
        }

        
        Sounder.instance.Play("返回按键");
        if (downloading)
        {
            confirm.Show(() =>
            {
                if (info != null)
                {
                    JLoader.instance.Remove(info);
                    info = null;
                    CharacterCard card = cards[characterIndex];
                    card.UpdateProgress(-1);
                    downloading = false;
                }
            }, "正在下载，要取消吗？");
        }
        else
        {
            BackToSongList();
        }
    }

    public void BackToSongList()
    {
        LoadLevel("SongList", false);
    }

}
