using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public int playerId = -1;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) Destroy(this);
        playerId = NetworkBehaviourId;
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
