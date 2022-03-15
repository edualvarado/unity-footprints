/****************************************************
 * File: DistributionBumpSphere.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Real-Time Locomotion on Soft Grounds with Dynamic Footprints
   * Last update: 07/02/2022
*****************************************************/

/* Code can be not updated */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistributionBumpSphere : MonoBehaviour
{
    #region Read-only && Static Fields

    // Types of brushes
    private DeformTerrainMasterSphere deformTerrainMaster;

    #endregion

    void Start()
    {
        deformTerrainMaster = this.GetComponent<DeformTerrainMasterSphere>();
    }

    void Update()
    {
        Debug.Log("[INFO] deformTerrainMaster");
    }
}
