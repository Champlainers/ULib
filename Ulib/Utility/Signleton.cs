// A generic singleton used in classes that are not components

public class Singleton<T> where T : class, new()
{
    private static T instance;

    public static T Instance
    { 
        get { return instance ?? (instance = new T()); }
    }
}

