using UnityEngine;

public class JumpscareManager : MonoBehaviour
{
    [SerializeField] GameObject[] jumpscares;

    [SerializeField] float jumpscareTimerMax;
    [SerializeField] float jumpscareTimerMin;
    float jumpscareTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jumpscareTimer = Random.Range(jumpscareTimerMin, jumpscareTimerMax);
    }

    // Update is called once per frame
    void Update()
    {
        jumpscareTimer -= Time.deltaTime;
        if (jumpscareTimer <= 0f)
        {
            Instantiate(jumpscares[Random.Range(0, jumpscares.Length)], GameManager.instance.player.transform.position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            jumpscareTimer = Random.Range(jumpscareTimerMin, jumpscareTimerMax);
        }
    }
}
