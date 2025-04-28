//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(CheckBoardGenerator))]
//public class CheckBoardGeneratorEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector(); // 기본 Inspector UI 그리기

//        CheckBoardGenerator checkBoardGenerator = (CheckBoardGenerator)target;

//        // Generate 버튼 추가
//        if (GUILayout.Button("Generate Checkboard"))
//        {
//            checkBoardGenerator.GenerateCheckBoard();
//        }

//        // Clear Children 버튼 추가
//        if (GUILayout.Button("Clear Children"))
//        {
//            checkBoardGenerator.ClearChildren();
//        }
//    }
//}


//public class CheckBoardGenerator : MonoBehaviour
//{
//    public int xCount = 10; // 가로로 배치할 체크무늬의 개수
//    public int yCount = 10; // 세로로 배치할 체크무늬의 개수
//    public GameObject quadPrefab; // 체크무늬로 사용할 Quad 프리팹
    
//    [HideInInspector] public bool isGenerated = false; // 체크무늬가 생성되었는지 여부

//    public void GenerateCheckBoard()
//    {
//        if (isGenerated) return; // 이미 생성되었으면 아무것도 하지 않음

//        // 카메라 정보
//        Camera cam = Camera.main;
//        float aspectRatio = cam.aspect; // 화면의 가로 세로 비율
//        float orthoSize = cam.orthographicSize; // 카메라의 크기

//        // 화면의 크기에 맞춰 Quad의 크기 계산
//        float screenWidth = orthoSize * 2 * aspectRatio; // 화면의 가로 크기
//        float screenHeight = orthoSize * 2; // 화면의 세로 크기

//        // Quad가 화면을 빈틈없이 채우도록 크기 계산
//        float quadWidth = screenWidth / xCount;  // Quad 가로 크기 계산
//        float quadHeight = screenHeight / yCount; // Quad 세로 크기 계산

//        // Quad가 화면을 채우도록 중앙을 기준으로 시작 위치 계산
//        Vector3 startPosition = new Vector3(-(screenWidth) / 2 + quadWidth / 2,
//                                            (screenHeight) / 2 - quadHeight / 2,
//                                            0);

//        // Quad 생성
//        for (int x = 0; x < xCount; x++)
//        {
//            for (int y = 0; y < yCount; y++)
//            {
//                // 소수점 좌표를 기준으로 색상 계산
//                float normalizedX = (float)x / xCount;
//                float normalizedY = (float)y / yCount;

//                // 색상 계산: (x, y) 좌표를 기반으로 번갈아가며 색상 적용
//                Color color = ((int)(normalizedX * 100) + (int)(normalizedY * 100)) % 2 == 0 ? Color.white : Color.black;

//                // 새로운 Quad 인스턴스 생성
//                GameObject newQuad = Instantiate(quadPrefab, startPosition + new Vector3(x * quadWidth, -y * quadHeight, 0), Quaternion.identity);
//                newQuad.transform.SetParent(transform); // CheckBG의 자식으로 설정

//                // 색상 적용
//                Renderer quadRenderer = newQuad.GetComponent<Renderer>();
//                if (quadRenderer != null)
//                {
//                    quadRenderer.sharedMaterial.color = color;
//                }
//            }
//        }

//        isGenerated = true; // 체크무늬 생성 완료
//    }

//    // 자식 노드를 모두 지우는 메서드
//    public void ClearChildren()
//    {

//        // 자기 자신을 제외한 자식 노드들만 찾기
//        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
//        {
//            // 자기 자신을 제외한 자식만 삭제
//            if (child != transform)
//            {
//                DestroyImmediate(child.gameObject); // 자식 객체 삭제
//            }
//        }
//        isGenerated = false; // 생성된 체크무늬 상태 초기화
//    }
//}