/****************************************************
 * File: ShowPressure.cs
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

public class ShowPressure : MonoBehaviour
{
    private Text pressure;

    void Start()
    {
        pressure = this.GetComponent<Text>();

    }

    void Update()
    {
        pressure.text = "Pressure - LF: " + FindObjectOfType<PhysicalFootprint>().pressureStressLeft.ToString("#") +
            " N | RF: " + FindObjectOfType<PhysicalFootprint>().pressureStressRight.ToString("#") + " N";
    }
}
