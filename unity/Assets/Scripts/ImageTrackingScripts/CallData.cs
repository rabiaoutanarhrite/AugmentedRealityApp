using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using TMPro;

public class CallData : MonoBehaviour
{
	[Tooltip("The gameobject of Panel template.")]
	[SerializeField] private GameObject pnlTemp; //Panel of images information.

	[Tooltip("Default image if can't call data.")]
	[SerializeField] private Sprite defaultIcon; //Default image if can't call data

	private string url; //the url of your host .

	[Tooltip("Make a list of textures for adding them to ImageReferenceLibrary.")]
	public List<Texture2D> allTextures; //Make a list of textures for adding them to ImageReferenceLibrary.

	[Tooltip("Tracked Image Info Runtime Save Manager Script Component.")]
	public TrackedImageInfoRuntimeSaveManager arTrackedImageRuntime;

	[Tooltip("The prefab of objects parent.")]
	[SerializeField] private GameObject objectParent;

//*********************** Structs to get data from json file  **************************//
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

	//****************************************************************//

	void Start()
	{
		url = ShareUrl.Instance.url; //get host url from the singleton object

		UIManager.instance.loadingData.SetActive(true); //Activing the loading bar 

		//fetch data from Json
		StartCoroutine(GetData()); //Start getting data by using the Couroutine

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
			g = Instantiate(panelTemplate, this.transform);

			g.transform.GetChild(0).GetComponent<Image>().sprite = allInfos[i].icon;
			g.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = allInfos[i].name;
			
			byte[] textureBytes = File.ReadAllBytes(Application.persistentDataPath + "/" + allInfos[i].name);
			Texture2D t = new Texture2D(512, 512, TextureFormat.RGBA32, true);
			t.LoadImage(textureBytes);
 			t.Apply();
			
			t.name = allInfos[i].name;
			allTextures.Add(t);

			arTrackedImageRuntime.ExternalAddJob(t); //Adding Images textures to the XRReferenceImageLibrary for the processing.
		}

 		panelTemplate.SetActive(false);
	}

	//***************************************************
	IEnumerator GetData()
	{

		UnityWebRequest request = UnityWebRequest.Get(url + "/objects?experience_category.name=My%20Museum");
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
			
			WWW w = new WWW(url  + allInfos[i].marker.formats.small.url);
			Debug.Log(url + allInfos[i].marker.formats.small.url);
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
		UIManager.instance.loadingData.SetActive(false);
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

#if UNITY_ANDROID

			for (int j = 0; j < allInfos[i].asset_android.Length; j++)
            {
				Debug.Log(allInfos[i].asset_android[j].file.url);
				UnityWebRequest w = UnityWebRequestAssetBundle.GetAssetBundle(url + allInfos[i].asset_android[j].file.url);
				yield return w.SendWebRequest();

				if (w.result == UnityWebRequest.Result.ConnectionError)
				{
					//error 
					Debug.Log("Network error");
				}
				else
				{
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

#else

			for (int k = 0; k < allInfos[i].asset_ios.Length; k++)
			{
				Debug.Log(allInfos[i].asset_ios[k].file.url);
				UnityWebRequest w = UnityWebRequestAssetBundle.GetAssetBundle(url + allInfos[i].asset_ios[k].file.url);
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
							Vector3 assetPosition = new Vector3(allInfos[i].asset_ios[k].positionX / 10, allInfos[i].asset_ios[k].positionY / 10, allInfos[i].asset_ios[k].positionZ / 10);
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

#endif
			Debug.Log("Another platform!");

		}
	}

}