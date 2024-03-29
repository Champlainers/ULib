using System;
using UnityEngine;
using System.Collections;

public class Testbed : MonoBehaviour 
{

	// Use this for initialization
    public GameObject testSubject1;
    public int i = 0;
	void Start () 
    {
        SoundManager.Instance.Init(true, true);
        KeyboardInputManager.Instance.Init();
        DebugConsole.Instance.Init();
	    DebugConsole.AddVarToWatch("Index", this, "i");
        KeyboardInputManager.SubscribeToEvent(KeyCode.Space, KeyEventType.OnPressed, Play3DOneShot);
        //KeyboardInputManager.SubscribeToEvent(KeyCode.W, KeyEventType.OnPressed, Play3DOneShot);
        //KeyboardInputManager.SubscribeToEvent(KeyCode.W, KeyEventType.OnReleased, DebugTest);
	}


    void PlaySoundQueue(object sender, EventArgs e)
    {
        SoundManager.Play3DSoundQueue(new[] {"Large", "Large"},testSubject1,1,"default",false);  
    }

    void Play3DOneShot(object sender, EventArgs e)
    {
        //SoundManager.Play3DSound(testSubject1, "Large");
        i++;
        Debug.Log(i);
    }

    void DebugTest(object sender, EventArgs e)
    {
        Debug.Log("Callback");
    }
}
