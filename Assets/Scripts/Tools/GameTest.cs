// 附加到Tilemap GameObject上的脚本示例
using UnityEngine;
using UnityEngine.Tilemaps;

public class IsometricTileSorter : MonoBehaviour
{
    public Tilemap tilemap;

    void Update()
    {
        if (tilemap == null) return;

        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                var tileObj = tilemap.GetInstantiatedObject(pos);
                if (tileObj != null)
                {
                    var renderer = tileObj.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        // 基于与参考点的距离设置排序顺序
                        Debug.Log(renderer.sortingOrder);
                    }
                }
            }
        }
    }
}