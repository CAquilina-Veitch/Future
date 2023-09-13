using Unity.Netcode;
using UnityEngine;



public enum status {Null, Ready, Locked, Playing, Waiting, Menus }


public class ThoughtController : NetworkBehaviour
{
    status[] currentPlayerStatuses = { status.Null, status.Null };

    public void UpdateStatus(int id, status newStatus)
    {
        currentPlayerStatuses[id] = newStatus;
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
