using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateCamera : MonoBehaviour
{

    public Transform rotateWithRespect;
    public float speed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.LookAt(rotateWithRespect);
        transform.Translate(Vector3.right * Time.fixedDeltaTime * speed);
    }
}
