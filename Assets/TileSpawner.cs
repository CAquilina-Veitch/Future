using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    [SerializeField] Storage storage;
    
    

    public void Spawn(Vector2 prop)
    {
        for(int x = 0; x<prop.x; x++)
        {
            for(int z = 0; z<prop.y; z++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, 0, z), Quaternion.identity, transform);
                tile.GetComponent<TileScript>().Initiate(new Vector2(x, z), (x + z) % 2);
                tile.name = $"Tile : {x}, {z}";
                storage.setTileAtPos(new Vector2(x, z),gameObject);
            }
        }
    }
}
