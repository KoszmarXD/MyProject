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
                        // ��_���e�襤��l���C��
                        selectedCell.ResetMaterial();
                    }

                    selectedCell = cell;
                    // �]�w���G�C��
                    selectedCell.GetComponent<Renderer>().material.color = Color.yellow;
                }
            }
        }
    }
}
