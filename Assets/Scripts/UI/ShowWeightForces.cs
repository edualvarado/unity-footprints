/****************************************************
 * File: ShowWeightForces.cs
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

public class ShowWeightForces : MonoBehaviour
{
    private Text weightForces;

    void Start()
    {
        weightForces = this.GetComponent<Text>();
    }

    void Update()
    {
        weightForces.text = "Weight Forces - LF: " + FindObjectOfType<DeformTerrainMaster>().weightForceLeft.y.ToString("#") + 
            " N | RF: " + FindObjectOfType<DeformTerrainMaster>().weightForceRight.y.ToString("#") + " N";
    }
}
