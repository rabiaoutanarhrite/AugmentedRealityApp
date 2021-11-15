using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public string url;

    [SerializeField] private GameObject objectsPnl;
    [SerializeField] private GameObject burgerPnl;
    [SerializeField] private GameObject allCatPnls;
    [SerializeField] private GameObject categoriesGridPnl;
    [SerializeField] private GameObject categoriesListPnl;

    [SerializeField] private Button gridBtn;
    [SerializeField] private Button listBtn;

    [SerializeField] private GameObject GridbuttonTemplate;
    [SerializeField] private GameObject ListbuttonTemplate;

    [SerializeField] private GameObject aRPlaneManager;
    [SerializeField] private GameObject aRImageTrackingManager;
  
    [SerializeField] private GameObject startScanPnl;
    [SerializeField] private GameObject imageTrackingUI;
    [SerializeField] private GameObject productsUI;
    
    public GameObject loadingBar;
    public GameObject imageTrackingScanning;
    public GameObject productsScanning;

    [Serializable]
    public struct Categorie
    {
        public string name;
        public string type;
        public string category_url;
        //public string Description;
        public CatImage image;
        public Sprite Icon;
        //public string IconUrl; 
    }

    [Serializable]
    public struct CatImage
    {
        public string url;
    }

    Categorie[] allCategories;

    private void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        StartCoroutine(GetCategories());

        gridBtn.interactable = false;

        loadingBar.SetActive(true);
    }

    private void MakeButtons()
    {
        GameObject g;

        int N = allCategories.Length;

        for (int i = 0; i < N; i++)
        {
            g = Instantiate(GridbuttonTemplate, categoriesGridPnl.transform);
            g.transform.GetChild(0).GetComponent<Image>().sprite = allCategories[i].Icon;
            g.transform.GetChild(1).GetComponent<Text>().text = allCategories[i].name;
            
            g.GetComponent<Button>().AddEventListener(i, ItemClicked);
        }

        Destroy(GridbuttonTemplate);

        for (int i = 0; i < N; i++)
        {
            g = Instantiate(ListbuttonTemplate, categoriesListPnl.transform);
            g.transform.GetChild(0).GetComponent<Image>().sprite = allCategories[i].Icon;
            g.transform.GetChild(1).GetComponent<Text>().text = allCategories[i].name;
            
            g.GetComponent<Button>().AddEventListener(i, ItemClicked);
        }

        Destroy(ListbuttonTemplate); 
    }

    void ItemClicked(int itemIndex)
    {
         
        allCatPnls.SetActive(false);
        startScanPnl.SetActive(true);

        if (allCategories[itemIndex].type == "Horizontal")
        {
            imageTrackingUI.SetActive(false);
            productsUI.SetActive(true);

            aRPlaneManager.SetActive(true);
            aRImageTrackingManager.SetActive(false);
            aRPlaneManager.GetComponent<ARPlaneManager>().requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
        }

        else if (allCategories[itemIndex].type == "Vertical")
        {
            imageTrackingUI.SetActive(false);
            productsUI.SetActive(true);

            aRPlaneManager.SetActive(true);
            aRImageTrackingManager.SetActive(false);
            aRPlaneManager.GetComponent<ARPlaneManager>().requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Vertical;
        }

        else if (allCategories[itemIndex].type == "ImageTracking")
        {
            imageTrackingUI.SetActive(true);
            productsUI.SetActive(false);

            aRImageTrackingManager.SetActive(true);
            aRImageTrackingManager.GetComponent<TrackedImageInfoRuntimeSaveManager>().enabled = true;
            aRPlaneManager.SetActive(false);
        }

        url = allCategories[itemIndex].category_url;
    }

    IEnumerator GetCategories()
    {
        string url = "http://192.168.1.106:1337/categories";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.chunkedTransfer = false;
        yield return request.Send();

        if (request.isNetworkError)
        {
            //show message "no internet "
        }
        else
        {
            if (request.isDone)
            {
                allCategories = JsonHelper.GetArray<Categorie>(request.downloadHandler.text);
                StartCoroutine(GetCategoriesIcons());
            }
        }
    }

    IEnumerator GetCategoriesIcons()
    {
        for (int i = 0; i < allCategories.Length; i++)
        {
            WWW w = new WWW("http://192.168.1.106:1337" + allCategories[i].image.url);
            yield return w;

            if (w.error != null)
            {
                //error
                //show default image
                //allCategories[i].Icon = defaultIcon;
            }
            else
            {
                if (w.isDone)
                {
                    Texture2D tx = w.texture;
                    allCategories[i].Icon = Sprite.Create(tx, new Rect(0f, 0f, tx.width, tx.height), Vector2.zero, 10f);
                }
            }
        }

        MakeButtons();
        loadingBar.SetActive(false);

    }

    public void OpenCategoriesPnl()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CloseObjectsPnl()
    {
        objectsPnl.SetActive(false);
        burgerPnl.SetActive(true);
    }

    public void GridPnl()
    {
        categoriesGridPnl.SetActive(true);
        categoriesListPnl.SetActive(false);
        gridBtn.interactable = false;
        listBtn.interactable = true;

    }

    public void ListPnl()
    {
        categoriesGridPnl.SetActive(false);
        categoriesListPnl.SetActive(true);
        gridBtn.interactable = true; ;
        listBtn.interactable = false;
    } 
}
