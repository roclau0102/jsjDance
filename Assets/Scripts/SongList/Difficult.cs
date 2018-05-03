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
        int newIndex = index + p;
        if (newIndex < 0)
        {
            newIndex = 0;
        }
        else if (newIndex > ( btns.Length-1))
        {
            newIndex = btns.Length-1;
        }

        Set(newIndex);
    }

    internal void Set(int newIndex)
    {
        if (changeCallback != null)
        {
            changeCallback((SongInfo.DIFF_LEVEL)newIndex);
        }

        for (int i = 0; i < btns.Length; i++)
        {
            if (i == newIndex)
            {
                btns[i].transform.Find("Light").gameObject.SetActive(true);
                btns[i].GetComponent<Animation>().Play("DiffZoomLarge");
            }
            else
            {
                btns[i].transform.Find("Light").gameObject.SetActive(false);
                if (index == i) btns[i].GetComponent<Animation>().Play("DiffZoomNormal");
            }
        }

        index = newIndex;
    }


}
