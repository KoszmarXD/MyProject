using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }  // ��ҹ��
    private GridManager gridManager;
    private List<ChessPiece> playerPieces;
    private List<ChessPiece> aiPieces;
    public bool isPlayerTurn;
    private AIController aiController;

    // �^�X�����ƥ�A�ǻ���e�O�_�����a�^�X
    public event Action<bool> OnTurnChanged;
    private void Awake()
    {
        // �ˬd�O�_�w����Ҧs�b
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // �T�O�u�s�b�@�ӹ��
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        // ��l�ƪ��a�M AI �Ѥl�C��
        playerPieces = new List<ChessPiece>(FindObjectsOfType<ChessPiece>().Where(p => p.isPlayerControlled));
        aiPieces = new List<ChessPiece>(FindObjectsOfType<ChessPiece>().Where(p => !p.isPlayerControlled));

        aiController = FindObjectOfType<AIController>();
        StartPlayerTurn();
    }

    private void Update()
    {
        if (isPlayerTurn)
        {
            if (AllPlayerPiecesHaveMoved())
            {
                Debug.Log("���a�^�X�����A�}�l AI �^�X");
                SwitchTurn();
            }
        }
        else
        {
            if (AllAIActionsCompleted())
            {
                Debug.Log("AI �^�X�����A�}�l���a�^�X");
                SwitchTurn();
            }
        }
    }

    public bool AllPlayerPiecesHaveMoved()
    {
        return playerPieces.All(piece => piece.hasMoved && !piece.IsSelected);
    }

    private bool AllAIActionsCompleted()
    {
        return aiPieces.All(piece => piece.hasMoved && piece.hasAttacked);
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        ResetActions(playerPieces);
        NotifyTurnChanged();
    }

    public void StartAITurn()
    {
        isPlayerTurn = false;
        ResetActions(aiPieces);
        ExecuteAIActions();
    }

    private void ResetActions(List<ChessPiece> pieces)
    {
        foreach (var piece in pieces)
        {
            piece.ResetActions();
        }
    }

    private void ExecuteAIActions()
    {
        if (aiController != null)
        {
            aiController.TakeAITurn();
        }
        // AI �ʧ@������A�^�X�|�۰ʤ���
    }
    public void NotifyTurnChanged()
    {
        OnTurnChanged?.Invoke(isPlayerTurn);
    }

    // �����^�X����k
    public void SwitchTurn()
    {
        isPlayerTurn = !isPlayerTurn; // �����^�X���A
        GridManager gridManager = FindObjectOfType<GridManager>();

        if (isPlayerTurn)
        {
            Debug.Log("�����쪱�a�^�X");
            StartPlayerTurn();
        }
        else
        {
            Debug.Log("������ AI �^�X");
            StartAITurn();
        }
    }
    private void UpdateGridCells()
    {
        gridManager.ResetOccupiedCells();
    }
    public void EndTurn()
    {
        isPlayerTurn = false;
        TurnManager.Instance.SwitchTurn();
    }
}
