using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Warrior : ChessPiece
{
    public int attackDamage = 25;

    protected override void Start()
    {
        base.Start();
        // 初始化其他戰士特有的屬性
    }

    public override void TakeTurn()
    {
        // 戰士的行為模式：移動並攻擊
        List<GridCell> availableMoves = gridManager.GetAvailableCells(gridPosition, movementRange, isPlayerControlled);
        // 這裡可以添加移動和攻擊的具體邏輯
    }
    public override void ReceiveDamage(int damage)
    {
        base.ReceiveDamage(damage);
        // 你可以在這裡添加更多戰士特有的受傷行為
        // 例如播放受傷動畫、顯示受傷效果等
    }
    protected override void Die()
    {
        base.Die();
        // 你可以在這裡添加更多戰士特有的死亡行為
        // 例如播放死亡動畫、掉落物品等
    }

    // 攻擊方法
    public void Attack(ChessPiece target)
    {
        if (target != null)
        {
            target.ReceiveDamage(attackDamage);
            Debug.Log($"{gameObject.name} 攻擊了 {target.gameObject.name} 造成 {attackDamage} 傷害");
        }
    }

    // 檢測周圍是否有敵人
    public ChessPiece DetectEnemy()
    {
        List<ChessPiece> enemies = gridManager.GetEnemies(isPlayerControlled);
        foreach (var enemy in enemies)
        {
            int distance = Mathf.Abs(enemy.gridPosition.x - gridPosition.x) + Mathf.Abs(enemy.gridPosition.y - gridPosition.y);
            if (distance <= attackRange)
            {
                return enemy;
            }
        }
        return null;
    }
}
