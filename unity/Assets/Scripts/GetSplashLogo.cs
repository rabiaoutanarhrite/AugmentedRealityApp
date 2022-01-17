using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
 

public class GetSplashLogo : MonoBehaviour
{

    [SerializeField] private float timeWait = 4f;

    [SerializeField] private Image logoImg;

    [SerializeField] private Image testImg;

    [SerializeField] private TextMeshProUGUI logoName;

    [SerializeField] private Sprite defaultIcon;

    [SerializeField] private Image bgPanel;

    [SerializeField] private Animator anim;


    [Serializable]
    public struct StrapiData
    {
        //public string Logo_Image;
        public LogoImage Logo_Image;
        public string Logo_Name;
        public string Background_Color;
        public Sprite Logo;
    }

    [Serializable]
    public struct LogoImage
    {
        public string url; 
    }


    StrapiData strapiData;

    LogoImage imageInfo; 

    private string url;

   
    void Start()
    {
        url = ShareUrl.Instance.url;
        Debug.Log(url);
        StartCoroutine(GetAppInfo()); 
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(timeWait);

        SceneManager.LoadScene(1);
    }

    void ShowData()
    {
        logoImg.sprite = strapiData.Logo;
        logoImg.preserveAspect = true;
        logoName.text = strapiData.Logo_Name;
        bgPanel.color = GetColorFromString(strapiData.Background_Color);
    }

    IEnumerator GetAppInfo()
    {

        UnityWebRequest request = UnityWebRequest.Get(url + "/splash-settings");
        request.chunkedTransfer = false;
        yield return request.Send();

        if(request.isNetworkError)
        {

        }
        else
        {
            if (request.isDone)
            {
                strapiData = JsonHelper.GetData<StrapiData>(request.downloadHandler.text);
                StartCoroutine(GetAppLogo());
            }
        }
    }

    IEnumerator GetAppLogo()
    {
        imageInfo = strapiData.Logo_Image;

        WWW w = new WWW(url + imageInfo.url);
        yield return w;

        if(w.error != null)
        {
         }
        else
        {
            if (w.isDone)
            {
                Texture2D tx = w.texture;
             
                strapiData.Logo = Sprite.Create(tx, new Rect(0f, 0f, tx.width, tx.height), Vector2.zero, 10f);
                anim.GetComponent<Animator>().enabled = true;
            }
        }

        ShowData();
        StartCoroutine(Wait());
    }

    private int HexToDec(string hex)
    {
        int dec = System.Convert.ToInt32(hex, 16);
        return dec;
    } 

    private float HexToFloatNormalized(string hex)
    {
        return HexToDec(hex) / 255f;
    }

    private Color GetColorFromString(string hexString)
    {
        float red = HexToFloatNormalized(hexString.Substring(0, 2));
        float green = HexToFloatNormalized(hexString.Substring(2, 2));
        float blue = HexToFloatNormalized(hexString.Substring(4, 2));
        return new Color(red, green, blue);
    }
}
