using UnityEngine;
using System.Collections;

public class Model : MonoBehaviour {
    GameObject[] list;

    
	// Use this for initialization
	void Start () {
        list =  new GameObject[transform.childCount];
        for(int i=0;i<transform.childCount;i++){
            list[i] = transform.GetChild(i).gameObject;
        }	    
	}

    public void SetIndex(int index, bool selected = false)
    {
        
        index = (index-1) * 2;
        if (selected) index++;
        for (int i = 0; i < list.Length; i++)
        {
            list[i].SetActive(i == index);
        }
        GetComponent<Animation>().Play();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
