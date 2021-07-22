using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowGauss : MonoBehaviour
{
    public Slider iterationsSlider;
    private Text iterationsValue;

    // Start is called before the first frame update
    void Start()
    {
        iterationsValue = this.GetComponent<Text>();

    }

    // Update is called once per frame
    void Update()
    {
        iterationsValue.text = (iterationsSlider.value).ToString();
    }
}
