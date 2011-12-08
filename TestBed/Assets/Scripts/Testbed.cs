using System;
using UnityEngine;
using System.Collections;

public class Testbed : MonoBehaviour 
{

	// Use this for initialization
    public GameObject testSubject1;
	void Start () 
    {
        SoundManager.Instance.Init(true, true);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SoundManager.Play3DSoundQueue(new[] {"Large", "Large"},testSubject1);
        }
        else if(Input.GetKeyDown(KeyCode.W))
        {
            SoundManager.Play3DSound(testSubject1,"Large");
        }
    }

    void DebugTest()
    {
        Debug.Log("Callback");
    }
}
