using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingRotation : MonoBehaviour
{
    [SerializeField] float speed = 60;
   
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * speed * Time.deltaTime, Space.World);
    }
}
