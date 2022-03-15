/****************************************************
 * File: ShowMomentumForces.cs
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

public class ShowMomentumForces : MonoBehaviour
{
    private Text momentumForces;

    void Start()
    {
        momentumForces = this.GetComponent<Text>();
    }

    void Update()
    {
        momentumForces.text = "Momentum Forces - LF: " + FindObjectOfType<DeformTerrainMaster>().momentumForceLeft.y.ToString("#") +
            " N | RF: " + FindObjectOfType<DeformTerrainMaster>().momentumForceRight.y.ToString("#") + " N";
    }
}
