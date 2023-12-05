using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class ListHash<T>
{
    private Dictionary<T, int> dict;
    private List<T> list;    

    public ListHash()
    {
        dict = new Dictionary<T, int>();
        list = new List<T>();
    }

    public void Insert(T element)
    {
        if (!dict.ContainsKey(element))
        {
            dict.Add(element, list.Count);
            list.Add(element);
        }
        
    }

    public void Remove(T element)
    {
        if (dict.TryGetValue(element, out int idx))
        {
            if (idx < list.Count - 1)
            {
                list[idx] = list[list.Count - 1];
                dict[list[idx]] = idx;
            }            
            list.RemoveAt(list.Count - 1);
            dict.Remove(element);            
        }        
    }

    public T GetRandom()
    {
        int idx = Random.Range(0, list.Count);
        return list[idx];
    }

    public int Count
    {        
        get
        {
            return dict.Count;
        } 
    }
}

public class RandomizedObjectSpawner : NetworkBehaviour
{
    [SerializeField] int numberOfObjectsToSpawn = 10;
    ListHash<Vector3> locationsToSpawn;
    [SerializeField] List<GameObject> houses;

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (!IsServer && !IsHost) { return; }        
        locationsToSpawn = new ListHash<Vector3>();
        for (int i = 0; i < transform.childCount; i++)
        {
            locationsToSpawn.Insert(transform.GetChild(i).localPosition);
        }

        Debug.Assert(numberOfObjectsToSpawn <= locationsToSpawn.Count, "Cannot spawn more objects than locations.");
        for (int i = 0; i < numberOfObjectsToSpawn; i++)
        {
            Vector3 location = locationsToSpawn.GetRandom();
            GameObject housePrefab = houses[Random.Range(0, houses.Count)]; // houses can be duplicate for now
            GameObject houseInstance = Instantiate(housePrefab, location, housePrefab.transform.rotation);
            base.Spawn(houseInstance);
            locationsToSpawn.Remove(location); // to ensure no duplicate locations are selected 
        }
    }
    
}
