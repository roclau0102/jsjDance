using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Keyboard : WXPanelBasic {

    List<tk2dBaseSprite> items = new List<tk2dBaseSprite>();
    public System.Action<string> OnPress;
    List<int> lineCount = new List<int>();
    
	// Use this for initialization
	void Start () {
        for (int j = 0; j < transform.childCount; j++)
        {
            Transform t = transform.GetChild(j);            
            int c = t.childCount;
            if (c == 0) continue;
            lineCount.Add(c);
            for (int i = 0; i < c; i++)
            {
                tk2dBaseSprite s = t.GetChild(i).GetComponent<tk2dBaseSprite>(); 
                items.Add(s);
            }
        }
        maxIndex = items.Count;
	}

    protected override void InShow()
    {
        focusIndex = -1;
        Move(1, 0);
    }
    

    public override void Enter()
    {
        if (OnPress != null)
        {
            string n = items[focusIndex].gameObject.name;
            if (n == "-大小")
            {
                for (int i = 0; i < 26; i++)
                {
                    TextMesh t = items[i].transform.GetChild(0).GetComponent<TextMesh>();
                    if(char.IsUpper(t.text.ToCharArray()[0] )){
                        t.text = t.text.ToLower();
                    }
                    else
                    {
                        t.text = t.text.ToUpper();
                    }                    
                }
            }
            else if (n == "确定")
            {
                gameObject.SetActive(false);
            }
            else
            {
                OnPress(items[focusIndex].gameObject.name);
            }            
        }
    }

    public override void Cancel()
    {
        gameObject.SetActive(false);        
    }


    public int GetLineCount(int pos)
    {        
        int start = 0;
        for (int i =0; i < lineCount.Count; i++)
        {
            if (pos >= start && pos < (start + lineCount[i]))
            {
                return lineCount[i];
            }
            start+= lineCount[i];
        }
        return 0;
    }


    public override void Move(int x, int y)
    {
        int newIndex = focusIndex;
        if (x != 0)
        {
            newIndex += x;            
        }
        else if (y != 0)
        {
            int c = GetLineCount(newIndex);
            newIndex += c*y;
            if (newIndex >= items.Count)
            {
                newIndex = items.Count - 1;
            }
        }

        if (newIndex >= items.Count)
        {
            newIndex = 0;
        }
        else if (newIndex < 0)
        {
            newIndex = items.Count - 1;
        }

        if (newIndex == focusIndex) return;
        focusIndex = newIndex;

        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetSprite(focusIndex == i ? "key_over" : "key");
        }           
    }
	
}


