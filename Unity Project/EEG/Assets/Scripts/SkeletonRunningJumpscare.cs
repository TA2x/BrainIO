using UnityEditor.Animations;
using UnityEngine;

public class SkeletonRunningJumpscare : MonoBehaviour
{
    [SerializeField] Transform skeleton;
    [SerializeField] float speed = 10f;
    [SerializeField] float footstepsSpeed = 0.35f;

    [SerializeField] AudioClip[] footsteps;
    [SerializeField] AudioSource footstepsSource;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(GameManager.instance.player.transform);

        Vector3 targetPos = GameManager.instance.player.transform.position;
        targetPos.y = skeleton.position.y;

        skeleton.position = Vector3.MoveTowards(skeleton.position, targetPos, Time.deltaTime * speed);

        if (Physics.Raycast(skeleton.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f))
        {
            Vector3 pos = skeleton.position;
            pos.y = hit.point.y;
            skeleton.position = pos;
        }

        if (!IsInvoking(nameof(PlayFootstep)))
        {
            Invoke(nameof(PlayFootstep), footstepsSpeed);
        }

        if (Vector3.Distance(skeleton.position, targetPos) <= 1f) Destroy(gameObject);
    }

    void PlayFootstep()
    {
        footstepsSource.clip = footsteps[Random.Range(0, footsteps.Length)];
        footstepsSource.Play();
    }
}
