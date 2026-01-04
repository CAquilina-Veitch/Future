using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PathBuilder : NetworkBehaviour
{
    List<Vector2Int> currentPath = new List<Vector2Int>();
    const int maxStamina = 8;
    bool inputEnabled = true;

    PlayerController playerController;
    int myPlayerIndex;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!IsOwner || !inputEnabled) return;
        if (playerController.playerIndex < 0) return;

        myPlayerIndex = playerController.playerIndex;

        // Right click - clear path
        if (Input.GetMouseButtonDown(1))
        {
            ClearPath();
            return;
        }

        // Left click - add to path or trim
        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }

        // Space - submit turn
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SubmitTurn();
        }
    }

    void HandleLeftClick()
    {
        Vector2Int clickedPos = GetTileUnderMouse();

        // Check if this position is already in the path
        int existingIndex = currentPath.IndexOf(clickedPos);
        if (existingIndex >= 0)
        {
            TrimPathTo(existingIndex);
            return;
        }

        // Check if we have stamina left
        if (currentPath.Count >= maxStamina) return;

        // Check if position is adjacent to the end of path
        Vector2Int lastPos = GetPathEnd();
        if (!IsAdjacent(lastPos, clickedPos)) return;

        AddToPath(clickedPos);
    }

    Vector2Int GetPathEnd()
    {
        if (currentPath.Count > 0)
        {
            return currentPath[currentPath.Count - 1];
        }
        return GameManager.Instance.GetPlayerPosition(myPlayerIndex);
    }

    bool IsAdjacent(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    Vector2Int GetTileUnderMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        TileScript tile = hit.collider.GetComponent<TileScript>();
        return tile.Position;
    }

    void AddToPath(Vector2Int pos)
    {
        currentPath.Add(pos);
        GameManager.Instance.GetTileAt(pos).SetInPath(true);
        playerController.SetGhostPosition(pos);
    }

    void TrimPathTo(int index)
    {
        for (int i = index + 1; i < currentPath.Count; i++)
        {
            GameManager.Instance.GetTileAt(currentPath[i]).SetInPath(false);
        }

        currentPath.RemoveRange(index + 1, currentPath.Count - index - 1);

        if (currentPath.Count > 0)
        {
            playerController.SetGhostPosition(currentPath[currentPath.Count - 1]);
        }
        else
        {
            playerController.HideGhost();
        }
    }

    public void ClearPath()
    {
        foreach (Vector2Int pos in currentPath)
        {
            GameManager.Instance.GetTileAt(pos).SetInPath(false);
        }
        currentPath.Clear();
        playerController.HideGhost();
    }

    void SubmitTurn()
    {
        GameManager.Instance.SubmitPathServerRpc(myPlayerIndex, currentPath.ToArray());
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }
}
