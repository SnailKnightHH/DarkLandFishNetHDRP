using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOManager : MonoBehaviour
{
    [SerializeField] private List<Item> _allItems;
    
    public List<Item> AllItems
    {
        get
        {
            return _allItems;
        }
    }

    private Dictionary<string, Item> _allItemsNameToItemMapping = new Dictionary<string, Item>();
    public Dictionary<string, Item> AllItemsNameToItemMapping
    {
        get
        {
            return _allItemsNameToItemMapping;
        }
    }

    public static SOManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

            foreach (Item item in _allItems) {
                _allItemsNameToItemMapping.Add(item.ItemName, item);
            }
        }
    }

}
