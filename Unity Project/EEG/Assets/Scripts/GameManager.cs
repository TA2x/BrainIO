using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public GameObject player;
    public Transform playerOrientation;

    public Camera cam;

    [SerializeField] private float gameLengthInSeconds = 600f;
    float gameTimer;
    public bool gameEnded = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameTimer = gameLengthInSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameEnded)
        {
            gameTimer -= Time.deltaTime;
            if (gameTimer <= 0f)
            {
                Debug.Log("You Win!");
                gameEnded = true;
            }
            else if (EEGReader.Instance.composure <= 0f)
            {
                Debug.Log("You Lose!");
                gameEnded = true;
            }
        }
    }

    public bool isVisible(Transform target)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return planes.All(p => p.GetDistanceToPoint(target.position) >= 0);
    }
}
