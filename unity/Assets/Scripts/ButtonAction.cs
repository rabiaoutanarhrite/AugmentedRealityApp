using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ButtonAction : MonoBehaviour
{
    public Text title;

    public ContentController contentController;

    public List<string> myAssetsUrls;
    public List<Vector3> myAssetsPosition;


    private Button button;


    private void Start()
    {
        button = this.GetComponent<Button>();
        int s = 0;
        var urlsAndPositions = myAssetsUrls.Zip(myAssetsPosition, (u, p) => new { myAssetsUrls = u, myAssetsPosition = p });

        foreach (var up in urlsAndPositions)
        {
            button.onClick.AddListener(delegate () { contentController.LoadContent(up.myAssetsUrls, up.myAssetsPosition); });
            Debug.Log("Round : " + s++);
        } 

         
    }

 
}