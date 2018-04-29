using UnityEngine;
using System.Collections;

public class Eff : MonoBehaviour
{
    #region 特效预制
    public GameObject arrowLeft;
    public GameObject arrowRight;
    public GameObject arrowTop;
    public GameObject arrowDown;

    public GameObject longPress;
    #endregion

    #region 特效
    public GameObject lastHit;
    public GameObject bigArrowDown;
    public GameObject bigArrowUp;
    public GameObject bigArrowLeft;
    public GameObject bigArrowRight;
    public GameObject combo100Eff;
    #endregion

    public static Eff instance
    {
        get;
        private set;
    }

    // Use this for initialization
	void Awake () {
        instance = this;
	}

    

    bool[] combo100 = new bool[2];

    public void ShowCombo100(bool b, int playerIndex)
    {
        combo100[playerIndex] = b;

        if (Version.currentPlatform == Version.PLAFTFORM_ENUM.WX_XRDS || Version.currentPlatform == Version.PLAFTFORM_ENUM.WX_XRDSS_DISPLAY) return;
        if (combo100[0] || combo100[1])
        {
            combo100Eff.SetActive(true);
        }
        else
        {
            combo100Eff.SetActive(false);
        }        
    }

    public void ScreenHit(SongInfo.DIRECTION d)
    {
        switch(d){
            case SongInfo.DIRECTION.LEFT:
                bigArrowLeft.SetActive(true);
                break;
            case SongInfo.DIRECTION.RIGHT:
                bigArrowRight.SetActive(true);
                break;
            case SongInfo.DIRECTION.UP:
                bigArrowUp.SetActive(true);
                break;
            case SongInfo.DIRECTION.DOWN:
                bigArrowDown.SetActive(true);
                break; 
        }

    }

    public void AddArrowEff(Vector3 pos, SongInfo.DIRECTION d)
    {
        GameObject prefab=null;
        switch(d){
            case SongInfo.DIRECTION.LEFT:
                prefab = arrowLeft;
                break;
            case SongInfo.DIRECTION.RIGHT:
                prefab = arrowRight;
                break;
            case SongInfo.DIRECTION.UP:
                prefab = arrowTop;
                break;
            case SongInfo.DIRECTION.DOWN:
                prefab = arrowDown;
                break;            
        }

        if (prefab == null)
        {
            Debug.Log(d);
            Debug.Break();
        }

        GameObject go = PrefabPool.instance(prefab);
        go.name = prefab.name;
        go.transform.parent = transform;
        go.transform.position = pos;
        go.GetComponent<ParticleSystem>().Play();
        go.SetActive(true);
        TimeCallback call = go.GetComponent<TimeCallback>();
        ParticleSystem particle = go.GetComponent<ParticleSystem>();
        particle.Stop();
        particle.Play(true);
        call.data = prefab;
        call.doneCallback = (g) =>
        {
            PrefabPool.restore(g, g.GetComponent<TimeCallback>().data as GameObject);
        };
    }

    public LongPressEff AddLongArrowEff()
    {
        GameObject go = PrefabPool.instance(longPress);
        go.transform.parent = transform;
        LongPressEff eff=  go.GetComponent<LongPressEff>();
        eff.prefab = longPress;
        return eff;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
