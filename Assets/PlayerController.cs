using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public int playerIndex = -1;
    public GameObject ghostPlayer;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            gameObject.AddComponent<PathBuilder>();
        }

        // Register with GameManager on all clients
        if (playerIndex >= 0)
        {
            GameManager.Instance.RegisterPlayer(playerIndex, this);
        }
    }

    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
        GameManager.Instance.RegisterPlayer(index, this);
        SetPlayerIndexClientRpc(index);
    }

    [ClientRpc]
    void SetPlayerIndexClientRpc(int index)
    {
        playerIndex = index;
        GameManager.Instance.RegisterPlayer(index, this);
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
