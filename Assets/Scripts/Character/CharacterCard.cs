using UnityEngine;
using System.Collections;

public class CharacterCard : BasePropItem {
    public GameObject frame;
    tk2dSprite s;
    public tk2dTextMesh priceText;
    public GameObject progressBar;
    public tk2dSprite bar;
    public tk2dSprite undownload;

	// Use this for initialization
	void Start () {
        progressBar.SetActive(false);        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public bool isLock = false;
    public CharacterData data;
    public int index;

    internal void SetIndex(int i)
    {
        index = i;
        if (s == null) s = GetComponent<tk2dSprite>();
        priceText.text = Global.CHARACTER_TABLE[i+1].price.ToString();

        
            switch (i)
            {
                case 1:
                case 2:
                case 5:
                case 6:
                    isLock = false;
                    undownload.gameObject.SetActive(false);
                    break;
                default:
                    isLock = DataUtils.GetCharacterIsUnLock(i + 1) == false;

                    if (Global.isAllResLocal)
                    {
                        undownload.gameObject.SetActive(false);
                    }
                    else
                    {
                        undownload.gameObject.SetActive(!isLock && !Global.instance.CheckVideoDownload(data));
                    }
                    break;
            }
        


       
        RefreshLock();
        
    }

    public bool GetCanSelect()
    {        
        switch (index)
        {
            case 1:
            case 2:
            case 5:
            case 6:
                return true;
        }
        return Global.instance.CheckVideoDownload(data) && DataUtils.GetCharacterIsUnLock(index+1);
    }

    

    internal void SetSelect(bool p)
    {
        Vector3 pos = transform.localPosition;
        pos.z = p ?-1 : (frame.activeSelf ? -0.5f : 0);
        transform.localPosition = pos;
        transform.localScale = p ? new Vector3(1.2f, 1.2f, 1) : Vector3.one;        
    }

    internal void SetFocus(bool p)
    {
        if (frame.activeSelf != p) frame.SetActive(p);
        Vector3 pos = transform.localPosition;
        pos.z = p? -0.5f:0;
        transform.localPosition = pos;       
    }

    internal void UpdateProgress(float per)
    {
        //Debug.Log("---------------------------");
        //Debug.Log(per);
        if (per == 1 || per==-1)
        {
            progressBar.SetActive(false);            
            if (per == -1)
            {
                undownload.gameObject.SetActive(true);
            }
            return;
        }

        if (!progressBar.activeSelf)
        {
            progressBar.SetActive(true);            
        }
        bar.scale = new Vector3(per, 1, 1);
    }

    internal void RefreshLock()
    {        
        s.SetSprite("p" + (index+1) + (isLock ? "_lock" : ""));
        priceText.gameObject.SetActive(isLock);        
    }
}
