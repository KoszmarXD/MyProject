using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    public static HighlightManager Instance;

    // 高亮預製件
    public GameObject highlightPrefab;

    // 存儲當前的高亮物件
    private List<GameObject> currentHighlights = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 在指定位置生成高亮
    public void HighlightCell(Vector3 position)
    {
        Vector3 highlightPosition = new Vector3(position.x, position.y + 0.1f, position.z);
        GameObject highlight = Instantiate(highlightPrefab, highlightPosition, Quaternion.identity);
        currentHighlights.Add(highlight);
        Debug.Log($"Highlighting Cell at {position}");
        ClearHighlights(); // 清除之前的高亮
    }

    // 在指定棋子上生成高亮
    public void HighlightUnit(Transform unitTransform)
    {
        ClearHighlights(); // 清除之前的高亮
        Vector3 highlightPosition = unitTransform.position + Vector3.up * 0.05f;
        GameObject highlight = Instantiate(highlightPrefab, highlightPosition, Quaternion.identity);
        currentHighlights.Add(highlight);
        Debug.Log($"Highlighting Unit at {unitTransform.position}");
    }

    // 清除所有高亮
    public void ClearHighlights()
    {
        foreach (GameObject highlight in currentHighlights)
        {
            Destroy(highlight);
        }
        currentHighlights.Clear();
        Debug.Log("Cleared all highlights.");
    }
    // 清除特定類型的高亮（可選）
    public void ClearHighlights(bool clearUnits)
    {
        if (clearUnits)
        {
            // 假設您使用不同的標籤來區分高亮類型
            // 例如，為單位高亮設置標籤 "UnitHighlight"
            GameObject[] unitHighlights = GameObject.FindGameObjectsWithTag("UnitHighlight");
            foreach (GameObject highlight in unitHighlights)
            {
                Destroy(highlight);
            }
        }
        else
        {
            ClearHighlights();
        }
    }
}
