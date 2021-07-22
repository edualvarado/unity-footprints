using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowHits : MonoBehaviour
{
    private Text hits;

    // Start is called before the first frame update
    void Start()
    {
        hits = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        hits.text = "N° Hits - LF: " + FindObjectOfType<PhysicalFootprint>().counterHitsLeft.ToString("#") +
            " | RF: " + FindObjectOfType<PhysicalFootprint>().counterHitsRight.ToString("#");
    }
}
