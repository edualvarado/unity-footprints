/****************************************************
 * File: DrawForce.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Physically-driven Footprints Generation for Real-Time Interactions between a Character and Deformable Terrains
*****************************************************/

using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Functions to draw debug vectors
/// </summary>
public static class DrawForce
{
    public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowLength, float lengthMultiplier = 1f, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction * Math.Abs(arrowLength) * lengthMultiplier);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction * arrowLength * lengthMultiplier, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction * arrowLength * lengthMultiplier, left * arrowHeadLength);
    }

    public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowLength, Color color, float lengthMultiplier = 1f, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction * Math.Abs(arrowLength) * lengthMultiplier);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction * arrowLength * lengthMultiplier, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction * arrowLength * lengthMultiplier, left * arrowHeadLength);
    }

    public static void ForDebug(Vector3 pos, float arrowLength, float lengthMultiplier = 1f, float arrowHeadLength = 0.05f, float arrowHeadAngle = 20.0f)
    {
        Vector3 direction;

        if (arrowLength >= 0)
            direction = Vector3.up;
        else
            direction = Vector3.down;

        Debug.DrawRay(pos, direction * Math.Abs(arrowLength) * lengthMultiplier);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + (direction * arrowLength * lengthMultiplier), right * arrowHeadLength);
        Debug.DrawRay(pos + (direction * arrowLength * lengthMultiplier), left * arrowHeadLength);
    }
    public static void ForDebug(Vector3 pos, float arrowLength, Color color, float lengthMultiplier = 1f, float arrowHeadLength = 0.05f, float arrowHeadAngle = 20.0f)
    {
        Vector3 direction;

        if (arrowLength >= 0)
            direction = Vector3.up;
        else
            direction = Vector3.down;

        Debug.DrawRay(pos, direction * Math.Abs(arrowLength) * lengthMultiplier, color);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + (direction * Math.Abs(arrowLength) * lengthMultiplier), right * arrowHeadLength, color);
        Debug.DrawRay(pos + (direction * Math.Abs(arrowLength) * lengthMultiplier), left * arrowHeadLength, color);
    }

    public static void ForDebug3D(Vector3 pos, Vector3 direction, float lengthMultiplier = 1f, float arrowHeadLength = 0.05f, float arrowHeadAngle = 20.0f)
    {
        if(direction != Vector3.zero)
        {
            Debug.DrawRay(pos, direction * lengthMultiplier);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + (direction * lengthMultiplier), right * arrowHeadLength);
            Debug.DrawRay(pos + (direction * lengthMultiplier), left * arrowHeadLength);
        }
    }
    public static void ForDebug3D(Vector3 pos, Vector3 direction, Color color, float lengthMultiplier = 1f, float arrowHeadLength = 0.05f, float arrowHeadAngle = 20.0f)
    {
        //float reduce = 0.5f;
        float reduce = 1f;

        //pos = pos + new Vector3(0.5f, 0, 0);

        if (direction != Vector3.zero)
        {
            Debug.DrawRay(pos, reduce * direction * lengthMultiplier, color);

            Vector3 right = Quaternion.LookRotation( reduce *direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Quaternion.Euler(30, 0, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Quaternion.Euler(-30, 0, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + (reduce * direction * lengthMultiplier), right * arrowHeadLength, color);
            Debug.DrawRay(pos + (reduce * direction * lengthMultiplier), left * arrowHeadLength, color);
        }
    }
    public static void ForDebug3DVelocity(Vector3 pos, Vector3 direction, Color color, float lengthMultiplier = 1f, float arrowHeadLength = 0.05f, float arrowHeadAngle = 20.0f)
    {
        float reduce = 0.5f;

        //pos = pos + new Vector3(0.5f, 0, 0);

        if (direction != Vector3.zero)
        {
            Debug.DrawRay(pos, reduce * direction * lengthMultiplier, color);

            Vector3 right = Quaternion.LookRotation(reduce * direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Quaternion.Euler(30, 0, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Quaternion.Euler(-30, 0, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + (reduce * direction * lengthMultiplier), right * arrowHeadLength, color);
            Debug.DrawRay(pos + (reduce * direction * lengthMultiplier), left * arrowHeadLength, color);
        }
    }
}