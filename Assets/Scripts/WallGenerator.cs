using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WallGenerator))]
public class WallGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WallGenerator script = (WallGenerator)target;

        if (GUILayout.Button("Generate Wall"))
        {
            script.GenerateWall();
        }
    }
}


public class WallGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float quadSize = 1f;

    void Start()
    {
        GenerateWall();
    }

    public void GenerateWall()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.parent = transform;
                quad.transform.localPosition = new Vector3(x * quadSize, y * quadSize, 0);
                quad.transform.localScale = new Vector3(quadSize, quadSize, 1);

                // 체크무늬 색상 적용
                Renderer renderer = quad.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Unlit/Color"));
                renderer.material.color = (x + y) % 2 == 0 ? Color.black : Color.white;
            }
        }
    }
}
