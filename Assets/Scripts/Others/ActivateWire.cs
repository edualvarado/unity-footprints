using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivateWire : MonoBehaviour
{
    public bool activateWireframe;

    void OnPreRender()
    {
        if (activateWireframe)
            GL.wireframe = true;
    }
    void OnPostRender()
    {
        if (activateWireframe)
            GL.wireframe = false;
    }
}
