using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour
{
    public GridCell CurrentCell { get; private set; }
    public Vector2Int gridPosition; // �Ѥl�b�ѽL�W����m
    public bool isPlayerControlled; // �O�_�Ѫ��a����
    public bool hasMoved { get; internal set; }    // �����O�_�w����
    public bool hasAttacked { get; internal set; } // �����O�_�w����
    public bool IsSelected { get; set; }          // �����O�_�Q�襤

    public int movementRange = 3; // ���ʽd��
    public int attackRange = 1; // �����d��
    public int attackDamage = 10; // �����ˮ`�A�]���q���ݩ�
    public int health = 100; // �Ѥl���ͩR��

    public GridManager gridManager; // �Ѧ� GridManager �����

    protected virtual void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }
    public void Initialize(GridManager manager)
    {
        gridManager = manager;
    }

    // �b�Ѥl���ʧ������s gridPosition
    public void UpdateGridPosition(Vector2Int newPosition, GridManager gridManager)
    {
        if (CurrentCell != null) // �T�O������l
            CurrentCell.isOccupied = false;

        gridPosition = newPosition;
        CurrentCell = gridManager?.GetGridCell(newPosition);

        if (gridManager != null)
        {
            GridCell targetCell = gridManager.GetGridCell(newPosition);
            if (targetCell != null)
            {
                transform.position = targetCell.transform.position;
            }
        }
        else
        {
            Debug.LogWarning("GridManager �|����l�ơI");
        }
        // �P�B��s�Ѥl�b GridManager �W�����A
        gridManager.UpdateGridCellState(gridManager.GetGridCell(gridPosition), true); // �]�m�s��
    }

    // �T�O�C�����ʮɧ�s CurrentCell
    public void MoveToCell(GridCell newCell)
    {
        if (CurrentCell != null)
            CurrentCell.isOccupied = false; // �Ѱ����m�����ڪ��A

        CurrentCell = newCell;
        CurrentCell.isOccupied = true; // ��s�s��m�����ڪ��A
        hasMoved = true; // ���ʫ�аO
    }

    public void SetHasMoved(bool value)
    {
        hasMoved = value;
    }

    public void SetHasAttacked(bool value)
    {
        hasAttacked = value;
    }

    // �q�Χ�����k
    public virtual void Attack(ChessPiece target)
    {
        if (target != null)
        {
            target.ReceiveDamage(attackDamage);
            Debug.Log($"{gameObject.name} �����F {target.gameObject.name}�A�y�� {attackDamage} �ˮ`");
        }
    }
    // ��H��k�A����Ѥl�ݹ�{
    public abstract void TakeTurn();

    public virtual void ResetActions()
    {
        hasMoved = false;
        hasAttacked = false;
    }
    public virtual void ReceiveDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} ����F {damage} �ˮ`�A�Ѿl�ͩR�ȡG{health}");
        if (health <= 0)
        {
            Die();
        }
    }

    // ���沾��
    public void Move(Vector2Int targetPosition)
    {
        if (!hasMoved)
        {
            // ���沾���޿�...
            hasMoved = true;
        }
    }
    public void MoveAlongPath(List<GridCell> path)
    {
        StartCoroutine(MoveCoroutine(path));
    }

    private IEnumerator MoveCoroutine(List<GridCell> path)
    {
        foreach (var cell in path)
        {
            Vector3 targetPosition = new Vector3(cell.transform.position.x, transform.position.y, cell.transform.position.z);
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 5f); // �t�ץi�վ�
                yield return null;
            }
        }
    }

    // �������`��k�A�������i�H�мg
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} �w���`");
        gridManager.RemoveChessPiece(this);
        Destroy(gameObject);
    }
}
