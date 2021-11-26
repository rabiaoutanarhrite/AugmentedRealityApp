using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine.Networking;

public static class ButtonExtension
{
	public static void AddEventListener<T>(this Button button, T param, Action<T> OnClick)
	{
		button.onClick.AddListener(delegate () {
			OnClick(param);
		});
	}
}

public class CallModelData : MonoBehaviour
{
	

	public UIManager uIManager;
	public List<string> assetsUrl;
	public List<Vector3> assetsPosition;
	
	[SerializeField] Sprite defaultIcon;

	[Serializable]
	public struct Object
	{
		public string name;
		public string description;
		public AssetAndroid[] asset_android;
		public AssetIos[] asset_ios;
		public Marker marker;
		public Sprite icon;
	}

	[Serializable]
	public struct AssetAndroid
	{
		public float positionX;
		public float positionY;
		public float positionZ;
		public string Name;
		public File file;
	}

	[Serializable]
	public struct AssetIos
	{
		public float positionX;
		public float positionY;
		public float positionZ;
		public string Name;
		public File file;
	}

	[Serializable]
	public struct File
	{
		public string url;
	}

	[Serializable]
	public struct Marker
	{
		public MarkerFormat formats;
	}

	[Serializable]
	public struct MarkerFormat
    {
		public Thumbnail thumbnail; 
    }

	[Serializable]
	public struct Thumbnail
    {
		public string url;
    }

	Object[] allObjects;
 
	void Start()
	{
		UIManager.instance.loadingData.SetActive(true);
		//fetch data from Json
		StartCoroutine(GetObjects());
	}

    private void OnEnable()
    {
		StartCoroutine(StopAnimation());    
    }

    void DrawUI()
	{
		GameObject buttonTemplate = transform.GetChild(0).gameObject;
		GameObject g;

		int N = allObjects.Length;

		for (int i = 0; i < N; i++)
		{
			g = Instantiate(buttonTemplate, transform);
			g.transform.GetChild(0).GetComponent<Image>().sprite = allObjects[i].icon;
			g.transform.GetChild(1).GetComponent<Text>().text = allObjects[i].name;
			//g.name = allObjects[i].asset.file.url;


#if UNITY_ANDROID 

			int A = allObjects[i].asset_android.Length;

			for (int j = 0; j < A; j++)
            {
				assetsUrl.Add(allObjects[i].asset_android[j].file.url);

				float x, y, z;

				Vector3 pos;


				Debug.Log("Asset.length: " + allObjects[i].asset_android.Length.GetType());

				if (allObjects[i].asset_android.Length != 0)
                {
					Debug.Log("X: " + allObjects[i].asset_android[j].positionX + "Y: " + allObjects[i].asset_android[j].positionY + "Z: " + allObjects[i].asset_android[j].positionZ);
 
					x = allObjects[i].asset_android[j].positionX;
					y = allObjects[i].asset_android[j].positionY;
					z = allObjects[i].asset_android[j].positionZ;
					 
					pos = new Vector3(x, y, z);

					assetsPosition.Add(pos); 
				}
                else
                {
					Debug.Log("Empty array");
                }

				g.transform.GetComponent<ButtonAction>().myAssetsPosition = assetsPosition;
				g.transform.GetComponent<ButtonAction>().myAssetsUrls = assetsUrl;
			}


					
#else
			int A = allObjects[i].asset_ios.Length;

			for (int j = 0; j < A; j++)
            {
				assetsUrl.Add(allObjects[i].asset_ios[j].file.url);
            }
			g.transform.GetComponent<ButtonAction>().myAssetsUrls = assetsUrl;
#endif
			Debug.Log("Another platform!");

			//Debug.Log("Requesting bundle at " + bundleURL);

			g.GetComponent<Button>().AddEventListener(i, ItemClicked);
		}

		Destroy(buttonTemplate);
	}

	void ItemClicked(int itemIndex)
	{
		//Debug.Log("name " + allObjects[itemIndex].Name);
	}

	//***************************************************
	IEnumerator GetObjects()
	{
		UnityWebRequest request = UnityWebRequest.Get(uIManager.url);
		//request.chunkedTransfer = false;
		yield return request.SendWebRequest();

		if (request.result == UnityWebRequest.Result.ConnectionError)
		{
			//show message "no internet "
		}
		else
		{
			if (request.isDone)
			{
				allObjects = JsonHelper.GetArray<Object>(request.downloadHandler.text);
				StartCoroutine(GetObjectsIcones());
			}
		}
	}

	IEnumerator GetObjectsIcones()
	{
		for (int i = 0; i < allObjects.Length; i++)
		{
			WWW w = new WWW("http://192.168.1.106:1337" + allObjects[i].marker.formats.thumbnail.url);

			yield return w;

			if (w.error != null)
			{
				//error
				//show default image
				allObjects[i].icon = defaultIcon;
			}
			else
			{
				if (w.isDone)
				{
					Texture2D tx = w.texture;
					allObjects[i].icon = Sprite.Create(tx, new Rect(0f, 0f, tx.width, tx.height), Vector2.zero, 10f);
				}
			}
		}

		DrawUI();
		UIManager.instance.loadingData.SetActive(false);
		StartCoroutine(StopAnimation());
 	}

	IEnumerator StopAnimation()
	{
		UIManager.instance.productsScanning.SetActive(true);

		yield return new WaitForSeconds(7f);

		UIManager.instance.productsScanning.SetActive(false);
	}

}