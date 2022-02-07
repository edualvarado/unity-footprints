/****************************************************
 * File: ShowFootPos.cs
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

public class ShowFootPos : MonoBehaviour
{
    private Text footPositions;

    void Start()
    {
        footPositions = this.GetComponent<Text>();
    }

    void Update()
    {
        footPositions.text = "Foot Pos - LF: " + FindObjectOfType<DeformTerrainMaster>().centerGridLeftFootHeight.ToString("#") + 
            " | RF: " + FindObjectOfType<DeformTerrainMaster>().centerGridLeftFootHeight.ToString("#");
    }
}
