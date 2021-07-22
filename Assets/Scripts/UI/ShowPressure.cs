using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPressure : MonoBehaviour
{
    private Text pressure;

    // Start is called before the first frame update
    void Start()
    {
        pressure = this.GetComponent<Text>();

    }

    // Update is called once per frame
    void Update()
    {
        pressure.text = "Pressure - LF: " + FindObjectOfType<PhysicalFootprint>().pressureStressLeft.ToString("#") +
            " N | RF: " + FindObjectOfType<PhysicalFootprint>().pressureStressRight.ToString("#") + " N";

    }
}
