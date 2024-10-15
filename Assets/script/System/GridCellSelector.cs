using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridCellSelector : MonoBehaviour
{
    private GridManager gridManager;
    public GridCell selectedCell;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GridCell cell = hit.collider.GetComponent<GridCell>();
                if (cell != null)
                {
                    if (selectedCell != null)
                    {
                        // 恢復之前選中格子的顏色
                        selectedCell.ResetMaterial();
                    }

                    selectedCell = cell;
                    // 設定高亮顏色
                    selectedCell.GetComponent<Renderer>().material.color = Color.yellow;
                }
            }
        }
    }
}
