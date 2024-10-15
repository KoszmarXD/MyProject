using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public TerrainType[] terrainTypes; // �bInspector���]�m���P���a������
    public List<ChessPiece> chessPieces; // �Ҧ��Ѥl

    private GridCell[,] gridCells;
    private int width = 0;
    private int height = 0;

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

    public List<GridCell> GetAvailableCells(Vector2Int start, int range, bool isPlayerControlled)
    {
        List<GridCell> available = new List<GridCell>();
        // �ϥμs���u���j���Ψ�L��k�ӭp��i���ʽd��
        for (int x = start.x - range; x <= start.x + range; x++)
        {
            for (int z = start.y - range; z <= start.y + range; z++)
            {
                if (x >= 0 && x < width && z >= 0 && z < height)
                {
                    GridCell cell = gridCells[x, z];
                    if (cell != null && cell.isWalkable)
                    {
                        available.Add(cell);
                    }
                }
            }
        }
        return available;
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
}
