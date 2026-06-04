using UnityEngine;

public class JumpscareManager : MonoBehaviour
{
    [SerializeField] GameObject[] jumpscares;

    [SerializeField] float jumpscareTimerMax;
    [SerializeField] float jumpscareTimerMin;
    float jumpscareTimer;

    [SerializeField] string previousJumpscareName = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jumpscareTimer = Random.Range(jumpscareTimerMin, jumpscareTimerMax);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.gameEnded) return;

        jumpscareTimer -= Time.deltaTime;
        if (jumpscareTimer <= 0f)
        {
            int random = Random.Range(0, jumpscares.Length);
            while (jumpscares[random].name == previousJumpscareName)
            {
                random = Random.Range(0, jumpscares.Length);
            }

            GameObject jumpscare = Instantiate(jumpscares[random], GameManager.instance.player.transform.position, Quaternion.identity);
            jumpscare.name = jumpscares[random].name;
            previousJumpscareName = jumpscare.name;
            jumpscareTimer = Random.Range(jumpscareTimerMin, jumpscareTimerMax);
        }
    }
}
