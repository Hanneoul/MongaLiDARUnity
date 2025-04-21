using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CheckBoardGenerator))]
public class CheckBoardGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // �⺻ Inspector UI �׸���

        CheckBoardGenerator checkBoardGenerator = (CheckBoardGenerator)target;

        // Generate ��ư �߰�
        if (GUILayout.Button("Generate Checkboard"))
        {
            checkBoardGenerator.GenerateCheckBoard();
        }

        // Clear Children ��ư �߰�
        if (GUILayout.Button("Clear Children"))
        {
            checkBoardGenerator.ClearChildren();
        }
    }
}


public class CheckBoardGenerator : MonoBehaviour
{
    public int xCount = 10; // ���η� ��ġ�� üũ������ ����
    public int yCount = 10; // ���η� ��ġ�� üũ������ ����
    public GameObject quadPrefab; // üũ���̷� ����� Quad ������
    public float spacing = 0f; // Quad ���� ���� (�⺻�� 0, �ٷ� �ٰ� ����)

    [HideInInspector] public bool isGenerated = false; // üũ���̰� �����Ǿ����� ����

    // ��ư�� ������ ȣ��Ǵ� �޼���
    public void GenerateCheckBoard()
    {
        if (isGenerated) return; // �̹� �����Ǿ����� �ƹ��͵� ���� ����

        // ī�޶� ����
        Camera cam = Camera.main;
        float aspectRatio = cam.aspect; // ȭ���� ���� ���� ����
        float orthoSize = cam.orthographicSize; // ī�޶��� ũ��

        // ȭ���� ũ�⿡ ���� Quad�� ũ�� ���
        float quadWidth = (orthoSize * 2 * aspectRatio) / xCount;
        float quadHeight = (orthoSize * 2) / yCount;

        // Quad�� ȭ���� ä�쵵�� �߾��� �������� ���� ��ġ ���
        Vector3 startPosition = new Vector3(-(quadWidth * xCount) / 2 + quadWidth / 2,
                                            (quadHeight * yCount) / 2 - quadHeight / 2,
                                            0);

        // Quad ����
        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                // ���� ����: ¦�� ��ġ�� ���, Ȧ�� ��ġ�� ������
                Color color = (x + y) % 2 == 0 ? Color.white : Color.black;

                // ���ο� Quad �ν��Ͻ� ����
                GameObject newQuad = Instantiate(quadPrefab, startPosition + new Vector3(x * (quadWidth + spacing), -y * (quadHeight + spacing), 0), Quaternion.identity);
                newQuad.transform.SetParent(transform); // CheckBG�� �ڽ����� ����

                // ���� ����
                Renderer quadRenderer = newQuad.GetComponent<Renderer>();
                if (quadRenderer != null)
                {
                    quadRenderer.sharedMaterial.color = color;
                }
            }
        }

        isGenerated = true; // üũ���� ���� �Ϸ�
    }

    // �ڽ� ��带 ��� ����� �޼���
    public void ClearChildren()
    {

        // �ڱ� �ڽ��� ������ �ڽ� ���鸸 ã��
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            // �ڱ� �ڽ��� ������ �ڽĸ� ����
            if (child != transform)
            {
                DestroyImmediate(child.gameObject); // �ڽ� ��ü ����
            }
        }
        isGenerated = false; // ������ üũ���� ���� �ʱ�ȭ
    }
}