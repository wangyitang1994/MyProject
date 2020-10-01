using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T instance;
    public static T Instance{
        get{
            return instance;
        }
    }

    protected virtual void Awake(){
        if(instance == null){
            instance = this as T;
        }
        else{
            Debug.LogError("实例化了两次 type:"+this.GetType());
        }
    }
}