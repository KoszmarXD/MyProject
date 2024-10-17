using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    private GridManager gridManager;

    void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("GridManager ���b���������I");
        }
    }

    // ��󦨥����j�M�A���Ҧ��i�F����l
    public List<GridCell> FindAccessibleCells(Vector2Int startPos, float maxCost)
    {
        List<GridCell> accessible = new List<GridCell>();
        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        HashSet<GridCell> closedList = new HashSet<GridCell>();
        HashSet<GridCell> openSet = new HashSet<GridCell>(); // �Ω���ܤw�K�[��}��C����l

        GridCell startCell = gridManager.GetGridCell(startPos);
        if (startCell == null)
        {
            Debug.LogError("�_�I��l�� null�I");
            return accessible;
        }

        Node startNode = new Node(startCell, null, 0f, 0f); // hCost �]�m�� 0
        openList.Enqueue(startNode, startNode.fCost);
        openSet.Add(startCell);
        Debug.Log($"�_�l�`�I: {startCell.gameObject.name} at ({startCell.gridPosition.x}, {startCell.gridPosition.y})");

        int maxIterations = 10000; // �]�m�@�ӳ̤j���N���ƥH����L���j��
        int iterations = 0;

        while (!openList.IsEmpty() && iterations < maxIterations)
        {
            Node currentNode = openList.Dequeue();
            openSet.Remove(currentNode.gridCell); // �q�}�񶰦X������
            iterations++;

            Debug.Log($"���N {iterations}: �B�z�`�I: {currentNode.gridCell.gameObject.name} at ({currentNode.gridCell.gridPosition.x}, {currentNode.gridCell.gridPosition.y})");

            if (closedList.Contains(currentNode.gridCell))
            {
                Debug.Log($"�`�I {currentNode.gridCell.gameObject.name} �w�b�ʳ��C���A���L�C");
                continue;
            }

            closedList.Add(currentNode.gridCell);

            // �p�G��e�`�����W�L�̤j�����A�����X�i
            if (currentNode.gCost > maxCost)
            {
                Debug.Log($"�`�I {currentNode.gridCell.gameObject.name} �� gCost ({currentNode.gCost}) �W�L�̤j���� ({maxCost})�A�����X�i�C");
                continue;
            }

            accessible.Add(currentNode.gridCell);
            Debug.Log($"�K�[��i�F�C��: {currentNode.gridCell.gameObject.name}�AgCost: {currentNode.gCost}");

            foreach (GridCell neighbor in gridManager.GetNeighbors(currentNode.gridCell))
            {
                if (!neighbor.isWalkable)
                {
                    Debug.Log($"�F�~ {neighbor.gameObject.name} ���i�樫�A���L�C");
                    continue;
                }

                if (closedList.Contains(neighbor))
                {
                    Debug.Log($"�F�~ {neighbor.gameObject.name} �w�b�ʳ��C���A���L�C");
                    continue;
                }

                float tentativeGCost = currentNode.gCost + neighbor.movementCost;
                Debug.Log($"�p��F�~ {neighbor.gameObject.name} �� tentativeGCost: {tentativeGCost}");

                if (tentativeGCost > maxCost)
                {
                    Debug.Log($"�F�~ {neighbor.gameObject.name} �� tentativeGCost ({tentativeGCost}) �W�L�̤j���� ({maxCost})�A���L�C");
                    continue;
                }

                // �b FindAccessibleCells ���AhCost �]�m�� 0
                Node neighborNode = new Node(neighbor, currentNode, tentativeGCost, 0f);

                if (!openSet.Contains(neighbor))
                {
                    openList.Enqueue(neighborNode, neighborNode.fCost);
                    openSet.Add(neighbor);
                    Debug.Log($"�K�[��}��C��: {neighbor.gameObject.name}�AfCost: {neighborNode.fCost}");
                }
                else
                {
                    Debug.Log($"�F�~ {neighbor.gameObject.name} �w�b�}��C���A���L�C");
                }
            }
        }

        if (iterations >= maxIterations)
        {
            Debug.LogError("A* ���|�M��F��̤j���N���ơA�i��s�b�L���j��C");
        }

        Debug.Log($"��� {accessible.Count} �ӥi���ʮ�l�q ({startPos.x}, {startPos.y}) �d�򤺡A�`���N����: {iterations}");
        return accessible;
    }

    public (List<GridCell> path, float totalCost) FindPath(Vector2Int startPos, Vector2Int targetPos)
    {
        GridCell startCell = gridManager.GetGridCell(startPos);
        GridCell targetCell = gridManager.GetGridCell(targetPos);

        if (startCell == null || targetCell == null)
        {
            Debug.LogError("�_�I�β��I��l�� null�I");
            return (null, 0f);
        }

        if (!targetCell.isWalkable)
        {
            Debug.LogError("���I��l���i�樫�I");
            return (null, 0f);
        }

        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        HashSet<GridCell> closedList = new HashSet<GridCell>();
        HashSet<GridCell> openSet = new HashSet<GridCell>(); // ���ܤw�K�[��}��C����l

        Node startNode = new Node(startCell, null, 0f, GetHeuristic(startCell, targetCell));
        openList.Enqueue(startNode, startNode.fCost);
        openSet.Add(startCell);
        Debug.Log($"�_�l�`�I: {startCell.gameObject.name} at ({startCell.gridPosition.x}, {startCell.gridPosition.y})");

        int maxIterations = 10000; // �]�m�@�ӳ̤j���N���ƥH����L���j��
        int iterations = 0;

        while (!openList.IsEmpty() && iterations < maxIterations)
        {
            Node currentNode = openList.Dequeue();
            openSet.Remove(currentNode.gridCell); // �q�}�񶰦X������
            iterations++;

            Debug.Log($"���N {iterations}: �B�z�`�I: {currentNode.gridCell.gameObject.name} at ({currentNode.gridCell.gridPosition.x}, {currentNode.gridCell.gridPosition.y})");

            if (closedList.Contains(currentNode.gridCell))
            {
                Debug.Log($"�`�I {currentNode.gridCell.gameObject.name} �w�b�ʳ��C���A���L�C");
                continue;
            }

            closedList.Add(currentNode.gridCell);

            // �p�G��F���I�A���ظ��|
            if (currentNode.gridCell == targetCell)
            {
                Debug.Log("���\�����|�I");
                List<GridCell> path = ReconstructPath(currentNode);
                float totalCost = currentNode.gCost;
                return (path, totalCost);
            }

            foreach (GridCell neighbor in gridManager.GetNeighbors(currentNode.gridCell))
            {
                if (!neighbor.isWalkable)
                {
                    Debug.Log($"�F�~ {neighbor.gameObject.name} ���i�樫�A���L�C");
                    continue;
                }

                if (closedList.Contains(neighbor))
                {
                    Debug.Log($"�F�~ {neighbor.gameObject.name} �w�b�ʳ��C���A���L�C");
                    continue;
                }

                float tentativeGCost = currentNode.gCost + neighbor.movementCost;
                Debug.Log($"�p��F�~ {neighbor.gameObject.name} �� tentativeGCost: {tentativeGCost}");


                Node neighborNode = new Node(neighbor, currentNode, tentativeGCost, GetHeuristic(neighbor, targetCell));

                if (!openSet.Contains(neighbor))
                {
                    openList.Enqueue(neighborNode, neighborNode.fCost);
                    openSet.Add(neighbor);
                    Debug.Log($"�K�[��}��C��: {neighbor.gameObject.name}�AfCost: {neighborNode.fCost}");
                }
                else
                {
                    Debug.Log($"�F�~ {neighbor.gameObject.name} �w�b�}��C���A���L�C");
                }
            }
        }

        if (iterations >= maxIterations)
        {
            Debug.LogError("A* ���|�M��F��̤j���N���ơA�i��s�b�L���j��C");
        }

        // �L�k�����|
        Debug.LogWarning("�L�k���q�_�I����I�����|�I");
        return (null, 0f);
    }

    private List<GridCell> ReconstructPath(Node endNode)
    {
        List<GridCell> path = new List<GridCell>();
        Node current = endNode;

        while (current != null)
        {
            path.Add(current.gridCell);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private float GetHeuristic(GridCell a, GridCell b)
    {
        // �ҫ��y�Z��
        return Mathf.Abs(a.gridPosition.x - b.gridPosition.x) + Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
    }

    private class Node
    {
        public GridCell gridCell;
        public Node parent;
        public float gCost; // �q�_�I���e�`�I������
        public float hCost; // �q��e�`�I����I���w������
        public float fCost { get { return gCost + hCost; } }

        public Node(GridCell cell, Node parentNode, float g, float h)
        {
            gridCell = cell;
            parent = parentNode;
            gCost = g;
            hCost = h;
        }

        // ���g Equals �M GetHashCode�A�H�K�b PriorityQueue �����T����`�I
        public override bool Equals(object obj)
        {
            if (obj is Node other)
            {
                return gridCell == other.gridCell;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return gridCell.GetHashCode();
        }
    }
}
