using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraPos;
    [SerializeField] private Transform cam;

    // Update is called once per frame
    void Update()
    {
        cam.position = cameraPos.position;
    }
}
