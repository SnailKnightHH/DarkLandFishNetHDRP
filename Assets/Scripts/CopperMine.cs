using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopperMine : Structure
{
    private MeshRenderer meshRenderer;
    protected override void FinishBuildingAction()
    {
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        meshRenderer.enabled = false;
        yield return ACTION_DELAY;
        base.Despawn();
    }

    protected override void Start()
    {
        base.Start();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    void Update()
    {
        
    }
}
