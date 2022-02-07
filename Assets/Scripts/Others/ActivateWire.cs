/****************************************************
 * File: ActivateWire.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Real-Time Locomotion on Soft Grounds with Dynamic Footprints
   * Last update: 07/02/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivateWire : MonoBehaviour
{
    public bool activateWireframe;

    void OnPreRender()
    {
        if (activateWireframe)
            GL.wireframe = true;
    }
    void OnPostRender()
    {
        if (activateWireframe)
            GL.wireframe = false;
    }
}
