using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] bool paused = false;

    PlayerInput input;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = new PlayerInput();
        input.Actions.Enable();

        input.Actions.Pause.performed += Pause;
    }

    public void Pause(InputAction.CallbackContext ctx)
    {
        if (paused)
        {
            menu.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            menu.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        paused = !paused;

        GameManager.instance.player.GetComponent<PlayerMovement>()._enabled = !paused;
        GameManager.instance.player.GetComponent<PlayerLook>()._enabled = !paused;
        EEGReader.Instance.isCalibrating = paused;
    }
    
    public void Resume()
    {
        menu.SetActive(false);
        Time.timeScale = 1f;
        GameManager.instance.player.GetComponent<PlayerMovement>()._enabled = true;
        GameManager.instance.player.GetComponent<PlayerLook>()._enabled = true;
        EEGReader.Instance.isCalibrating = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Menu()
    {
        EEGReader.Instance.isCalibrating = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameManager.instance.player.GetComponent<PlayerMovement>().input.Movement.Disable();
        SceneManager.LoadScene("GameCalibrationScene");
    }
}
