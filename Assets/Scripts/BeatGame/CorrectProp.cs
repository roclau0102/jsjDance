using UnityEngine;
using System.Collections;

public class CorrectProp : MonoBehaviour {

    public tk2dTextMesh text;
    public Animation iconAnimation;
    public GameObject particle;

    public int GetCount()
    {
        return DataUtils.correntPropCount;
    }

    /// <summary>
    /// 使用
    /// </summary>
    /// <returns>是否使用成功</returns>
    public bool Use()
    {
        if (DataUtils.correntPropCount > 0)
        {
            DataUtils.correntPropCount--;
            UpdateText();
            iconAnimation.Play();
            particle.SetActive(true);
            return true;
        }
        return false;
    }


	// Use this for initialization
	void Start () {
        UpdateText();
	}

    void UpdateText()
    {
        text.text = DataUtils.correntPropCount.ToString();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
