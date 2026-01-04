using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public Vector2Int gridSize = new Vector2Int(7, 7);
    [SerializeField] TileSpawner tileSpawner;
    [SerializeField] GameObject playerPrefab;

    // Player starting positions (like chess: a4 and g4 on 7x7)
    public Vector2Int player1Start = new Vector2Int(0, 3);
    public Vector2Int player2Start = new Vector2Int(6, 3);

    Dictionary<Vector2Int, GameObject> playerPositions = new Dictionary<Vector2Int, GameObject>();
    Dictionary<Vector2Int, TileScript> tiles = new Dictionary<Vector2Int, TileScript>();

    // Turn submission storage
    List<Vector2Int>[] submittedPaths = new List<Vector2Int>[2];
    bool[] hasSubmitted = new bool[2];

    // Resolution state
    bool isResolving = false;

    void Awake()
    {
        Instance = this;
    }

    public void StartGame()
    {
        if (!IsServer) return;

        tileSpawner.Spawn(gridSize);
        SpawnPlayers();
    }

    void SpawnPlayers()
    {
        SpawnPlayerAt(player1Start, 0);
        SpawnPlayerAt(player2Start, 1);
    }

    void SpawnPlayerAt(Vector2Int pos, int playerIndex)
    {
        Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
        GameObject player = Instantiate(playerPrefab, worldPos, Quaternion.identity);
        player.GetComponent<NetworkObject>().Spawn();
        player.GetComponent<PlayerController>().SetPlayerIndex(playerIndex);
        playerPositions[pos] = player;
    }

    public void RegisterTile(Vector2Int pos, TileScript tile)
    {
        tiles[pos] = tile;
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y;
    }

    public bool IsTileEmpty(Vector2Int pos)
    {
        return !playerPositions.ContainsKey(pos);
    }

    public TileScript GetTileAt(Vector2Int pos)
    {
        return tiles[pos];
    }

    public Vector2Int GetPlayerPosition(int playerIndex)
    {
        foreach (var kvp in playerPositions)
        {
            PlayerController pc = kvp.Value.GetComponent<PlayerController>();
            if (pc.playerIndex == playerIndex)
            {
                return kvp.Key;
            }
        }
        return playerIndex == 0 ? player1Start : player2Start;
    }

    public GameObject GetPlayerObject(int playerIndex)
    {
        foreach (var kvp in playerPositions)
        {
            PlayerController pc = kvp.Value.GetComponent<PlayerController>();
            if (pc.playerIndex == playerIndex)
            {
                return kvp.Value;
            }
        }
        return null; // Will error if used when player doesn't exist
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitPathServerRpc(int playerIndex, Vector2Int[] path)
    {
        if (isResolving) return;

        submittedPaths[playerIndex] = new List<Vector2Int>(path);
        hasSubmitted[playerIndex] = true;

        if (hasSubmitted[0] && hasSubmitted[1])
        {
            StartCoroutine(ResolveTurns());
        }
    }

    IEnumerator ResolveTurns()
    {
        isResolving = true;
        NotifyResolutionStartClientRpc();

        int maxSteps = Mathf.Max(
            submittedPaths[0].Count,
            submittedPaths[1].Count
        );

        bool[] stopped = new bool[2];
        Vector2Int[] currentPos = new Vector2Int[2];
        currentPos[0] = GetPlayerPosition(0);
        currentPos[1] = GetPlayerPosition(1);

        for (int step = 0; step < maxSteps; step++)
        {
            Vector2Int[] targetPos = new Vector2Int[2];
            bool[] wantsToMove = new bool[2];

            for (int p = 0; p < 2; p++)
            {
                if (stopped[p] || step >= submittedPaths[p].Count)
                {
                    targetPos[p] = currentPos[p];
                    wantsToMove[p] = false;
                }
                else
                {
                    targetPos[p] = submittedPaths[p][step];
                    wantsToMove[p] = true;
                }
            }

            // Check for collision: both trying to move to same tile
            if (wantsToMove[0] && wantsToMove[1] && targetPos[0] == targetPos[1])
            {
                stopped[0] = true;
                stopped[1] = true;
            }
            else
            {
                for (int p = 0; p < 2; p++)
                {
                    if (!wantsToMove[p] || stopped[p]) continue;

                    int otherPlayer = 1 - p;

                    // Check if target is occupied by other player who isn't moving away
                    if (targetPos[p] == currentPos[otherPlayer])
                    {
                        if (!wantsToMove[otherPlayer] || targetPos[otherPlayer] == currentPos[otherPlayer])
                        {
                            stopped[p] = true;
                        }
                    }
                }
            }

            // Move players who aren't stopped
            for (int p = 0; p < 2; p++)
            {
                if (wantsToMove[p] && !stopped[p])
                {
                    MovePlayer(p, currentPos[p], targetPos[p]);
                    currentPos[p] = targetPos[p];
                }
            }

            UpdatePlayerPositionsClientRpc(currentPos[0], currentPos[1]);
            yield return new WaitForSeconds(0.5f);
        }

        // Reset for next turn
        hasSubmitted[0] = false;
        hasSubmitted[1] = false;
        submittedPaths[0] = null;
        submittedPaths[1] = null;
        isResolving = false;

        NotifyResolutionEndClientRpc();
    }

    void MovePlayer(int playerIndex, Vector2Int from, Vector2Int to)
    {
        GameObject player = GetPlayerObject(playerIndex);
        playerPositions.Remove(from);
        playerPositions[to] = player;
        player.transform.position = new Vector3(to.x, to.y, 0);
    }

    [ClientRpc]
    void UpdatePlayerPositionsClientRpc(Vector2Int pos0, Vector2Int pos1)
    {
        GameObject p0 = GetPlayerObject(0);
        GameObject p1 = GetPlayerObject(1);
        p0.transform.position = new Vector3(pos0.x, pos0.y, 0);
        p1.transform.position = new Vector3(pos1.x, pos1.y, 0);
    }

    [ClientRpc]
    void NotifyResolutionStartClientRpc()
    {
        PathBuilder localBuilder = FindLocalPathBuilder();
        localBuilder.SetInputEnabled(false);
    }

    [ClientRpc]
    void NotifyResolutionEndClientRpc()
    {
        PathBuilder localBuilder = FindLocalPathBuilder();
        localBuilder.SetInputEnabled(true);
        localBuilder.ClearPath();
    }

    PathBuilder FindLocalPathBuilder()
    {
        foreach (var pb in FindObjectsOfType<PathBuilder>())
        {
            if (pb.IsOwner) return pb;
        }
        return null; // Will error if no local PathBuilder
    }
}
