using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public int playerIndex = -1;

    // Reference to ghost player object (you set this up visually)
    public GameObject ghostPlayer;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gameObject.AddComponent<PathBuilder>();
        }
    }

    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
        SetPlayerIndexClientRpc(index);
    }

    [ClientRpc]
    void SetPlayerIndexClientRpc(int index)
    {
        playerIndex = index;
    }

    public void SetGhostPosition(Vector2Int pos)
    {
        ghostPlayer.transform.position = new Vector3(pos.x, pos.y, 0);
        ghostPlayer.SetActive(true);
    }

    public void HideGhost()
    {
        ghostPlayer.SetActive(false);
    }
}
