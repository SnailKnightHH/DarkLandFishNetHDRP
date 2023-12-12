using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : Tool
{
    private Light light;
    void Start()
    {
        light = GetComponentInChildren<Light>();
    }


    public override IEnumerator UseTool(Player player)
    {
        light.enabled = !light.enabled;
        yield return null;
    }
}
