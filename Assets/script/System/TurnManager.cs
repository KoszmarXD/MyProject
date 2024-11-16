using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }  // 單例實例
    private GridManager gridManager;
    private List<ChessPiece> playerPieces;
    private List<ChessPiece> aiPieces;
    public bool isPlayerTurn;
    private AIController aiController;

    // 回合切換事件，傳遞當前是否為玩家回合
    public event Action<bool> OnTurnChanged;
    private void Awake()
    {
        // 檢查是否已有實例存在
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // 確保只存在一個實例
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        // 初始化玩家和 AI 棋子列表
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
                Debug.Log("玩家回合結束，開始 AI 回合");
                SwitchTurn();
            }
        }
        else
        {
            if (AllAIActionsCompleted())
            {
                Debug.Log("AI 回合結束，開始玩家回合");
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
        // AI 動作完成後，回合會自動切換
    }
    public void NotifyTurnChanged()
    {
        OnTurnChanged?.Invoke(isPlayerTurn);
    }

    // 切換回合的方法
    public void SwitchTurn()
    {
        isPlayerTurn = !isPlayerTurn; // 切換回合狀態
        GridManager gridManager = FindObjectOfType<GridManager>();

        if (isPlayerTurn)
        {
            Debug.Log("切換到玩家回合");
            StartPlayerTurn();
        }
        else
        {
            Debug.Log("切換到 AI 回合");
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
