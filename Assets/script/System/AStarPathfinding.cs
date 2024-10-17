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

    // 基於成本的搜尋，找到所有可達的格子
    public List<GridCell> FindAccessibleCells(Vector2Int startPos, float maxCost)
    {
        List<GridCell> accessible = new List<GridCell>();
        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        HashSet<GridCell> closedList = new HashSet<GridCell>();
        HashSet<GridCell> openSet = new HashSet<GridCell>(); // 用於跟蹤已添加到開放列表的格子

        GridCell startCell = gridManager.GetGridCell(startPos);
        if (startCell == null)
        {
            Debug.LogError("起點格子為 null！");
            return accessible;
        }

        Node startNode = new Node(startCell, null, 0f, 0f); // hCost 設置為 0
        openList.Enqueue(startNode, startNode.fCost);
        openSet.Add(startCell);
        Debug.Log($"起始節點: {startCell.gameObject.name} at ({startCell.gridPosition.x}, {startCell.gridPosition.y})");

        int maxIterations = 10000; // 設置一個最大迭代次數以防止無限迴圈
        int iterations = 0;

        while (!openList.IsEmpty() && iterations < maxIterations)
        {
            Node currentNode = openList.Dequeue();
            openSet.Remove(currentNode.gridCell); // 從開放集合中移除
            iterations++;

            Debug.Log($"迭代 {iterations}: 處理節點: {currentNode.gridCell.gameObject.name} at ({currentNode.gridCell.gridPosition.x}, {currentNode.gridCell.gridPosition.y})");

            if (closedList.Contains(currentNode.gridCell))
            {
                Debug.Log($"節點 {currentNode.gridCell.gameObject.name} 已在封閉列表中，跳過。");
                continue;
            }

            closedList.Add(currentNode.gridCell);

            // 如果當前總成本超過最大成本，停止擴展
            if (currentNode.gCost > maxCost)
            {
                Debug.Log($"節點 {currentNode.gridCell.gameObject.name} 的 gCost ({currentNode.gCost}) 超過最大成本 ({maxCost})，停止擴展。");
                continue;
            }

            accessible.Add(currentNode.gridCell);
            Debug.Log($"添加到可達列表: {currentNode.gridCell.gameObject.name}，gCost: {currentNode.gCost}");

            foreach (GridCell neighbor in gridManager.GetNeighbors(currentNode.gridCell))
            {
                if (!neighbor.isWalkable)
                {
                    Debug.Log($"鄰居 {neighbor.gameObject.name} 不可行走，跳過。");
                    continue;
                }

                if (closedList.Contains(neighbor))
                {
                    Debug.Log($"鄰居 {neighbor.gameObject.name} 已在封閉列表中，跳過。");
                    continue;
                }

                float tentativeGCost = currentNode.gCost + neighbor.movementCost;
                Debug.Log($"計算鄰居 {neighbor.gameObject.name} 的 tentativeGCost: {tentativeGCost}");

                if (tentativeGCost > maxCost)
                {
                    Debug.Log($"鄰居 {neighbor.gameObject.name} 的 tentativeGCost ({tentativeGCost}) 超過最大成本 ({maxCost})，跳過。");
                    continue;
                }

                // 在 FindAccessibleCells 中，hCost 設置為 0
                Node neighborNode = new Node(neighbor, currentNode, tentativeGCost, 0f);

                if (!openSet.Contains(neighbor))
                {
                    openList.Enqueue(neighborNode, neighborNode.fCost);
                    openSet.Add(neighbor);
                    Debug.Log($"添加到開放列表: {neighbor.gameObject.name}，fCost: {neighborNode.fCost}");
                }
                else
                {
                    Debug.Log($"鄰居 {neighbor.gameObject.name} 已在開放列表中，跳過。");
                }
            }
        }

        if (iterations >= maxIterations)
        {
            Debug.LogError("A* 路徑尋找達到最大迭代次數，可能存在無限迴圈。");
        }

        Debug.Log($"找到 {accessible.Count} 個可移動格子從 ({startPos.x}, {startPos.y}) 範圍內，總迭代次數: {iterations}");
        return accessible;
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

        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        HashSet<GridCell> closedList = new HashSet<GridCell>();
        HashSet<GridCell> openSet = new HashSet<GridCell>(); // 跟蹤已添加到開放列表的格子

        Node startNode = new Node(startCell, null, 0f, GetHeuristic(startCell, targetCell));
        openList.Enqueue(startNode, startNode.fCost);
        openSet.Add(startCell);
        Debug.Log($"起始節點: {startCell.gameObject.name} at ({startCell.gridPosition.x}, {startCell.gridPosition.y})");

        int maxIterations = 10000; // 設置一個最大迭代次數以防止無限迴圈
        int iterations = 0;

        while (!openList.IsEmpty() && iterations < maxIterations)
        {
            Node currentNode = openList.Dequeue();
            openSet.Remove(currentNode.gridCell); // 從開放集合中移除
            iterations++;

            Debug.Log($"迭代 {iterations}: 處理節點: {currentNode.gridCell.gameObject.name} at ({currentNode.gridCell.gridPosition.x}, {currentNode.gridCell.gridPosition.y})");

            if (closedList.Contains(currentNode.gridCell))
            {
                Debug.Log($"節點 {currentNode.gridCell.gameObject.name} 已在封閉列表中，跳過。");
                continue;
            }

            closedList.Add(currentNode.gridCell);

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
                if (!neighbor.isWalkable)
                {
                    Debug.Log($"鄰居 {neighbor.gameObject.name} 不可行走，跳過。");
                    continue;
                }

                if (closedList.Contains(neighbor))
                {
                    Debug.Log($"鄰居 {neighbor.gameObject.name} 已在封閉列表中，跳過。");
                    continue;
                }

                float tentativeGCost = currentNode.gCost + neighbor.movementCost;
                Debug.Log($"計算鄰居 {neighbor.gameObject.name} 的 tentativeGCost: {tentativeGCost}");


                Node neighborNode = new Node(neighbor, currentNode, tentativeGCost, GetHeuristic(neighbor, targetCell));

                if (!openSet.Contains(neighbor))
                {
                    openList.Enqueue(neighborNode, neighborNode.fCost);
                    openSet.Add(neighbor);
                    Debug.Log($"添加到開放列表: {neighbor.gameObject.name}，fCost: {neighborNode.fCost}");
                }
                else
                {
                    Debug.Log($"鄰居 {neighbor.gameObject.name} 已在開放列表中，跳過。");
                }
            }
        }

        if (iterations >= maxIterations)
        {
            Debug.LogError("A* 路徑尋找達到最大迭代次數，可能存在無限迴圈。");
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

        // 重寫 Equals 和 GetHashCode，以便在 PriorityQueue 中正確比較節點
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
