using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPoisson : MonoBehaviour
{
    public Slider poissonSlider;
    private Text poissonValue;

    // Start is called before the first frame update
    void Start()
    {
        poissonValue = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        poissonValue.text = (poissonSlider.value).ToString();
    }
}
