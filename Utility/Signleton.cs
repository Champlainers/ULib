// A generic singleton used in classes that are not components

public class Singleton<T> where T : class, new()
{
    private static T instance;

    public static T Instance
    { 
        get 
        {
            if (instance == null)
            {
                instance = new T();
            }
            return instance ;
        }
    }
}

