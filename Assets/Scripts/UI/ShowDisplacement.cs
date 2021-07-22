using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowDisplacement : MonoBehaviour
{
    private Text displacement;

    // Start is called before the first frame update
    void Start()
    {
        displacement = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        displacement.text = "Deform. - LF: " + ((FindObjectOfType<PhysicalFootprint>().heightCellDisplacementYoungLeft)*1000).ToString("#.#") +
            " mm | RF: " + ((FindObjectOfType<PhysicalFootprint>().heightCellDisplacementYoungRight)*1000).ToString("#.#") + " mm";
    }
}
