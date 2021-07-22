using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetYoung : MonoBehaviour
{
    private Slider youngSlider;

    // Start is called before the first frame update
    void Start()
    {
        youngSlider = this.GetComponent<Slider>();
        youngSlider.minValue = 100000;
        youngSlider.maxValue = 1000000;
        youngSlider.value = 1000000;

    }
}
