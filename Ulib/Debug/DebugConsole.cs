
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class DebugConsole : ComponentSingleton<DebugConsole>
{    
    private enum DebugMode
    {
        Console,
        Input,
        Watch,
    }

    private KeyCode openKey = KeyCode.BackQuote;
    private bool isShowing;
    private Rect windowRect = new Rect(0, 0, 250, 300),
                 dragRect = new Rect(0, 0, 10000, 10000);

    private DebugMode currentMode = DebugMode.Console;
    private Dictionary<string,KeyValuePair<object,string>> watchVaribles = new Dictionary<string, KeyValuePair<object,string>>(); 
    void OnGUI()
    {
        if(!isShowing)return;
        windowRect = GUI.Window(0, windowRect, DrawConsoleWindow, "Console");

    }

    void Update()
    {
        if (Input.GetKeyDown(openKey)) isShowing = !isShowing;
        
    }
    private void DrawConsoleWindow(int id)
    {
        GUILayout.BeginHorizontal();
        currentMode = (DebugMode) GUILayout.SelectionGrid((int) currentMode, Enum.GetNames(typeof(DebugMode)), 3);
        GUILayout.EndHorizontal();

        switch (currentMode)
        {
            case DebugMode.Console:
                DrawConsole();
                break;
            case DebugMode.Input:
                DrawInput();
                break;
            case DebugMode.Watch:
                DrawWatches();
                break;
        }



        GUI.DragWindow(dragRect);
    }

    private  void DrawConsole()
    {

    }

    private  void DrawInput()
    {

    }

    private  void DrawWatches()
    {
        string s = "";
        //Horizontal lables
        //One verticle scroll value
        foreach (KeyValuePair<string, KeyValuePair<object, string>> v in watchVaribles)
        {
            KeyValuePair<object, string> temp = v.Value;
            Debug.Log(temp);
            FieldInfo imfo = temp.Key.GetType().GetField(temp.Value);
            Debug.Log(imfo);
            s += v.Key + "\n" +  imfo.GetValue(temp.Key) + "\n";
        }
        GUILayout.TextArea(s);
    }

    private void GetKeyName()
    {
        var values = Enum.GetValues(typeof(KeyCode));

        foreach (KeyCode value in values)
        {
            if(Input.GetKeyDown(value))Debug.Log(value);
        }
    }

   
    public static void AddVarToWatch(string name, object container, string varName)
    {
        Instance.watchVaribles.Add(name, new KeyValuePair<object, string>(container,varName));
    }
    

}

