using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class AudioTrigger : MonoBehaviour 
{
	public string SoundToPlay;
    public bool hasBeenTriggered = false;
	// Use this for initialization
	void Start ()
	{
		collider.isTrigger = true;
	}
	
	void OnTriggerEnter()
	{
        if (!hasBeenTriggered)
        {
            //SoundManager.PauseMusic("Pulser");
            SoundManager.PlaySound(SoundToPlay);
            hasBeenTriggered = true;

            
        }
	}
	
}
