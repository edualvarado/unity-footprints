/****************************************************
 * File: DistributionBump.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Real-Time Locomotion on Soft Grounds with Dynamic Footprints
   * Last update: 07/02/2022
*****************************************************/

/* In progress - Not used for the moment */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistributionBump : MonoBehaviour
{
    #region Read-only && Static Fields

    // Types of brushes
    private DeformTerrainMaster deformTerrainMaster;

    #endregion

    #region Unity Methods

    void Start()
    {
        deformTerrainMaster = this.GetComponent<DeformTerrainMaster>();
    }

    void Update()
    {
        Debug.Log("[INFO] deformTerrainMaster");
    }

    #endregion
}
