using UnityEngine;

public class Sand2CubeCreator : MonoBehaviour
{
    void Start()
    {
        // Tạo Cube tại vị trí (0, 0.5, 0) với tên "Sand2Cube"
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 0.5f, 0);
        cube.name = "Sand2Cube";
    }
}
