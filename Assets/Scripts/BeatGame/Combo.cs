using UnityEngine;
using System.Collections;

public class Combo : MonoBehaviour {

    public tk2dSprite[] number;
    public GameObject eff;
    

    int v = -1;

	// Use this for initialization
	void Start () {
	
	}
    public void Awake()
    {
        gameObject.SetActive(false);
    }

    public int GetCount()
    {
        return v;
    }

     string lastStr;
      

    /// <summary>
    /// 添加一次
    /// </summary>
    public void Add()
    {        
        lastStr = v.ToString();
        v++;
        if (v < 2)
        {
            return;
        }
        else if (v > 999)
        {
            v = 999;
        }
        if (v % 10 == 0) eff.SetActive(true);
        gameObject.SetActive(true);
        StartCoroutine(CAdd());
        GetComponent<Animation>().Play();
    }

   IEnumerator  CAdd()
    {          
        string str = v.ToString();

        for (int i = 0; i < 3; i++)
        {
            if (i < str.Length)
            {
                number[i].gameObject.SetActive(true);
                number[i].SetSprite(str.Substring(i, 1));
            }
            else
            {
                number[i].gameObject.SetActive(false);
            }
        }


        for (int i = 0; i < 3; i++)
        {
            if (i < str.Length)
            {
                if (lastStr.Length != str.Length || (lastStr.Substring(i, 1) != str.Substring(i, 1)))
                {
                    number[i].GetComponent<Animation>().Play();
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }



    /// <summary>
    /// 归零
    /// </summary>
    public void Reset()
    {
        v = -1;
        gameObject.SetActive(false);
    }
}
