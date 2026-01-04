using UnityEngine;

public class TileScript : MonoBehaviour
{
    public Vector2Int Position;

    // For path highlighting - you set this visually
    public bool isInPath = false;

    // 2D collider for click detection
    BoxCollider2D col;

    public void Initiate(Vector2Int pos, int checkerboardIndex)
    {
        Position = pos;
        transform.position = new Vector3(pos.x, pos.y, 0);

        // Add 2D collider for raycast detection
        col = gameObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);
    }

    public void SetInPath(bool inPath)
    {
        isInPath = inPath;
        // You handle visuals for highlighting
    }
}
