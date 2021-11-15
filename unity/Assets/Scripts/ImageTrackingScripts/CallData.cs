using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class CallData : MonoBehaviour
{
	[SerializeField] private GameObject contentGObject;

	[SerializeField] private GameObject pnlTemp;

	[SerializeField] private Sprite defaultIcon;

	[SerializeField] private Text infoText;
	
	public List<Texture2D> allTextures;

	public TrackedImageInfoRuntimeSaveManager arTrackedImageRuntime;

	[SerializeField] private GameObject objectParent;

	[Serializable]
	public struct Info
	{
		public string name;
		public string description;
		public AssetAndroid[] asset_android;
		public AssetIos[] asset_ios;
		public Marker marker;
		public Sprite icon;
		public Texture2D image;
	}

	[Serializable]
	public struct AssetAndroid
	{
		public float positionX;
		public float positionY;
		public float positionZ;
		public float rotationX;
		public float rotationY;
		public float rotationZ;
		public string Name;
		public Data file;
	}

	[Serializable]
	public struct AssetIos
	{
		public float positionX;
		public float positionY;
		public float positionZ;
		public float rotationX;
		public float rotationY;
		public float rotationZ;
		public string Name;
		public Data file;
	}

	[Serializable]
	public struct Data
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
		public Small small;
	}

	[Serializable]
	public struct Small
	{
		public string url;
	}

	Info[] allInfos;

	void Start()
	{
		UIManager.instance.loadingBar.SetActive(true);

		//fetch data from Json
		StartCoroutine(GetData());
		Debug.Log(Application.persistentDataPath);

	}

    private void OnEnable()
    {
		StartCoroutine(StopAnimation());
	}

	void DrawUI()
	{
		GameObject panelTemplate = pnlTemp;
		GameObject g;

		int N = allInfos.Length;

		for (int i = 0; i < N; i++)
		{
			g = Instantiate(panelTemplate, contentGObject.transform);

			g.transform.GetChild(0).GetComponent<Image>().sprite = allInfos[i].icon;
			g.transform.GetChild(1).GetComponent<Text>().text = allInfos[i].name;
			
			byte[] textureBytes = File.ReadAllBytes(Application.persistentDataPath + "/" + allInfos[i].name);
			Texture2D t = new Texture2D(512, 512, TextureFormat.RGBA32, true);
			t.LoadImage(textureBytes);
 			t.Apply();
			
			t.name = allInfos[i].name;
			allTextures.Add(t);

			arTrackedImageRuntime.ExternalAddJob(t);
 			Debug.Log("Texture format: " + t.format + " W: " + t.width + "H: " + t.height);
		}

 		panelTemplate.SetActive(false);
	}

	//***************************************************
	IEnumerator GetData()
	{
		string url = "http://192.168.1.106:1337/objects?experience_category.name=My%20Museum";

		UnityWebRequest request = UnityWebRequest.Get(url);
		request.chunkedTransfer = false;
		yield return request.Send();

		if (request.isNetworkError)
		{
			//show message "no internet "
			Debug.Log("No Internet");
		}
		else
		{
			if (request.isDone)
			{
				allInfos = JsonHelper.GetArray<Info>(request.downloadHandler.text);
				StartCoroutine(GetImages());
				StartCoroutine(GetBundleAssets());
			}
		}
		arTrackedImageRuntime.enabled = true;
	}

	IEnumerator GetImages()
	{

		for (int i = 0; i < allInfos.Length; i++)
		{
			
			WWW w = new WWW("http://192.168.1.106:1337"  + allInfos[i].marker.formats.small.url);
			Debug.Log("http://192.168.1.106:1337" + allInfos[i].marker.formats.small.url);
			yield return w;

			if (w.error != null)
			{
				//error
				//show default image
				allInfos[i].icon = defaultIcon;
			}
			else
			{
				if (w.isDone)
				{
					Texture2D tx;

					tx = w.texture;
					tx.name = allInfos[i].name;

					allInfos[i].icon = Sprite.Create(tx, new Rect(0f, 0f, tx.width, tx.height), Vector2.zero, 10f);

					byte[] textureBytes = tx.EncodeToJPG();
					File.WriteAllBytes(Application.persistentDataPath + "/" + allInfos[i].name, textureBytes);
				}
			}
		}

		DrawUI();
		UIManager.instance.loadingBar.SetActive(false);
		StartCoroutine(StopAnimation());
	}

	IEnumerator StopAnimation()
	{
		UIManager.instance.imageTrackingScanning.SetActive(true);

		yield return new WaitForSeconds(5f);

		UIManager.instance.imageTrackingScanning.SetActive(false);
	}

	IEnumerator GetBundleAssets()
    {

		for (int i = 0; i < allInfos.Length; i++)
		{
			for (int j = 0; j < allInfos[i].asset_android.Length; j++)
            {
				Debug.Log(allInfos[i].asset_android[j].file.url);
				UnityWebRequest w = UnityWebRequestAssetBundle.GetAssetBundle("http://192.168.1.106:1337" + allInfos[i].asset_android[j].file.url);
				yield return w.SendWebRequest();

				if (w.result == UnityWebRequest.Result.ConnectionError)
				{
					//error 
					Debug.Log("Network error");
				}
				else
				{
					//tx = new Texture2D(512, 512, TextureFormat.ARGB32, false);
					if (w.isDone)
					{
						AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(w);
						if (bundle != null)
						{
							GameObject objParent = Instantiate(objectParent, Vector3.zero, Quaternion.identity);
							Vector3 assetPosition = new Vector3(allInfos[i].asset_android[j].positionX / 10, allInfos[i].asset_android[j].positionY / 10, allInfos[i].asset_android[j].positionZ / 10);
							//Quaternion assetRotation = Quaternion.Euler(allInfos[i].asset_android[j].rotationX, allInfos[i].asset_android[j].rotationY, allInfos[i].asset_android[j].rotationZ);
							string rootAssetPath = bundle.GetAllAssetNames()[0];
							GameObject arObject = Instantiate(bundle.LoadAsset(rootAssetPath) as GameObject, assetPosition, Quaternion.identity, objParent.transform);
							bundle.Unload(false);
 							
							objParent.name = allInfos[i].name;
							arTrackedImageRuntime.newList.Add(objParent);
							objParent.SetActive(false);


							Debug.Log("Pos: " + arObject.transform.position);

						}
						else
						{
							Debug.Log("Not a valid asset Bundle");

						}

					}
				}
			}
		}
	}

}