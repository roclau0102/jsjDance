using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class SongList : MonoBehaviour {

    public GameObject prefabSongCard;
    public List<SongCard> cards;
    public int selectIndex = 0;
    public float normalH = 1.17f;
    public float hoverH = 1.26f;
    public float xOffset = 0.17f;
    public float yOffset = 0.1f;
    bool firstTime = true;
    public float oneOffset = 0.1f;
    Vector3 bornPos = Vector3.zero;
    public float hight = 4;
    public CD cd;    
    public delegate SongInfo.DIFF_LEVEL GetDiffDelegate();
    public GetDiffDelegate getDiffCallback;
    public TextMesh songCount;

	// Use this for initialization
	void Start () {
        
	}

    void LoadCover()
    {
        cd.clearTexture();
        SongData card = dataList[selectIndex];
        if (card.coverBytes == null || card.coverBytes.Length == 0)
        {
            if (Global.IsDownloadCover(card))
            {
                Global.instance.DownloadCover(card, (t) =>
                {
                    if (t is Texture2D)
                    {
                        card.texture2d = t as Texture2D;

                    }
                    else
                    {
                        card.coverBytes = t as byte[];
                    }
                    Invoke("UpdateCD", 0.5f); 
                });
            }
            else
            {
                UpdateCD();
            }
        }
        else
        {
            UpdateCD();
        }
    }

    public bool downloading = false;

    internal bool Press()
    {
        SongCard card = curCard;


        if (card.isDownloaded && DataUtils.GetSongBuy(card.data))
        {
            return true;
        }

        bool needBuy = card.data.price != 0 && !DataUtils.GetSongBuy(card.data);
        bool hasMoney = DataUtils.GetMoney() >= card.data.price;

        if (needBuy)
        {
            if (hasMoney)
            {                    
                SongListMain.instance.buyConfirm.Show(() =>
                {
                    DataUtils.SetSongBuy(card.data);
                    DataUtils.AddMoney(-card.data.price);
                    card.CheckBuy();
                    Download(card);
                    Sounder.instance.Play("购买歌曲音效");
                }, "使用" + DataUtils.SONG_MONEY+"音乐币购买此音乐");                    
            }else{
                    SongListMain.instance.Alert("每首歌需要100音乐币,您的音乐币不足!");
                    Sounder.instance.Play("BAD音效");                        
            }
        }
        else
        {
            Download(card);
        }

        return false;
    }


    void Download(SongCard card)
    {
        downloading = true;
        card.Press((b) =>
        {            
            if (b)
            {   
                LoadCover();
            }
            downloading = false;
        });
    }

    List<SongData> dataList = new List<SongData>();
    int pageSize = 13;
    int upDownLeave = 6;
    bool initComplete = false;

    public Action createListComplete = null;

    internal void CreateList()
    {
        StartCoroutine("StartCreateList");
    }

    IEnumerator StartCreateList()
    {
        bornPos = transform.localPosition;
        cards = new List<SongCard>();

        float t = Time.time;
        foreach (KeyValuePair<int, SongData> data in Global.MUSIC_TABLE)
        {            
            if ((Version.currentPlatform == Version.PLAFTFORM_ENUM.WX_XRDS ||
                Version.currentPlatform == Version.PLAFTFORM_ENUM.WX_XRDSS_DISPLAY) &&
                data.Value.enName.Trim() == "SistarShakeIt")
            {
                continue;
            }

            bool found = false;
            for (int i = dataList.Count-1; i >0 ; i--)
            {
                if (data.Value.IsDownloaded()==dataList[i].IsDownloaded() &&  data.Value.id > dataList[i].id)
                {
                    found = true;
                    if(i!=dataList.Count-1){
                        dataList.Insert(i + 1, data.Value);
                    }
                    else
                    {
                        dataList.Add(data.Value);
                    }
                    break;
                }
            }
            if (!found)
            {
                dataList.Add(data.Value);
            }            
        }
        Debug.Log("排序用时:" + (Time.time-t));


        for (int i = 0; i < dataList.Count; i++)
        {
            dataList[i].index = i;
            if (DataUtils.songDataID == dataList[i].id)
            {
                selectIndex = i;
            }
        }

        int startIndex = 0;
        if (selectIndex > upDownLeave)
        {
            startIndex = selectIndex - upDownLeave;
        }
        if (startIndex + pageSize > dataList.Count)
        {
            startIndex = dataList.Count  - pageSize;
        }
        
        
        for (int i = 0; i < pageSize; i++)
        {
            cards.Add((Instantiate(prefabSongCard) as GameObject).GetComponent<SongCard>());
            cards[i].gameObject.transform.parent = transform;
            cards[i].gameObject.SetActive(false);
            cards[i].SetData(dataList[startIndex + i]);
            yield return new WaitForSeconds(0.01f);
        }
        Move(0);

        for (int i = 0; i < pageSize; i++)
        {
            cards[i].gameObject.SetActive(true);
        }

        initComplete = true;

        if (createListComplete != null)
        {
            createListComplete();
        }
    }

    

    enum REACH_STATE
    {
        NONE,
        REACH,
        DONE
    }

    float speed =1f;
    REACH_STATE reach = REACH_STATE.NONE;
    const float cardHeight = 1.08f;
    SongCard curCard = null;
    public bool isScroll = true;
    
	// Update is called once per frame
	void FixedUpdate () {
        if (cards == null || cards.Count == 0 || !initComplete || !isScroll) return;

        if (selectIndex < 0){
            selectIndex = 0;
        }
        else if (selectIndex >= dataList.Count)
        {
            selectIndex = dataList.Count-1;
        }

        float ty = -cards[0].index * cardHeight;
        float tx = -cards[0].index * xOffset;
        Vector3 temp = Vector3.one;

        float selectedY = 0;
        float selectedX = 0;

        for (int i = 0; i < cards.Count; i++)
        {
            SongCard go = cards[i];
            if (go.resetPositionNow)
            {
                temp.Set(tx, ty, 0);
                go.transform.localPosition = temp;
                go.resetPositionNow = false;
            }
            else
            {
                temp.Set(tx, go.gameObject.transform.localPosition.y + (ty - go.gameObject.transform.localPosition.y) * speed, 0);
                go.transform.localPosition = temp;
            }
            
            go.SetHover(go.index == selectIndex);
            if (reach == REACH_STATE.NONE && Mathf.Abs(ty - go.gameObject.transform.localPosition.y) < 0.1f)
            {
                reach = REACH_STATE.REACH;
            }

            float targetS = 1;
            if (go.index  == selectIndex)
            {
                curCard = go;
                selectedX = go.transform.localPosition.x;
                selectedY = go.transform.localPosition.y;
                targetS = 1.25f;
                ty -= oneOffset;
            } else if (go.index== (selectIndex-1))
            {
                ty -= oneOffset*1.2f;
                targetS = 1.1f; 
            }
            else if (go.index == (selectIndex + 1))
            {
                targetS = 1.1f;
                ty -= oneOffset*0.5f;
            }
            else if (i == (selectIndex - 2))
            {
                ty -= oneOffset*0.5f;
            }
            temp.Set(go.transform.localScale.x + (targetS - go.transform.localScale.x) * speed, go.transform.localScale.y+(targetS - go.transform.localScale.y) *speed, 1);
            go.transform.localScale = temp;            
            
            ty -= (i == selectIndex ? hoverH : normalH)- yOffset;
            tx -= xOffset;
        }

        
        if (reach == REACH_STATE.REACH)
        {           
            
            SongCard g;
            if (direction<0)
            {
                //向上加
                if (cards[0].index >0)
                {
                    g = cards[cards.Count-1];
                    if (selectIndex<= (dataList.Count- upDownLeave-1))
                    {
                        g.SetData(dataList[cards[0].index - 1]);
                        g.resetPositionNow = true;
                        cards.RemoveAt(cards.Count - 1);
                        cards.Insert(0, g);
                    }
                }                
            }
            else if(direction>0)
            {
                if ((cards[cards.Count - 1].index + 1) < dataList.Count )
                {
                    g = cards[0];
                    if (selectIndex >= upDownLeave)
                    {
                        SongCard sc = cards[cards.Count - 1];
                        SongData nextData = dataList[sc.data.index + 1];
                        g.SetData(nextData);
                        g.resetPositionNow = true;
                        cards.RemoveAt(0);
                        cards.Add(g);
                    }                    
                }
            }
            reach = REACH_STATE.DONE;
        }

        temp.Set(transform.localPosition.x + (bornPos.x - selectedX - transform.localPosition.x) * speed, transform.localPosition.y + (-selectedY + hight - transform.localPosition.y) * speed, 0);
        transform.localPosition = temp;

        if (firstTime)
        {
            speed = .3f;
            firstTime = false;
        }
	}

    public void OnDiffChange(SongInfo.DIFF_LEVEL diff)
    {
        if (cards == null) return;
        DataUtils.difficult = diff;
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].RefreshBg();
        }
        UpdateCD();
    }


    int direction = 0;
    public void Move(int p=0)
    {
        if (Global.MUSIC_TABLE.Count == 0) return;
        selectIndex += p;
        direction = p;

        if (selectIndex < 0)
        {
            selectIndex = 0;
        }
        else if (cards != null && selectIndex >= dataList.Count)
        {
            selectIndex = dataList.Count - 1;
        }
        songCount.text = (  selectIndex+1) + "/" + Global.MUSIC_TABLE.Count;
        LoadCover();
        reach = REACH_STATE.NONE;
    }

    void UpdateCD()
    {
        if (dataList[selectIndex].coverBytes != null)
        {
            cd.UpdateData(dataList[selectIndex], dataList[selectIndex].coverBytes);
        }
        else if (dataList[selectIndex].texture2d != null)        
        {
            cd.UpdateData(dataList[selectIndex], dataList[selectIndex].texture2d);
        }
    }
     
    internal int GetSelectData()
    {
        return dataList[selectIndex].id;
    }

    internal void CancelDownload()
    {
        if (SongCard.cancelCallback != null) SongCard.cancelCallback();
        downloading = false;
    }

    internal SongData GetRandomDownload()
    {
        List<SongData> downloaded = new List<SongData>();
        for (int i = 0; i < dataList.Count; i++)
        {
            if(dataList[i].IsDownloaded()){
                downloaded.Add(dataList[i]);
            }            
        }
        SongData d = downloaded[UnityEngine.Random.Range(0, downloaded.Count)];
        downloaded.Clear();
        return d;
    }

    internal void GetXiaoPingGouItem()
    {
        
    }
}
