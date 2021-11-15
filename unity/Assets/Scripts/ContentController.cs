using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;



public class ContentController : MonoBehaviour {

    public API api;
    public UIManager uiManager;

   

    public void LoadContent(string url, Vector3 assetPosition) {
        DestroyAllChildren();
        api.GetBundleObject(url, OnContentLoaded, assetPosition);
        uiManager.CloseObjectsPnl();
    }

    void OnContentLoaded(GameObject content) {
        //do something cool here
        Debug.Log("Loaded: " + content.name);
    }

    void DestroyAllChildren() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
}
