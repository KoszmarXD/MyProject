using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    public static HighlightManager Instance;

    // ���G�w�s��
    public GameObject highlightPrefab;

    // �s�x��e�����G����
    private List<GameObject> currentHighlights = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // �b���w��m�ͦ����G
    public void HighlightCell(Vector3 position)
    {
        Vector3 highlightPosition = new Vector3(position.x, position.y + 0.1f, position.z);
        GameObject highlight = Instantiate(highlightPrefab, highlightPosition, Quaternion.identity);
        currentHighlights.Add(highlight);
        Debug.Log($"Highlighting Cell at {position}");
        ClearHighlights(); // �M�����e�����G
    }

    // �b���w�Ѥl�W�ͦ����G
    public void HighlightUnit(Transform unitTransform)
    {
        ClearHighlights(); // �M�����e�����G
        Vector3 highlightPosition = unitTransform.position + Vector3.up * 0.05f;
        GameObject highlight = Instantiate(highlightPrefab, highlightPosition, Quaternion.identity);
        currentHighlights.Add(highlight);
        Debug.Log($"Highlighting Unit at {unitTransform.position}");
    }

    // �M���Ҧ����G
    public void ClearHighlights()
    {
        foreach (GameObject highlight in currentHighlights)
        {
            Destroy(highlight);
        }
        currentHighlights.Clear();
        Debug.Log("Cleared all highlights.");
    }
    // �M���S�w���������G�]�i��^
    public void ClearHighlights(bool clearUnits)
    {
        if (clearUnits)
        {
            // ���]�z�ϥΤ��P�����ҨӰϤ����G����
            // �Ҧp�A����찪�G�]�m���� "UnitHighlight"
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
