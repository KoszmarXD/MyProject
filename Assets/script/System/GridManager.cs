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
    private int width = 0;
    private int height = 0;
    private object maxCost;

    void Start()
    {
        InitializeGrid();
        InitializeChessPieces();
    }

    /// <summary>
    /// ��l�ƴѽL��AŪ���������Ҧ��a�� GridCell �ե󪺪���
    /// </summary>
    private void InitializeGrid()
    {
        // ���Ҧ��������� GridCell ����
        GridCell[] cells = FindObjectsOfType<GridCell>();

        if (cells.Length == 0)
        {
            Debug.LogError("�������S�������� GridCell�I");
            return;
        }

        // ���]�ѽL�O�x�Ϊ��A�p��e�שM����
        int maxX = 0;
        int maxZ = 0;

        foreach (var cell in cells)
        {
            if (cell.gridPosition.x > maxX) maxX = cell.gridPosition.x;
            if (cell.gridPosition.y > maxZ) maxZ = cell.gridPosition.y;
        }

        width = maxX + 1;
        height = maxZ + 1;

        gridCells = new GridCell[width, height];

        foreach (var cell in cells)
        {
            int x = cell.gridPosition.x;
            int z = cell.gridPosition.y;

            if (x >= 0 && x < width && z >= 0 && z < height)
            {
                gridCells[x, z] = cell;
                // ��l�ƩΧ�s��l���ݩʡ]�ھ� TerrainType�^
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
    }
    /// <summary>
    /// �ھ� GridCell ���a�����������ݩʩM����
    /// </summary>
    /// <param name="cell">�n�����ݩʪ� GridCell</param>
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

    // �s�W��k�G������w��m�� GridCell
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
            Debug.LogWarning($"GridCell ��m ({position.x}, {position.y}) �W�X�d��I");
        }
        return null;
    }

    // �s�W��k�G������w GridCell ���F�~
    public List<GridCell> GetNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();

        Vector2Int pos = cell.gridPosition;

        // 8 ��V���F�~�]�i�ھڻݭn�վ㬰 4 ��V�^
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1), // �W
            new Vector2Int(1, 0), // �k
            new Vector2Int(0, -1), // �U
            new Vector2Int(-1, 0), // ��
            /*new Vector2Int(1, 1), // �k�W
            new Vector2Int(1, -1), // �k�U
            new Vector2Int(-1, -1), // ���U
            new Vector2Int(-1, 1) // ���W*/
        };

        foreach (var dir in directions)
        {
            Vector2Int neighborPos = new Vector2Int(pos.x + dir.x, pos.y + dir.y);
            GridCell neighbor = GetGridCell(neighborPos);
            if (neighbor != null && neighbor.isWalkable)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
    // ����Ҧ��b���ʽd�򤺥B�i�F�� GridCell
    public List<GridCell> GetAccessibleCells(Vector2Int start, int range)
    {
        List<GridCell> accessible = new List<GridCell>();
        AStarPathfinding aStar = FindObjectOfType<AStarPathfinding>();
        if (aStar == null)
        {
            Debug.LogError("AStarPathfinding ���b���������I");
            return accessible;
        }

        GridCell startCell = GetGridCell(start);
        if (startCell == null)
        {
            Debug.LogError("�_�I��l�� null�I");
            return accessible;
        }

        // �M���Ҧ��i�樫����l�A���ˬd�q�_�I��ؼЮ�l���`�����O�_�b�d��
        foreach (var cell in gridCells)
        {
            if (cell != null && cell.isWalkable)
            {
                var (path, totalCost) = aStar.FindPath(start, cell.gridPosition);
                if (path != null && totalCost <= range)
                {
                    accessible.Add(cell);
                }
            }
        }

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
