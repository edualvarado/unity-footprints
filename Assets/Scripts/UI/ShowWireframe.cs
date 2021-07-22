using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowWireframe : MonoBehaviour
{
    [SerializeField] private Toggle activateWireframe;

    void OnPreRender()
    {
        if(activateWireframe.isOn)
            GL.wireframe = true;
    }
    void OnPostRender()
    {
        if (activateWireframe.isOn)
            GL.wireframe = false;
    }
}
