using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackedImageInfoRuntimeSaveManager : MonoBehaviour
{
    //[SerializeField] private GameObject prefabOnTrack;

    [Tooltip("The Scale of the displaying object.")]
    [SerializeField] private Vector3 scaleFactor = new Vector3(0.1f, 0.1f, 0.1f);

    [Tooltip("Gettting XRReferenceImageLibrary from ARTrackedImageManager Component .")]
    [SerializeField] private XRReferenceImageLibrary runtimeImageLibrary;

    private ARTrackedImageManager trackImageManager; //ARTrackedImageManager Component .

    [Tooltip("List of objects")]
    public List<GameObject> newList;
 
    void Start()
    { 

        foreach (GameObject arObject in newList)
        {
            GameObject newARObject = Instantiate(arObject, arObject.transform.position, arObject.transform.rotation);
            newARObject.name = arObject.name;
            newARObject.SetActive(false);
            Debug.Log("Ar Obj " + arObject.name);
        }

        trackImageManager = gameObject.AddComponent<ARTrackedImageManager>();
        trackImageManager.referenceLibrary = trackImageManager.CreateRuntimeLibrary(runtimeImageLibrary);
        trackImageManager.requestedMaxNumberOfMovingImages = 3;
        trackImageManager.enabled = true;

        trackImageManager.trackedImagesChanged += OnTrackedImagesChanged;

    }

    public void ExternalAddJob(Texture2D t)
    {
        StartCoroutine(AddImageJob(t));
    }

    void OnDisable()
    {
        trackImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    public IEnumerator AddImageJob(Texture2D texture2D)
    {
        yield return null;

        var firstGuid = new SerializableGuid(0, 0);
        var secondGuid = new SerializableGuid(0, 0);

        XRReferenceImage newImage = new XRReferenceImage(firstGuid, secondGuid, new Vector2(0.1f, 0.1f), texture2D.name, texture2D);
        
        try
        {
 
            MutableRuntimeReferenceImageLibrary mutableRuntimeReferenceImageLibrary = trackImageManager.referenceLibrary as MutableRuntimeReferenceImageLibrary;

            var jobHandle = mutableRuntimeReferenceImageLibrary.ScheduleAddImageJob(texture2D, texture2D.name, 0.1f);
 
            while (!jobHandle.IsCompleted)
            {
                //jobLog.text = "Job Running...";
                Debug.Log("Job Running...");
            }

        }
        catch (Exception e)
        {
            //debugLog.text = e.ToString();
            Debug.Log(e);
        }
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateARImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                UpdateARImage(trackedImage);
            }

        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {

            foreach (GameObject g in newList)
            {
                if (g.name == trackedImage.referenceImage.name)
                {
                    g.SetActive(false);
                }
            }
        }
    }

    public void UpdateARImage(ARTrackedImage trackedImage)
    {
        // Display the name of the tracked image in the canvas
 
        AssignGameObject(trackedImage.referenceImage.name, trackedImage.transform);
     }

    void AssignGameObject(string name, Transform newTransform)
    {

        foreach (GameObject g in newList)
        {
            if (g.name == name)
            {
                GameObject goARObject = g;
                goARObject.transform.position = newTransform.position;
                goARObject.transform.rotation = newTransform.rotation;
                goARObject.transform.GetChild(0).localScale = scaleFactor;

                goARObject.SetActive(true);
               
            }
            else
            {
                g.SetActive(false);

            }
        }
    }
}