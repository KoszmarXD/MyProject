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
            Debug.LogError("GridManager 未在場景中找到！");
        }
    }

    public (List<GridCell> path, float totalCost) FindPath(Vector2Int startPos, Vector2Int targetPos)
    {
        GridCell startCell = gridManager.GetGridCell(startPos);
        GridCell targetCell = gridManager.GetGridCell(targetPos);

        if (startCell == null || targetCell == null)
        {
            Debug.LogError("起點或終點格子為 null！");
            return (null, 0f);
        }

        if (!targetCell.isWalkable)
        {
            Debug.LogError("終點格子不可行走！");
            return (null, 0f);
        }

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Node startNode = new Node(startCell, null, 0f, GetHeuristic(startCell, targetCell));
        openList.Add(startNode);

        int maxIterations = 10;
        int iteration = 0;

        while (openList.Count > 0 && iteration < maxIterations)
        {
            iteration++;
            // 找到 f 值最低的節點
            openList.Sort((a, b) => a.fCost.CompareTo(b.fCost));
            Node currentNode = openList[0];
            openList.RemoveAt(0);
            closedList.Add(currentNode);

            // 如果到達終點，重建路徑
            if (currentNode.gridCell == targetCell)
            {
                Debug.Log("成功找到路徑！");
                List<GridCell> path = ReconstructPath(currentNode);
                float totalCost = currentNode.gCost;
                return (path, totalCost);
            }

            foreach (GridCell neighbor in gridManager.GetNeighbors(currentNode.gridCell))
            {
                if (!neighbor.isWalkable || closedList.Any(n => n.gridCell == neighbor) || neighbor.currentPiece != null && neighbor != targetCell)
                {
                    continue;
                }

                float tentativeGCost = currentNode.gCost + neighbor.movementCost;

                Node neighborNode = FindNodeInList(openList, neighbor);
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighbor, currentNode, tentativeGCost, GetHeuristic(neighbor, targetCell));
                    openList.Add(neighborNode);
                    Debug.Log($"添加到開放列表: {neighbor.gameObject.name}，fCost: {neighborNode.fCost}");
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.parent = currentNode;
                    Debug.Log($"更新節點: {neighbor.gameObject.name}，新的 fCost: {neighborNode.fCost}");
                }
            }
        }

        // 無法找到路徑
        Debug.LogWarning("無法找到從起點到終點的路徑！");
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
        // 曼哈頓距離
        return Mathf.Abs(a.gridPosition.x - b.gridPosition.x) + Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
    }

    private bool IsInList(HashSet<Node> list, GridCell cell)
    {
        foreach (Node node in list)
        {
            if (node.gridCell == cell)
                return true;
        }
        return false;
    }

    private Node FindNodeInList(List<Node> list, GridCell cell)
    {
        foreach (Node node in list)
        {
            if (node.gridCell == cell)
                return node;
        }
        return null;
    }

    private class Node
    {
        public GridCell gridCell;
        public Node parent;
        public float gCost; // 從起點到當前節點的成本
        public float hCost; // 從當前節點到終點的預估成本
        public float fCost { get { return gCost + hCost; } }

        public Node(GridCell cell, Node parentNode, float g, float h)
        {
            gridCell = cell;
            parent = parentNode;
            gCost = g;
            hCost = h;
        }
    }
}
