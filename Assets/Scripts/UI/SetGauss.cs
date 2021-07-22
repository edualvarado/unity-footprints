using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetGauss : MonoBehaviour
{
    private Slider iterationsSlider;

    // Start is called before the first frame update
    void Start()
    {
        iterationsSlider = this.GetComponent<Slider>();
        iterationsSlider.minValue = 1;
        iterationsSlider.maxValue = 15;
        iterationsSlider.value = 1;
    }
}
