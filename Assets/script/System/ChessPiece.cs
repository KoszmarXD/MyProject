using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour
{
    public GridCell CurrentCell { get; private set; }
    public Vector2Int gridPosition; // 棋子在棋盤上的位置
    public bool isPlayerControlled; // 是否由玩家控制
    public bool hasMoved { get; internal set; }    // 紀錄是否已移動
    public bool hasAttacked { get; internal set; } // 紀錄是否已攻擊
    public bool IsSelected { get; set; }          // 紀錄是否被選中

    public int movementRange = 3; // 移動範圍
    public int attackRange = 1; // 攻擊範圍
    public int attackDamage = 10; // 攻擊傷害，設為通用屬性
    public int health = 100; // 棋子的生命值

    public GridManager gridManager; // 參考 GridManager 的實例

    protected virtual void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }
    public void Initialize(GridManager manager)
    {
        gridManager = manager;
    }

    // 在棋子移動完成後更新 gridPosition
    public void UpdateGridPosition(Vector2Int newPosition, GridManager gridManager)
    {
        if (CurrentCell != null) // 確保釋放原格子
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
            Debug.LogWarning("GridManager 尚未初始化！");
        }
        // 同步更新棋子在 GridManager 上的狀態
        gridManager.UpdateGridCellState(gridManager.GetGridCell(gridPosition), true); // 設置新格
    }

    // 確保每次移動時更新 CurrentCell
    public void MoveToCell(GridCell newCell)
    {
        if (CurrentCell != null)
            CurrentCell.isOccupied = false; // 解除原位置的佔據狀態

        CurrentCell = newCell;
        CurrentCell.isOccupied = true; // 更新新位置的佔據狀態
        hasMoved = true; // 移動後標記
    }

    public void SetHasMoved(bool value)
    {
        hasMoved = value;
    }

    public void SetHasAttacked(bool value)
    {
        hasAttacked = value;
    }

    // 通用攻擊方法
    public virtual void Attack(ChessPiece target)
    {
        if (target != null)
        {
            target.ReceiveDamage(attackDamage);
            Debug.Log($"{gameObject.name} 攻擊了 {target.gameObject.name}，造成 {attackDamage} 傷害");
        }
    }
    // 抽象方法，具體棋子需實現
    public abstract void TakeTurn();

    public virtual void ResetActions()
    {
        hasMoved = false;
        hasAttacked = false;
    }
    public virtual void ReceiveDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} 受到了 {damage} 傷害，剩餘生命值：{health}");
        if (health <= 0)
        {
            Die();
        }
    }

    // 執行移動
    public void Move(Vector2Int targetPosition)
    {
        if (!hasMoved)
        {
            // 執行移動邏輯...
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
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 5f); // 速度可調整
                yield return null;
            }
        }
    }

    // 虛擬死亡方法，派生類可以覆寫
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} 已死亡");
        gridManager.RemoveChessPiece(this);
        Destroy(gameObject);
    }
}
