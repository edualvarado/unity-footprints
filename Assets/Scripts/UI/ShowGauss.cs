/****************************************************
 * File: ShowGauss.cs
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

public class ShowGauss : MonoBehaviour
{
    public Slider iterationsSlider;
    private Text iterationsValue;

    void Start()
    {
        iterationsValue = this.GetComponent<Text>();

    }

    void Update()
    {
        iterationsValue.text = (iterationsSlider.value).ToString();
    }
}
