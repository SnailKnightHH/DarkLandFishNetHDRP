using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities 
{
    public static string IGNORED_BY_TRIGGER_COLLIDER = "IgnoredByTriggerCollider";
    public static int SecondsToMilliseconds(float seconds)
    {
        return (int)(seconds * 1000);
    }

    public static bool GroundedCheck(Transform transform, float GroundedOffset, float GroundedRadius, LayerMask GroundLayers)
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        return Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }
}
