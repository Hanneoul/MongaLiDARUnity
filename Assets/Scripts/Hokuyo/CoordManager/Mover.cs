using UnityEngine;
using UnityEngine.UI;

public class Mover : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도

    public Text text;


    void Update()
    {
        text.text = transform.position.ToString();
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.Tab))
        {
                gameObject.SetActive(!gameObject.activeSelf);
        }

        if (Input.GetKey(KeyCode.W))
        {
            move.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            move.y -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            move.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            move.x += 1;
        }

        // 이동 적용 (프레임 독립적)
        transform.position += move.normalized * moveSpeed * Time.deltaTime;
    }
}
