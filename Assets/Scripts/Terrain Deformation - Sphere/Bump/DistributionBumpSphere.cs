using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// For the moment not used

public class DistributionBumpSphere : MonoBehaviour
{
    #region Variables

    // Types of brushes
    private DeformTerrainMasterSphere deformTerrainMaster;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        deformTerrainMaster = this.GetComponent<DeformTerrainMasterSphere>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("deformTerrainMaster");
    }
}
