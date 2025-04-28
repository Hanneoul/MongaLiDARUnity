//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(CheckBoardGenerator))]
//public class CheckBoardGeneratorEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector(); // �⺻ Inspector UI �׸���

//        CheckBoardGenerator checkBoardGenerator = (CheckBoardGenerator)target;

//        // Generate ��ư �߰�
//        if (GUILayout.Button("Generate Checkboard"))
//        {
//            checkBoardGenerator.GenerateCheckBoard();
//        }

//        // Clear Children ��ư �߰�
//        if (GUILayout.Button("Clear Children"))
//        {
//            checkBoardGenerator.ClearChildren();
//        }
//    }
//}


//public class CheckBoardGenerator : MonoBehaviour
//{
//    public int xCount = 10; // ���η� ��ġ�� üũ������ ����
//    public int yCount = 10; // ���η� ��ġ�� üũ������ ����
//    public GameObject quadPrefab; // üũ���̷� ����� Quad ������
    
//    [HideInInspector] public bool isGenerated = false; // üũ���̰� �����Ǿ����� ����

//    public void GenerateCheckBoard()
//    {
//        if (isGenerated) return; // �̹� �����Ǿ����� �ƹ��͵� ���� ����

//        // ī�޶� ����
//        Camera cam = Camera.main;
//        float aspectRatio = cam.aspect; // ȭ���� ���� ���� ����
//        float orthoSize = cam.orthographicSize; // ī�޶��� ũ��

//        // ȭ���� ũ�⿡ ���� Quad�� ũ�� ���
//        float screenWidth = orthoSize * 2 * aspectRatio; // ȭ���� ���� ũ��
//        float screenHeight = orthoSize * 2; // ȭ���� ���� ũ��

//        // Quad�� ȭ���� ��ƴ���� ä�쵵�� ũ�� ���
//        float quadWidth = screenWidth / xCount;  // Quad ���� ũ�� ���
//        float quadHeight = screenHeight / yCount; // Quad ���� ũ�� ���

//        // Quad�� ȭ���� ä�쵵�� �߾��� �������� ���� ��ġ ���
//        Vector3 startPosition = new Vector3(-(screenWidth) / 2 + quadWidth / 2,
//                                            (screenHeight) / 2 - quadHeight / 2,
//                                            0);

//        // Quad ����
//        for (int x = 0; x < xCount; x++)
//        {
//            for (int y = 0; y < yCount; y++)
//            {
//                // �Ҽ��� ��ǥ�� �������� ���� ���
//                float normalizedX = (float)x / xCount;
//                float normalizedY = (float)y / yCount;

//                // ���� ���: (x, y) ��ǥ�� ������� �����ư��� ���� ����
//                Color color = ((int)(normalizedX * 100) + (int)(normalizedY * 100)) % 2 == 0 ? Color.white : Color.black;

//                // ���ο� Quad �ν��Ͻ� ����
//                GameObject newQuad = Instantiate(quadPrefab, startPosition + new Vector3(x * quadWidth, -y * quadHeight, 0), Quaternion.identity);
//                newQuad.transform.SetParent(transform); // CheckBG�� �ڽ����� ����

//                // ���� ����
//                Renderer quadRenderer = newQuad.GetComponent<Renderer>();
//                if (quadRenderer != null)
//                {
//                    quadRenderer.sharedMaterial.color = color;
//                }
//            }
//        }

//        isGenerated = true; // üũ���� ���� �Ϸ�
//    }

//    // �ڽ� ��带 ��� ����� �޼���
//    public void ClearChildren()
//    {

//        // �ڱ� �ڽ��� ������ �ڽ� ���鸸 ã��
//        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
//        {
//            // �ڱ� �ڽ��� ������ �ڽĸ� ����
//            if (child != transform)
//            {
//                DestroyImmediate(child.gameObject); // �ڽ� ��ü ����
//            }
//        }
//        isGenerated = false; // ������ üũ���� ���� �ʱ�ȭ
//    }
//}