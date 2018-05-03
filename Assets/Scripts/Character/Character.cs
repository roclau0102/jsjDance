using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class Character : BaseScene
{
    public CharacterCard[] cards;
    public CharacterName nameSprite;
    public BuyConfirm confirm;
    public Alert alert;
    public GameObject lockSprite;
    public Model model;
    public bool hasSelect = false;
    bool downloading = false;
    JLoaderInfo info;
    public GameObject selectEff;
    public GameObject okBtnSelectFrame; 

    //从1开始
    int index = 0;

    

	void Start()
    {
        Global.init(() => {                        
            List<int> random = new List<int>();
            for (int i = 0; i < cards.Length; i++)
            {
                CharacterCard card = cards[i];
                card.data = Global.CHARACTER_TABLE[i + 1];
                card.gameObject.SetActive(true);
                card.SetIndex(i + 1); 
                if(card.GetCanSelect() && (i+1)!=DataUtils.characterID ){
                    random.Add(i+1);
                }
            }
            int r = random[Random.Range(0, random.Count)];            
            string filter="";
            for(int i=0;i<random.Count;i++){
                filter+=random[i]+",";
            }            
            SelectCard(r);
            okBtnSelectFrame.gameObject.SetActive(true);

            if (DataUtils.runingAutoMode)
            {
                Invoke("AutoGoNext", 3);
            }
        });
	}

    void AutoGoNext()
    {
        if (!DataUtils.runingAutoMode) return;
        SelectCharacter(index, true);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    float pressTime=0;

    public override void Move(int x, int y, BaseScene.INPUT_TYPE type,JoystickManager.JOYSTICK_KEY_STATE keyState,JoystickManager.PLAYER_INDEX player )
    {
        if (hasSelect) return;
        if ((Time.time - pressTime) < 0.5f) return;
        pressTime = Time.time;
        if (downloading)
        {
             if (confirm.gameObject.activeSelf)
            {
                confirm.Move(x);                
            }
            return;
        }

        if (confirm.gameObject.activeSelf)
        {
            confirm.Move(x);
            return;
        }

        if (okBtnSelectFrame.activeSelf)
        {
            okBtnSelectFrame.SetActive(false);
        }
        else if (alert.gameObject.activeSelf)
        {
            return;
        }

       
        if (x !=0)
        {
            index += x;
        }
        else if (y != 0)
        {
            index += y*4;
        }

        if (index < 1)
        {
            index = 8;   
        }
        else if (index > 8)
        {
            index = 1; 
        }
        SelectCard(index);        
    }

    void SelectCard(int index)
    {
        this.index = index;
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].SetSelect((i + 1) == index);
            Vector3 pos = cards[i].transform.localPosition;
            pos.z = (i + 1) == index ? -1 : 0;
            cards[i].transform.localPosition = pos;
            cards[i].transform.localScale = (i + 1) == index ? new Vector3(1.2f, 1.2f, 1) : Vector3.one;
        }
        nameSprite.SetName(index);
        SelectCharacter(index);
        lockSprite.SetActive(cards[index - 1].isLock);                
    }

    public override void PressEnter(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (hasSelect) return;
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

        if (Global.isAllResLocal)
        {
            SelectCharacter(index, true);
            return;
        }

        switch (index)
        {
            case 2:
            case 3:
            case 6:
            case 7:
                //直接进入游戏
                SelectCharacter(index,true);
                return;
        }

        CharacterCard card = cards[index-1];
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
                    lockSprite.SetActive(cards[index - 1].isLock);
                    DownloadVideo();
                }
                else
                {
                    Sounder.instance.Play("BAD音效");
                    alert.Show("您的音乐币不足");
                }
            }, "花费" + card.data.price + "音乐币解锁" );
        }
        else
        {
            DownloadVideo();
        }
    }

    


    private void DownloadVideo()
    {
        if (downloading) return;

        CharacterCard card = cards[index-1];
        card.undownload.gameObject.SetActive(false);

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
                else if(per==1)
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
            SelectCharacter(index,true );
        }
    }

    bool run = false;

    void SelectCharacter(int index, bool press=false)
    {      
        DataUtils.characterID = index;
        model.SetIndex(index, press);
        if (press)
        {
            if (run) return;
            run = true;
            RemoveKeyEvent();
            hasSelect = true;
            okBtn.GetComponent<Animation>().Play();            
            Invoke("LoadLevel",1f);
            selectEff.gameObject.SetActive(true);
            Sounder.instance.Play("选中人物音效");
        }
        else
        {
            Sounder.instance.Play("按键音效");            
        }
    }

    void LoadLevel()
    { 
        LoadLevel("Video");
    }

    public override void Cancel(BaseScene.INPUT_TYPE type, JoystickManager.JOYSTICK_KEY_STATE keyState, JoystickManager.PLAYER_INDEX player)
    {
        if (hasSelect) return;
        Sounder.instance.Play("返回按键");
        if(downloading){
             confirm.Show(() => {
                if (info != null)
                {
                    JLoader.instance.Remove(info);
                    info = null;
                    CharacterCard card = cards[index - 1];
                    card.UpdateProgress(-1);
                    downloading = false;
                }        
            }, "正在下载，要取消吗？");
        }
        else
        {
            LoadLevel("Prop",false);
        }
    }
}
