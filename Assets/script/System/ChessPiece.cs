using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour
{
    public Vector2Int gridPosition; // 棋子在棋盤上的位置
    public bool isPlayerControlled = true; // 是否由玩家控制
    public int movementRange = 3; // 移動範圍
    public int attackRange = 1; // 攻擊範圍
    public int health = 100; // 棋子的生命值

    protected GridManager gridManager;

    protected virtual void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    // 在棋子移動完成後更新 gridPosition
    public void UpdateGridPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
        Debug.Log($"{gameObject.name} 的 gridPosition 更新為 ({newPosition.x}, {newPosition.y})");
    }

    // 抽象方法，具體棋子需實現
    public abstract void TakeTurn();
    public virtual void ReceiveDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} 受到了 {damage} 傷害，剩餘生命值：{health}");
        if (health <= 0)
        {
            Die();
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
