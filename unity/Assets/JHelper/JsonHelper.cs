public class JsonHelper
{
	public static T GetData<T>(string json)
	{
		string newJson = "{\"data\":" + json + "}";
		Wrapper<T> w = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(newJson);
		return w.data;
	}

	[System.Serializable]
	class Wrapper<T>
	{
		public T data;
	}

	public static T[] GetArray<T>(string json)
	{
		string newJson = "{\"data\":" + json + "}";
		Swrapper<T> sw = UnityEngine.JsonUtility.FromJson<Swrapper<T>>(newJson);
		return sw.data;
	}

	[System.Serializable]
	class Swrapper<T>
	{
		public T[] data;
	}
}