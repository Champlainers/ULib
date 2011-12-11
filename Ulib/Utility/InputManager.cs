using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class KeyEventArgs : EventArgs
{
    public KeyCode KeyPressed;
    public KeyEventType KeyEventType;

    public KeyEventArgs(KeyCode e, KeyEventType k)
    {
        KeyPressed = e;
        KeyEventType = k;
    }
}

public enum KeyEventType
{
    OnPressed,
    OnStay,
    OnReleased,
}

public class KeyboardInputManager : ComponentSingleton<KeyboardInputManager>
{
    public delegate void KeyEvent(object sender, KeyEventArgs e);

    private Dictionary<KeyCode, KeyEvent> keyPressEvents = new Dictionary<KeyCode, KeyEvent>();
    private Dictionary<KeyCode, KeyEvent> keyReleasedEvents = new Dictionary<KeyCode, KeyEvent>();
    private Dictionary<KeyCode, KeyEvent> keyStayEvents = new Dictionary<KeyCode, KeyEvent>();

    public static  void SubscribeToEvent(KeyCode key, KeyEventType type, KeyEvent listener = null)
    {
        switch (type)
        {
            case KeyEventType.OnPressed:
                SubscribePressed(key, listener);
                break;
            case KeyEventType.OnReleased:
                SubscribeReleased(key,listener);
                break;
            case KeyEventType.OnStay:
                SubscribeStay(key,listener);
                break;
        }
       
    }
    
    public static void UnsubscribeToEvent(KeyCode key, KeyEventType type, KeyEvent listener)
    {
        switch (type)
        {
            case KeyEventType.OnPressed:
                UnsubscribePressed(key, listener);
                break;
            case KeyEventType.OnReleased:
                UnsubscribeReleased(key, listener);
                break;
            case KeyEventType.OnStay:
                UnsubscribeStay(key, listener);
                break;
        }
    }
    
    #region  Helpers
    private static void SubscribePressed(KeyCode key, KeyEvent listener)
    {
        if(!Instance.keyPressEvents.ContainsKey(key))
            Instance.keyPressEvents.Add(key,null);

        if (listener == null) return;
            Instance.keyPressEvents[key] += listener;   
    }

    private static void SubscribeReleased(KeyCode key, KeyEvent listener)
    {
        if (!Instance.keyReleasedEvents.ContainsKey(key))
            Instance.keyReleasedEvents.Add(key, null);

        if (listener == null) return;
        Instance.keyReleasedEvents[key] += listener;
    }

    private static void SubscribeStay(KeyCode key, KeyEvent listener)
    {
        if (!Instance.keyStayEvents.ContainsKey(key))
            Instance.keyStayEvents.Add(key, null);

        if (listener == null) return;
        Instance.keyStayEvents[key] += listener;
    }

    private static void UnsubscribePressed(KeyCode key, KeyEvent listener)
    {
        if (!Instance.keyPressEvents.ContainsKey(key)) return;

        Instance.keyPressEvents[key] -= listener;
    }

    private static void UnsubscribeReleased(KeyCode key, KeyEvent listener)
    {
        if (!Instance.keyPressEvents.ContainsKey(key)) return;

        Instance.keyPressEvents[key] -= listener;
    }

    private static void UnsubscribeStay(KeyCode key, KeyEvent listener)
    {
        if (!Instance.keyPressEvents.ContainsKey(key)) return;

        Instance.keyPressEvents[key] -= listener;
    }
    #endregion
    public void Update()
    {
        foreach (KeyValuePair<KeyCode, KeyEvent> keyboardPressed in keyPressEvents)
        {
            if (Input.GetKeyDown(keyboardPressed.Key))
                keyboardPressed.Value(this, new KeyEventArgs(keyboardPressed.Key,KeyEventType.OnPressed));
           
            
        }

        foreach (KeyValuePair<KeyCode, KeyEvent> keyboardReleased in keyReleasedEvents)
        {
             if (Input.GetKeyUp(keyboardReleased.Key))
            {
                keyboardReleased.Value(this, new KeyEventArgs(keyboardReleased.Key, KeyEventType.OnReleased));
            }
        }

        foreach (KeyValuePair<KeyCode, KeyEvent> keyboardStay in keyStayEvents)
        {
            if (Input.GetKey(keyboardStay.Key))
            {
                keyboardStay.Value(this, new KeyEventArgs(keyboardStay.Key, KeyEventType.OnStay));

            }
        }
    }
}

