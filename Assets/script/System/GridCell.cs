using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainTypeEnum
{
    Grass,
    Water,
    Mountain,
    // �i�H�K�[��h�a������
}
public class GridCell : MonoBehaviour
{
    [Tooltip("��l�b�ѽL�W�� X ��m")]
    public int xPosition;

    [Tooltip("��l�b�ѽL�W�� Z ��m")]
    public int zPosition;

    // �ϥ� Vector2Int �Ӧs�x��m
    public Vector2Int gridPosition
    {
        get { return new Vector2Int(xPosition, zPosition); }
        set
        {
            xPosition = value.x;
            zPosition = value.y;
        }
    }

    [Tooltip("�O�_�i�樫")]
    public bool isWalkable = true;

    [Tooltip("���ʦ���")]
    public int movementCost = 1;

    [Tooltip("�a������")]
    public TerrainTypeEnum terrainType = TerrainTypeEnum.Grass;

    [Tooltip("���G����")]
    public Material highlightMaterial; // �b Inspector ���]�m

    private Renderer rend;
    private Material originalMaterial;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError($"{gameObject.name} �ʤ� Renderer �ե�I");
            return;
        }
        SetMaterialBasedOnTerrain();
    }

    // �ھڦa�������]�m��l�C��
    public void SetMaterialBasedOnTerrain()
    {
        switch (terrainType)
        {
            case TerrainTypeEnum.Grass:
                originalMaterial = Resources.Load<Material>("Grass"); // �T�O�A���@�� GrassMaterial �s�b�� Resources ��󧨤�
                break;
            case TerrainTypeEnum.Water:
                originalMaterial = Resources.Load<Material>("Water"); // �P�W
                break;
            case TerrainTypeEnum.Mountain:
                originalMaterial = Resources.Load<Material>("Mountain"); // �P�W
                break;
            default:
                originalMaterial = rend.material; // �q�{�ϥη�e����
                break;
        }
        if (originalMaterial != null)
        {
            rend.material = originalMaterial;
            Debug.Log($"{gameObject.name} �]�m���謰 {originalMaterial.name}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} ������������l����I");
        }
    }

    // ���G��ܮ�l
    public void Highlight()
    {
        if (highlightMaterial != null)
        {
            rend.material = highlightMaterial;
            Debug.Log($"{gameObject.name} �Q���G���");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} �S���]�m Highlight Material�I");
        }
    }

    // ���m��l���C��
    public void ResetMaterial()
    {
        if (originalMaterial != null)
        {
            rend.material = originalMaterial;
            Debug.Log($"{gameObject.name} ���譫�m�� {originalMaterial.name}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} ��l���謰 null�A�L�k���m�I");
        }
    }
    
}
