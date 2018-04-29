using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ZoomNumber : MonoBehaviour {
    public tk2dTextMesh[] texts;
    int _v;
	// Use this for initialization
	void Start () {        
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = "0";
        }
	}

    string backText;
    string nowText;

    public int GetValue()
    {
        return _v;
    }

    public void Add()
    {
        SetValue(_v + 1);
    }

    public void SetValue(int v)
    {        
        if(v== _v)return;
        backText = _v.ToString();
        nowText = v.ToString();
        while (backText.Length < 6)
        {
            backText = "0" + backText;
        }

        while (nowText.Length < 6)
        {
            nowText = "0" + nowText;
        }

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = nowText.Substring(nowText.Length-i-1,1);
        }
        _v = v;      
        StopCoroutine("Show");
       StartCoroutine("Show");                 
        //Show();
    }

    IEnumerator Show()
    {
        int startIndex = 0;
        for (int i = 0; i < backText.Length; i++)
        {
            if (backText.Substring(i,1) != nowText.Substring(i, 1))
            {
                startIndex = i;
                break;
            }
        }
        startIndex = backText.Length - startIndex;

        for (int i = 0;i<backText.Length ; i++)
        {            
            if (i < startIndex)
            {
                texts[i].GetComponent<Animation>().Stop();
                texts[i].GetComponent<Animation>().Play();
               yield return new WaitForSeconds(0.1f);
            }
        }
    }    
}




#if UNITY_EDITOR
[CanEditMultipleObjects()]
[CustomEditor(typeof(ZoomNumber), true)]
public class ZoomNumberExtend : Editor
{
    public override void OnInspectorGUI()
    {        
        base.OnInspectorGUI();
        ZoomNumber g = target as ZoomNumber;
        if (g.gameObject.activeSelf == false) return;

        int newV = EditorGUILayout.IntField("值", g.GetValue());
        if (newV != g.GetValue())
        {
            g.SetValue(newV);
        }

        if(GUILayout.Button("增加1", GUILayout.Height(50))){
            g.Add();
        }
    }
}
#endif