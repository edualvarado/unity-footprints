using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowWeightForces : MonoBehaviour
{
    private Text weightForces;

    // Start is called before the first frame update
    void Start()
    {
        weightForces = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        weightForces.text = "Weight Forces - LF: " + FindObjectOfType<DeformTerrainMaster>().weightForceLeft.y.ToString("#") + 
            " N | RF: " + FindObjectOfType<DeformTerrainMaster>().weightForceRight.y.ToString("#") + " N";
    }
}
