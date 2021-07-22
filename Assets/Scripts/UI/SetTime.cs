using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetTime : MonoBehaviour
{
    private Slider timeSlider;

    // Start is called before the first frame update
    void Start()
    {
        timeSlider = this.GetComponent<Slider>();
        timeSlider.minValue = 0.01f;
        timeSlider.maxValue = 1f;
    }
}
