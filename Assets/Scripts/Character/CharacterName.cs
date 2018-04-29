using UnityEngine;
using System.Collections;

public class CharacterName : MonoBehaviour {

    tk2dSprite s;
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        s = GetComponent<tk2dSprite>();
	}

    void UpdateName()
    {
        if (targetName!=null) s.SetSprite(targetName);
    }

    string targetName;

    internal void SetName(int p)
    {        
        p = p - 1;
        GetComponent<Animation>().Play();
        switch (p)
        {
            case 0:
            case 1:
                targetName = "alice";
                break;
            case 2:
            case 3:
                targetName = "ann";
                break;
            case 4:
            case 5:
                targetName = "emily";
                break;
            case 6:
            case 7:
                targetName = "princess";
                break;
        }
    }
}
