using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridCellEditor : MonoBehaviour
{
    public Button toggleWalkableButton;
    private GridCell selectedCell;

    void Start()
    {
        toggleWalkableButton.onClick.AddListener(ToggleWalkable);
    }

    void Update()
    {
        // ���]�A���@�Ӥ�k�ӳ]�m selectedCell�A�Ҧp�q�L GridCellSelector
        selectedCell = FindObjectOfType<GridCellSelector>().selectedCell;
    }

    void ToggleWalkable()
    {
        if (selectedCell != null)
        {
            selectedCell.isWalkable = !selectedCell.isWalkable;
            selectedCell.GetComponent<Renderer>().material.color = selectedCell.isWalkable ? Color.green : Color.red;
        }
    }
}
