using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public Vector2 Position;
    [SerializeField] Material[] mats;
    [SerializeField] MeshRenderer mR;


    public void Initiate(Vector2 pos, int r)
    {
        Position = pos;
        mR.material = mats[r];
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
