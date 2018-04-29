using UnityEngine;
using System.Collections;

public class PlayParticle : MonoBehaviour {
    public ParticleSystem ps;
	// Use this for initialization
	void Start () {
	
	}

    public void PlayParticles()
    {
        ps.Stop();
        ps.Play();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
