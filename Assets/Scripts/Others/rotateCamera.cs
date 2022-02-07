/****************************************************
 * File: RotateCamera.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Real-Time Locomotion on Soft Grounds with Dynamic Footprints
   * Last update: 07/02/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public Transform rotateWithRespect;
    public float speed = 1f;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        transform.LookAt(rotateWithRespect);
        transform.Translate(Vector3.right * Time.fixedDeltaTime * speed);
    }
}
