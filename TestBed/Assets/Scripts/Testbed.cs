using System;
using UnityEngine;
using System.Collections;

public class Testbed : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
        SoundManager.Instance.Init(true, true);
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(!Input.GetKeyDown(KeyCode.Space))return;
        SoundManager.PlaySoundQueue(new[]{"Large","Large"});
	}

    void DebugTest()
    {
        Debug.Log("Callback");
    }
}
