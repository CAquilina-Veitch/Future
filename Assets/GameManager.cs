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

    public Vector2Int player1Start = new Vector2Int(0, 3);
    public Vector2Int player2Start = new Vector2Int(6, 3);

    // Store players directly by index
    PlayerController[] players = new PlayerController[2];
    Vector2Int[] playerPositions = new Vector2Int[2];

    Dictionary<Vector2Int, TileScript> tiles = new Dictionary<Vector2Int, TileScript>();

    List<Vector2Int>[] submittedPaths = new List<Vector2Int>[2];
    bool[] hasSubmitted = new bool[2];
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
        GameObject playerObj = Instantiate(playerPrefab, worldPos, Quaternion.identity);

        NetworkObject netObj = playerObj.GetComponent<NetworkObject>();
        PlayerController pc = playerObj.GetComponent<PlayerController>();

        netObj.Spawn();
        pc.SetPlayerIndex(playerIndex);

        players[playerIndex] = pc;
        playerPositions[playerIndex] = pos;
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
        return pos != playerPositions[0] && pos != playerPositions[1];
    }

    public TileScript GetTileAt(Vector2Int pos)
    {
        return tiles[pos];
    }

    public Vector2Int GetPlayerPosition(int playerIndex)
    {
        return playerPositions[playerIndex];
    }

    public PlayerController GetPlayer(int playerIndex)
    {
        return players[playerIndex];
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

        int maxSteps = Mathf.Max(submittedPaths[0].Count, submittedPaths[1].Count);

        bool[] stopped = new bool[2];
        Vector2Int[] currentPos = new Vector2Int[2];
        currentPos[0] = playerPositions[0];
        currentPos[1] = playerPositions[1];

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

            // Both trying to move to same tile
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

                    int other = 1 - p;

                    // Target occupied by other player who isn't moving away
                    if (targetPos[p] == currentPos[other])
                    {
                        if (!wantsToMove[other] || targetPos[other] == currentPos[other])
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
                    currentPos[p] = targetPos[p];
                    playerPositions[p] = targetPos[p];
                    players[p].transform.position = new Vector3(targetPos[p].x, targetPos[p].y, 0);
                }
            }

            UpdatePlayerPositionsClientRpc(currentPos[0], currentPos[1]);
            yield return new WaitForSeconds(0.5f);
        }

        hasSubmitted[0] = false;
        hasSubmitted[1] = false;
        submittedPaths[0] = null;
        submittedPaths[1] = null;
        isResolving = false;

        NotifyResolutionEndClientRpc();
    }

    [ClientRpc]
    void UpdatePlayerPositionsClientRpc(Vector2Int pos0, Vector2Int pos1)
    {
        players[0].transform.position = new Vector3(pos0.x, pos0.y, 0);
        players[1].transform.position = new Vector3(pos1.x, pos1.y, 0);
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
        return null;
    }

    // Called by clients to register their player reference
    public void RegisterPlayer(int playerIndex, PlayerController pc)
    {
        players[playerIndex] = pc;
        playerPositions[playerIndex] = playerIndex == 0 ? player1Start : player2Start;
    }
}
