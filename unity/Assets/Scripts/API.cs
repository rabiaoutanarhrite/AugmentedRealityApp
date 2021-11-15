using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class API : MonoBehaviour {

    const string BundleFolder = "http://localhost:1337";
     
    public void GetBundleObject(string assetName, UnityAction<GameObject> callback, Vector3 bundleParent) {
        StartCoroutine(GetDisplayBundleRoutine(assetName, callback, bundleParent));
    }

    IEnumerator GetDisplayBundleRoutine(string assetName, UnityAction<GameObject> callback, Vector3 bundleParent) {

        string bundleURL = BundleFolder + assetName;


        //request asset bundle
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError) {
            Debug.Log("Network error");
        } else {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            if (bundle != null) {
                string rootAssetPath = bundle.GetAllAssetNames()[0];
                GameObject arObject = Instantiate(bundle.LoadAsset(rootAssetPath) as GameObject, bundleParent, Quaternion.identity);
                bundle.Unload(false);
                callback(arObject);
            } else {
                Debug.Log("Not a valid asset bundle");
            }
        }
    }
}
