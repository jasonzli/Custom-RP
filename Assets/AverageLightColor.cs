using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AverageLightColor : MonoBehaviour
{

    void Start()
    {
        Camera.main.backgroundColor = AverageColor();
    }

    //a function that averages the color all lights in the scene
    public Color AverageColor()
    {
        //get all lights in the scene
        Light[] lights = FindObjectsOfType<Light>();
        //create a new color
        Color color = new Color();
        //loop through all lights
        foreach (Light light in lights)
        {
            //add the color of the light to the color
            color += light.color;
        }
        //divide the color by the number of lights
        color /= lights.Length;
        //return the color
        return color;
    }
}
