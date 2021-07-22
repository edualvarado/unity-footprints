using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTotalForces : MonoBehaviour
{
    private Text totalForces;

    // Start is called before the first frame update
    void Start()
    {
        totalForces = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        totalForces.text = "Total Foot Forces - LF: " + FindObjectOfType<DeformTerrainMaster>().totalForceLeftFoot.y.ToString("#") +
            " N | RF: " + FindObjectOfType<DeformTerrainMaster>().totalForceRightFoot.y.ToString("#") + " N";
    }
}
