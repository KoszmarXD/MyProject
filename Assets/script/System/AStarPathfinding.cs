using System.Collections;
using System.Collections.Generic;
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

    public List<GridCell> FindPath(Vector2Int startPos, Vector2Int targetPos)
    {
        GridCell startCell = gridManager.GetGridCell(startPos);
        GridCell targetCell = gridManager.GetGridCell(targetPos);

        if (startCell == null || targetCell == null)
        {
            Debug.LogError("�_�I�β��I��l�� null�I");
            return null;
        }

        if (!targetCell.isWalkable)
        {
            Debug.LogError("���I��l���i�樫�I");
            return null;
        }

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        Node startNode = new Node(startCell, null, 0, GetHeuristic(startCell, targetCell));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // ��� f �ȳ̧C���`�I
            openList.Sort((a, b) => a.fCost.CompareTo(b.fCost));
            Node currentNode = openList[0];
            openList.RemoveAt(0);
            closedList.Add(currentNode);

            // �p�G��F���I�A���ظ��|
            if (currentNode.gridCell == targetCell)
            {
                return ReconstructPath(currentNode);
            }

            foreach (GridCell neighbor in gridManager.GetNeighbors(currentNode.gridCell))
            {
                if (!neighbor.isWalkable || IsInList(closedList, neighbor))
                {
                    continue;
                }

                float tentativeGCost = currentNode.gCost + neighbor.movementCost;

                Node neighborNode = FindNodeInList(openList, neighbor);
                if (neighborNode == null)
                {
                    neighborNode = new Node(neighbor, currentNode, tentativeGCost, GetHeuristic(neighbor, targetCell));
                    openList.Add(neighborNode);
                }
                else if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.parent = currentNode;
                }
            }
        }

        // �L�k�����|
        Debug.LogWarning("�L�k���q�_�I����I�����|�I");
        return null;
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
    }
}
