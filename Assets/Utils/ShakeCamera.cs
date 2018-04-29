using UnityEngine;
using System.Collections;

public class ShakeCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(count>0.01f){
			count-=0.01f;
			if(count<0.01f){
				count=0;
				this.transform.localPosition = new Vector3(0,0,-20);
				return;
			}
			Vector3 v = new Vector3((index%2==0?1.0f:-1.0f)*range*(count/countMax) , (index%2==0?1.0f:-1.0f)*range*(count/countMax),-20); 
			this.transform.localPosition =v;
			index++;
		}
	}
	public float count;
	float countMax;
	public float range;
	int index;

	public void Shake(float count){
		this.count = count;
		countMax = count;
		index = 0;
	}
}
