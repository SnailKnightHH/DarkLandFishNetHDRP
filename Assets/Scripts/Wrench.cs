using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Wrench : NetworkBehaviour
{    
    public Structure structure
    {
        get; set;
    }

    [SerializeField] private Material finishedMaterial;

    public IEnumerator Build(Player player)
    {
        yield return StartCoroutine(structure.Build(player));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
