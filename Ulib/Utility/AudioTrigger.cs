using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class AudioTrigger : MonoBehaviour 
{
	public string SoundToPlay;
    public bool OneShot = true, 
                FadeMusic = true,
				SendFeedBack = true;
    public float SoundVolume = .5f, 
                 MusicFadeVolume = .1f,
                 MusicFadeTime = .5f;


    private bool hasBeenTriggered = false;

	// Use this for initialization
	void Start ()
	{
		collider.isTrigger = true;
	}
	
	void OnTriggerEnter(Collider c)
	{
        if (c.gameObject.tag != "Player") return;

	    if (hasBeenTriggered && OneShot) return;

	    SoundManager.Play2DSound(SoundToPlay, SoundVolume, "default", false,FadeMusic, MusicFadeVolume, 1);
			

	    hasBeenTriggered = true;
	}
	
}
