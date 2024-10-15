using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour
{
    public Vector2Int gridPosition; // �Ѥl�b�ѽL�W����m
    public bool isPlayerControlled = true; // �O�_�Ѫ��a����
    public int movementRange = 3; // ���ʽd��
    public int attackRange = 1; // �����d��
    public int health = 100; // �Ѥl���ͩR��

    protected GridManager gridManager;

    protected virtual void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    // �b�Ѥl���ʧ������s gridPosition
    public void UpdateGridPosition(Vector2Int newPosition)
    {
        gridPosition = newPosition;
        Debug.Log($"{gameObject.name} �� gridPosition ��s�� ({newPosition.x}, {newPosition.y})");
    }

    // ��H��k�A����Ѥl�ݹ�{
    public abstract void TakeTurn();
    public virtual void ReceiveDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{gameObject.name} ����F {damage} �ˮ`�A�Ѿl�ͩR�ȡG{health}");
        if (health <= 0)
        {
            Die();
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
