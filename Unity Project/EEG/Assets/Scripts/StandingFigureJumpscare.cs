using UnityEditor.Animations;
using UnityEngine;

public class StandingFigureJumpscare : MonoBehaviour
{
    [SerializeField] Transform figure;
    [SerializeField] float lifeInSeconds = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.rotation = Quaternion.Euler(0f, Random.rotation.eulerAngles.y, 0f);

        if (Physics.Raycast(figure.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f))
        {
            Vector3 pos = figure.position;
            pos.y = hit.point.y;
            figure.position = pos;
        }

        Invoke(nameof(DestroyAfterTime), lifeInSeconds);
    }

    // Update is called once per frame
    void Update()
    {
        figure.LookAt(GameManager.instance.player.transform);

        if (Vector3.Distance(figure.position, GameManager.instance.player.transform.position) <= 3f) Destroy(gameObject);
    }

    void DestroyAfterTime()
    {
        Destroy(gameObject);
    }
}
