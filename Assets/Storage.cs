using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Storage : NetworkBehaviour
{
    public Vector2 tileProportions = new Vector2(6, 6);
    [SerializeField] TileSpawner tS;

    Dictionary<Vector2, GameObject> playerBase = new Dictionary<Vector2, GameObject>();
    Dictionary<Vector2, GameObject> tileBase = new Dictionary<Vector2, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        tS.Spawn(tileProportions);
    }



    public bool isPlayerAtPos (Vector2 pos)
    {
        return playerBase[pos] == null;
    }
    public bool isTileEmpty (Vector2 pos)
    {
        if (playerBase[pos] == null && tileObjAtPos(pos) != null)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public GameObject tileObjAtPos (Vector2 pos)
    {
        if (pos.x > tileProportions.x || pos.y > tileProportions.y || pos.x < 0 ||pos.y<0)
        {
            Debug.LogError($"Position {pos} is out of given bounds, {tileProportions}");
            return null;
        }
        else
        {
            return tileBase[pos];
        }
    }
    public void setTileAtPos(Vector2 pos,GameObject GO)
    {
        tileBase[pos] = GO;
    }






}
