using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public bool _enabled = true;

    PlayerInput input;

    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cam;

    [SerializeField] private float sensitivity;

    float xRotation;
    float yRotation;

    private void Awake()
    {
        input = new PlayerInput();
        input.Movement.Enable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_enabled) return;

        float mouseX = input.Movement.MouseX.ReadValue<float>() * sensitivity;
        float mouseY = input.Movement.MouseY.ReadValue<float>() * sensitivity;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}
