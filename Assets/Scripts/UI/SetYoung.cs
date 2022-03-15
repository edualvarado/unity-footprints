/****************************************************
 * File: SetYoung.cs
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

public class SetYoung : MonoBehaviour
{
    private Slider youngSlider;

    void Start()
    {
        youngSlider = this.GetComponent<Slider>();
        youngSlider.minValue = 100000;
        youngSlider.maxValue = 1000000;
        youngSlider.value = 1000000;
    }
}
