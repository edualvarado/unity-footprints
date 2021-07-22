using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPoisson : MonoBehaviour
{
    private Slider poissonSlider;

    // Start is called before the first frame update
    void Start()
    {
        poissonSlider = this.GetComponent<Slider>();
        poissonSlider.minValue = 0.1f;
        poissonSlider.maxValue = 0.5f;
    }
}
