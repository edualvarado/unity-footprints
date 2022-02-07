/****************************************************
 * File: SetGauss.cs
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

public class SetGauss : MonoBehaviour
{
    private Slider iterationsSlider;

    void Start()
    {
        iterationsSlider = this.GetComponent<Slider>();
        iterationsSlider.minValue = 1;
        iterationsSlider.maxValue = 15;
        iterationsSlider.value = 1;
    }
}
