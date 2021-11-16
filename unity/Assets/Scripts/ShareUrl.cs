using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShareUrl : MonoBehaviour
{
    private static ShareUrl _instance;

    [Tooltip("Put the Url of your host")]
    public string url;

    public static ShareUrl Instance { get { return _instance; } }
     
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        DontDestroyOnLoad(this.gameObject);

    }
}
