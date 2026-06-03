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

    public bool jumpscarePlaying = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool isVisible(GameObject target)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return planes.All(p => p.GetDistanceToPoint(target.transform.position) >= 0);
    }
}
