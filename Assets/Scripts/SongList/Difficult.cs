using UnityEngine;
using System.Collections;

public class Difficult : MonoBehaviour {
    public GameObject[] btns;
    int index = 0;
    public System.Action<SongInfo.DIFF_LEVEL> changeCallback;


	// Use this for initialization
	void Start () {
        Move((int)DataUtils.difficult);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public SongInfo.DIFF_LEVEL GetDiff()
    {
        return (SongInfo.DIFF_LEVEL)index;
    }

    internal void Move(int p)
    {
        int lastIndex = index;
        index += p;
        if (index < 0)
        {
            index = 0;
        }
        else if (index >( btns.Length-1))
        {
            index = btns.Length-1;
        }

        if (changeCallback != null)
        {
            changeCallback((SongInfo.DIFF_LEVEL)index);
        }

        for (int i = 0; i < btns.Length; i++)
        {
            if (i == index)
            {
                btns[i].transform.Find("Light").gameObject.SetActive(true);
                btns[i].GetComponent<Animation>().Play("DiffZoomLarge");
            }
            else
            {
                btns[i].transform.Find("Light").gameObject.SetActive(false);
                if (lastIndex== i) btns[i].GetComponent<Animation>().Play("DiffZoomNormal");
            }
        }
    }


}
