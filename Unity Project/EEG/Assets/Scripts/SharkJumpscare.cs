using UnityEngine;

public class SharkJumpscare : MonoBehaviour
{
    [SerializeField] Transform shark;
    bool seen = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.instance.jumpscarePlaying = true;
        transform.rotation = GameManager.instance.playerOrientation.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        shark.LookAt(GameManager.instance.player.transform);
        transform.position = GameManager.instance.player.transform.position;

        if (GameManager.instance.isVisible(shark.gameObject) && !seen)
        {
            GameManager.instance.jumpscarePlaying = false;
            Destroy(gameObject, 1f);
        }
    }
}
