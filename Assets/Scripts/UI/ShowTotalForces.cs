/****************************************************
 * File: ShowTotalForces.cs
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

public class ShowTotalForces : MonoBehaviour
{
    private Text totalForces;

    void Start()
    {
        totalForces = this.GetComponent<Text>();
    }

    void Update()
    {
        totalForces.text = "Total Foot Forces - LF: " + FindObjectOfType<DeformTerrainMaster>().totalForceLeftFoot.y.ToString("#") +
            " N | RF: " + FindObjectOfType<DeformTerrainMaster>().totalForceRightFoot.y.ToString("#") + " N";
    }
}
