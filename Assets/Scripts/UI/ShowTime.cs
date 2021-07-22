using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTime : MonoBehaviour
{
    public Slider timeSlider;
    private Text timeValue;

    // Start is called before the first frame update
    void Start()
    {
        timeValue = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        timeValue.text = (timeSlider.value).ToString() + " s";
    }
}
