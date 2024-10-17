using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class GridManager : MonoBehaviour
{
    public TerrainType[] terrainTypes; // �bInspector���]�m���P���a������
    public List<ChessPiece> chessPieces; // �Ҧ��Ѥl

    private GridCell[,] gridCells;
    public int width = 0;
    public int height = 0;
    public int Width
    {
        get { return width; }
    }

    public int Height
    {
        get { return height; }
    }


    void Start()
    {
        InitializeGrid();
        InitializeChessPieces();
    }

    // ��l�ƴѽL��AŪ���������Ҧ��a�� GridCell �ե󪺪���
    private void InitializeGrid()
    {
        // ���Ҧ��������� GridCell ����
        GridCell[] cells = FindObjectsOfType<GridCell>();

        if (cells.Length == 0)
        {
            Debug.LogError("�������S�������� GridCell�I");
            return;
        }

        // ���̤p�M�̤j�� x �M z �y��
        int minX = int.MaxValue;
        int minZ = int.MaxValue;
        int maxX = int.MinValue;
        int maxZ = int.MinValue;

        foreach (var cell in cells)
        {
            if (cell.transform.position.x < minX) minX = Mathf.RoundToInt(cell.transform.position.x);
            if (cell.transform.position.z < minZ) minZ = Mathf.RoundToInt(cell.transform.position.z);
            if (cell.transform.position.x > maxX) maxX = Mathf.RoundToInt(cell.transform.position.x);
            if (cell.transform.position.z > maxZ) maxZ = Mathf.RoundToInt(cell.transform.position.z);
        }

        // �p�ⰾ���q�H�T�O�Ҧ��y�Ь��D�t��
        int offsetX = minX < 0 ? -minX : 0;
        int offsetZ = minZ < 0 ? -minZ : 0;

        width = maxX + offsetX + 1;
        height = maxZ + offsetZ + 1;

        gridCells = new GridCell[width, height];

        foreach (var cell in cells)
        {
            int x = Mathf.RoundToInt(cell.transform.position.x) + offsetX;
            int z = Mathf.RoundToInt(cell.transform.position.z) + offsetZ;

            if (x >= 0 && x < width && z >= 0 && z < height)
            {
                gridCells[x, z] = cell;
                cell.gridPosition = new Vector2Int(x, z); // �۰ʤ��t gridPosition
                ApplyTerrainType(cell);
            }
            else
            {
                Debug.LogWarning($"GridCell {cell.name} �� gridPosition ({x}, {z}) �W�X�d��I");
            }
        }

        Debug.Log($"Grid ��l�Ƨ����A�e��: {width}, ����: {height}");
    }
    private void InitializeChessPieces()
    {
        chessPieces = new List<ChessPiece>(FindObjectsOfType<ChessPiece>());
        Debug.Log($"��� {chessPieces.Count} �ӴѤl");
    }

    // �ھ� GridCell ���a�����������ݩʩM����
    private void ApplyTerrainType(GridCell cell)
    {
        // �ϥΪT�|�Ӥǰt����
        switch (cell.terrainType)
        {
            case TerrainTypeEnum.Grass:
                cell.SetMaterialBasedOnTerrain();
                break;
            case TerrainTypeEnum.Water:
                cell.SetMaterialBasedOnTerrain();
                break;
            case TerrainTypeEnum.Mountain:
                cell.SetMaterialBasedOnTerrain();
                break;
            default:
                Debug.LogWarning($"���B�z�� terrainType: {cell.terrainType}");
                break;
        }
    }

    // ������w��m�� GridCell
    public GridCell GetGridCell(Vector2Int position)
    {
        if (position.x >= 0 && position.x < width && position.y >= 0 && position.y < height)
        {
            GridCell cell = gridCells[position.x, position.y];
            if (cell != null)
            {
                Debug.Log($"���o GridCell: {cell.gameObject.name} at ({position.x}, {position.y})");
                return cell;
            }
            else
            {
                Debug.LogWarning($"GridCell �b ({position.x}, {position.y}) �� null�I");
            }
        }
        else
        {
            Debug.LogWarning($"GridCell ��m ({position.x}, {position.y}) �W�X�d��I�e��: {width}, ����: {height}");
        }
        return null;
    }

    //������w GridCell ���F�~
    public List<GridCell> GetNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();

        Vector2Int pos = cell.gridPosition;

        // 4 ��V���F�~
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), // �W
            new Vector2Int(1, 0), // �k
            new Vector2Int(0, -1), // �U
            new Vector2Int(-1, 0)  // ��
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborPos = new Vector2Int(pos.x + dir.x, pos.y + dir.y);
            // �ˬd�F�~��m�O�_�b���Ľd��
            if (neighborPos.x >= 0 && neighborPos.x < width && neighborPos.y >= 0 && neighborPos.y < height)
            {
                GridCell neighbor = GetGridCell(neighborPos);
                if (neighbor != null && neighbor.isWalkable)
                {
                    neighbors.Add(neighbor);
                    Debug.Log($"���o�F�~ GridCell: {neighbor.gameObject.name} at ({neighbor.gridPosition.x}, {neighbor.gridPosition.y})");
                }
            }
            else
            {
                Debug.LogWarning($"�F�~ GridCell ��m ({neighborPos.x}, {neighborPos.y}) �W�X�d��I");
            }
        }

        return neighbors;
    }

    // ����Ҧ��b���ʽd�򤺥B�i�F�� GridCell
    public List<GridCell> GetAccessibleCells(Vector2Int start, float maxCost)
    {
        List<GridCell> accessible = new List<GridCell>();
        AStarPathfinding aStar = FindObjectOfType<AStarPathfinding>();
        if (aStar == null)
        {
            Debug.LogError("AStarPathfinding ���b���������I");
            return accessible;
        }

        accessible = aStar.FindAccessibleCells(start, maxCost);

        Debug.Log($"��� {accessible.Count} �ӥi���ʮ�l�q ({start.x}, {start.y}) �d��");
        return accessible;
    }

    // ����Ҧ��Ĥ�Ѥl
    public List<ChessPiece> GetEnemies(bool isPlayerControlled)
    {
        List<ChessPiece> enemies = new List<ChessPiece>();
        foreach (var piece in chessPieces)
        {
            if (piece.isPlayerControlled != isPlayerControlled)
            {
                enemies.Add(piece);
            }
        }
        return enemies;
    }

    // ��s�Ѥl�C��]�Ҧp�Ѥl���`�ɡ^
    public void RemoveChessPiece(ChessPiece piece)
    {
        if (chessPieces.Contains(piece))
        {
            chessPieces.Remove(piece);
        }
    }
    
    void OnDrawGizmos()
    {
        if (gridCells == null)
            return;

        Gizmos.color = Color.blue;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridCell cell = gridCells[x, z];
                if (cell != null)
                {
                    Gizmos.DrawWireCube(cell.transform.position, new Vector3(1, 0.1f, 1));
                }
            }
        }
    }

    internal List<GridCell> GetAvailableCells(Vector2Int gridPosition, int movementRange, bool isPlayerControlled)
    {
        throw new NotImplementedException();
    }
}
