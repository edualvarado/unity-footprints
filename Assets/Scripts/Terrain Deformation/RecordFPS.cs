/****************************************************
 * File: RecordFPS.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Real-Time Locomotion on Soft Grounds with Dynamic Footprints
   * Last update: 07/02/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordFPS : MonoBehaviour
{
    public int m_frameCounter = 0;
    public float m_timeCounter = 0.0f;
    public float m_lastFramerate = 0.0f;
    public float m_refreshTime = 0.5f;

    public int real_fps;
    public int min_fps = 300;
    public int max_fps = 0;

    // Start is called before the first frame update
    void Start()
    {
        //fpsValue = this.GetComponent<Text>();
        //max_fps = (int)(1.0f / Time.smoothDeltaTime);
        //min_fps = (int)(1.0f / Time.smoothDeltaTime);
    }

    // Update is called once per frame
    void Update()
    {

        if (real_fps > max_fps)
            max_fps = real_fps;

        if (m_timeCounter < m_refreshTime)
        {
            m_timeCounter += Time.deltaTime;
            m_frameCounter++;
        }
        else
        {
            //This code will break if you set your m_refreshTime to 0, which makes no sense
            m_lastFramerate = (float)m_frameCounter / m_timeCounter;
            m_frameCounter = 0;
            m_timeCounter = 0.0f;

            if (m_lastFramerate > max_fps)
                max_fps = (int)m_lastFramerate;

            if ((m_lastFramerate < min_fps))
                min_fps = (int)m_lastFramerate;
        }

        //fpsValue.text = m_lastFramerate.ToString("#") + " FPS";
    }
}