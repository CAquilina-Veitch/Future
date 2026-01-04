using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [SerializeField] TileScript tilePrefab;
    [SerializeField] GameManager gameManager;

    public void Spawn(Vector2Int gridSize)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                TileScript tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
                tile.Initiate(pos, (x + y) % 2);
                tile.name = $"Tile_{x}_{y}";
                gameManager.RegisterTile(pos, tile);
            }
        }
    }
}
