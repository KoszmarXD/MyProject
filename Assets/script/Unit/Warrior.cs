using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Warrior : ChessPiece
{
    protected override void Start()
    {
        base.Start();
        // ��l�ƨ�L�Ԥh�S�����ݩ�
        attackDamage = 25; // �]�w�Ԥh�������ˮ`
    }

    public override void TakeTurn()
    {
        // �Ԥh���欰�Ҧ��G���ʨç���
        //List<GridCell> availableMoves = gridManager.GetAvailableCells(gridPosition, movementRange, isPlayerControlled);
        // �o�̥i�H�K�[���ʩM�����������޿�
        if (hasMoved && hasAttacked)
        {
            Debug.Log($"{gameObject.name} �w�g�������ʩM������ʡA�����^�X");
            return;
        }

        ChessPiece target = DetectEnemy();
        if (!hasMoved && target == null)
        {
            SetHasMoved(true);
        }

        if (target != null && !hasAttacked)
        {
            Attack(target);
            SetHasAttacked(true);
        }
    }
    public override void ReceiveDamage(int damage)
    {
        base.ReceiveDamage(damage);
        // �A�i�H�b�o�̲K�[��h�Ԥh�S�������˦欰
        // �Ҧp������˰ʵe�B��ܨ��ˮĪG��
    }
    protected override void Die()
    {
        base.Die();
        // �A�i�H�b�o�̲K�[��h�Ԥh�S�������`�欰
        // �Ҧp���񦺤`�ʵe�B�������~��
    }    

    // �˴��P��O�_���ĤH
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
