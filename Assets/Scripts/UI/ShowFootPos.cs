using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowFootPos : MonoBehaviour
{
    private Text footPositions;

    // Start is called before the first frame update
    void Start()
    {
        footPositions = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        footPositions.text = "Foot Pos - LF: " + FindObjectOfType<DeformTerrainMaster>().centerGridLeftFootHeight.ToString("#") + 
            " | RF: " + FindObjectOfType<DeformTerrainMaster>().centerGridLeftFootHeight.ToString("#");
    }
}
