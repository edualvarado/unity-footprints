using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowYoung : MonoBehaviour
{
    public Text youngValue;

    // Start is called before the first frame update
    private void Start()
    {
        youngValue = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        youngValue.text = FindObjectOfType<PhysicalFootprint>().youngModulus.ToString();
    }
}
