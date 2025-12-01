using UnityEngine;
using UnityEngine.UIElements;

public class FloaterAnim : MonoBehaviour
{
    public Vector2 move;

    void Start()
    {
        Destroy(gameObject, 1.5f);
    }

    void Update()
    {
        gameObject.transform.position += (Vector3)(move * Time.deltaTime);
    }
}
