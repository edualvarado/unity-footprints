using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMomentumForces : MonoBehaviour
{
    private Text momentumForces;

    // Start is called before the first frame update
    void Start()
    {
        momentumForces = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        momentumForces.text = "Momentum Forces - LF: " + FindObjectOfType<DeformTerrainMaster>().momentumForceLeft.y.ToString("#") +
            " N | RF: " + FindObjectOfType<DeformTerrainMaster>().momentumForceRight.y.ToString("#") + " N";
    }
}
