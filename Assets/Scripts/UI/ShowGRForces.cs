/****************************************************
 * File: ShowGRForces.cs
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

public class ShowGRForces : MonoBehaviour
{
    private Text grForces;

    void Start()
    {
        grForces = this.GetComponent<Text>();
    }

    void Update()
    {
        grForces.text = "GRF - LF: " + FindObjectOfType<DeformTerrainMaster>().totalGRForceLeft.y.ToString("#.") +
            " N | RF: " + FindObjectOfType<DeformTerrainMaster>().totalGRForceRight.y.ToString("#.") + " N";
    }
}
