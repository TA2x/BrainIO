using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EEGCalibration : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] Button startButton;
    [SerializeField] float calibrationTimeVal = 15f;
    float calibrationTime;
    bool calibrating;

    private void Start()
    {
        calibrationTime = calibrationTimeVal;
    }

    private void Update()
    {
        if (calibrating)
        {
            calibrationTime -= Time.deltaTime;
            statusText.text = "Calibration in progress. Sit still and breath normally." +
                "\nYou will load into the game in " + (int)calibrationTime + " seconds.";
            if (calibrationTime < 0f)
            {
                LoadGame();
            }
        }
    }

    public void StartCalibration()
    {
        EEGReader.Instance.StartCalibration();
        startButton.interactable = false;
        calibrating = true;
    }

    void LoadGame()
    {
        EEGReader.Instance.FinishCalibration();
        SceneManager.LoadScene("GameScene");
    }
}
