using UnityEngine;

public class ComponentSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get { return instance ?? (instance = new GameObject(typeof(T).ToString()).AddComponent<T>()); }
    }

    public virtual void Init(){}
}
