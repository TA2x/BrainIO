using System.Collections;
using UnityEngine;

public class BreathingJumpscare : MonoBehaviour
{
    [SerializeField] Transform figure;
    [SerializeField] float speed = 10f;
    [SerializeField] float footstepsSpeed = 0.35f;

    [SerializeField] float lifeInSeconds = 5f;

    [SerializeField] AudioClip[] footsteps;
    [SerializeField] AudioSource footstepsSource;
    [SerializeField] AudioSource breathingSource;

    bool breath = false;
    bool breathStarted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.instance.player.GetComponent<PlayerMovement>()._enabled = false;
        GameManager.instance.player.GetComponent<PlayerLook>()._enabled = false;
        transform.rotation = Quaternion.Euler(transform.rotation.x, GameManager.instance.playerOrientation.rotation.eulerAngles.y, transform.rotation.z);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = GameManager.instance.player.transform.position;
        targetPos.y = figure.position.y;

        if (!breath)
        {
            figure.position = Vector3.MoveTowards(figure.position, targetPos, Time.deltaTime * speed);

            if (Physics.Raycast(figure.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f))
            {
                Vector3 pos = figure.position;
                pos.y = hit.point.y;
                figure.position = pos;
            }

            if (!IsInvoking(nameof(PlayFootstep)))
            {
                Invoke(nameof(PlayFootstep), footstepsSpeed);
            }
        }
        else
        {
            if (!breathStarted)
            {
                CancelInvoke(nameof(PlayFootstep));
                StartCoroutine(Breath());
                breathStarted = true;
            }
        }

        if (Vector3.Distance(figure.position, targetPos) <= 1.5f) breath = true;
    }

    void PlayFootstep()
    {
        footstepsSource.clip = footsteps[Random.Range(0, footsteps.Length)];
        footstepsSource.Play();
    }

    IEnumerator Breath()
    {
        footstepsSource.Stop();
        breathingSource.Play();

        yield return new WaitForSeconds(lifeInSeconds);

        GameManager.instance.player.GetComponent<PlayerMovement>()._enabled = true;
        GameManager.instance.player.GetComponent<PlayerLook>()._enabled = true;
        Destroy(gameObject);
    }
}
