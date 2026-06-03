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
        if (GameManager.instance.jumpscarePlaying) return;

        jumpscareTimer -= Time.deltaTime;
        if (jumpscareTimer <= 0f)
        {
            Instantiate(jumpscares[Random.Range(0, jumpscares.Length)], GameManager.instance.player.transform.position, Quaternion.identity);
            jumpscareTimer = Random.Range(jumpscareTimerMin, jumpscareTimerMax);
        }
    }
}
