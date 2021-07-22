using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowGRForces : MonoBehaviour
{
    private Text grForces;

    // Start is called before the first frame update
    void Start()
    {
        grForces = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        grForces.text = "GRF - LF: " + FindObjectOfType<DeformTerrainMaster>().totalGRForceLeft.y.ToString("#.") +
            " N | RF: " + FindObjectOfType<DeformTerrainMaster>().totalGRForceRight.y.ToString("#.") + " N";
    }
}
