using UnityEngine;
using System.Collections;

public class PropCard : BasePropItem
{
    public tk2dTextMesh priceTxt;
    public GameObject frame;
    public PropData data;
    public GameObject buyGo;
    public tk2dSprite bg;
    public tk2dSprite icon;

    internal void SetSelect(bool p)
    {
        frame.SetActive(p);
    }


    public bool GetIsBuy()
    {
        return buyGo.activeSelf;
    }


    internal void SetData(PropData propData)
    {
        data = propData;
        priceTxt.text = data.price.ToString();
    }

    internal void SetBuy(bool p)
    {
        buyGo.SetActive(p);
    }


    internal void SetEnable(bool p)
    {
        bg.SetSprite(p?"单个道具底板":"单个道具底板1");
        icon.color = p ? Color.white : Color.gray;
    }
}
