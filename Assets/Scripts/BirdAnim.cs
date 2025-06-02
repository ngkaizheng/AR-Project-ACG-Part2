using UnityEngine;
using System.Collections;

public enum birdBehaviors
{
    sing,
    preen,
    ruffle,
    peck,
    hopForward,
    hopBackward,
    hopLeft,
    hopRight,
}
public class BirdAnim : MonoBehaviour
{


    public AudioClip song1;
    public AudioClip song2;
    public AudioClip flyAway1;
    public AudioClip flyAway2;

    public bool fleeCrows = true;
    public float birdScale = 1.0f; // Scale factor for bird size

    [Header("Animation Settings")]
    public float idleAgitation = 0.5f; // Base agitation level for idle animations
    public float flySpeed = 10.0f; // Speed for flying away


    Animator anim;
    bool paused = false;
    bool idle = true;
    bool flying = false;
    bool landing = false;
    bool onGround = true;
    bool dead = false;
    // BoxCollider birdCollider;
    // SphereCollider solidCollider;
    float distanceToTarget = 0.0f;
    float agitationLevel = 0.5f;
    float originalAnimSpeed = 1.0f;
    Vector3 originalVelocity = Vector3.zero;
    Coroutine flyCoroutine; // Store the current flight coroutine

    // Animation state and property hashes
    int idleAnimationHash;
    int flyAnimationHash;
    int hopIntHash;
    int flyingBoolHash;
    int peckBoolHash;
    int ruffleBoolHash;
    int preenBoolHash;
    int landingBoolHash;
    int singTriggerHash;
    int flyingDirectionHash;
    int dieTriggerHash;

    void OnEnable()
    {
        // birdCollider = gameObject.GetComponent<BoxCollider>();
        // solidCollider = gameObject.GetComponent<SphereCollider>();
        anim = gameObject.GetComponent<Animator>();

        // if (!birdCollider) Debug.LogError("BoxCollider not found on " + gameObject.name);
        // if (!solidCollider) Debug.LogError("SphereCollider not found on " + gameObject.name);
        if (!anim) Debug.LogError("Animator not found on " + gameObject.name);

        idleAnimationHash = Animator.StringToHash("Base Layer.Idle");
        flyAnimationHash = Animator.StringToHash("Base Layer.fly");
        hopIntHash = Animator.StringToHash("hop");
        flyingBoolHash = Animator.StringToHash("flying");
        peckBoolHash = Animator.StringToHash("peck");
        ruffleBoolHash = Animator.StringToHash("ruffle");
        preenBoolHash = Animator.StringToHash("preen");
        landingBoolHash = Animator.StringToHash("landing");
        singTriggerHash = Animator.StringToHash("sing");
        flyingDirectionHash = Animator.StringToHash("flyingDirectionX");
        dieTriggerHash = Animator.StringToHash("die");
        anim.SetFloat("IdleAgitated", agitationLevel);
    }

    public void FlyToTarget(Vector3 target)
    {
        if (dead || flying || landing || paused) return;

        // Stop existing flight coroutine if running
        if (flyCoroutine != null)
        {
            StopCoroutine(flyCoroutine);
            Debug.Log("Stopped previous FlyToTarget coroutine");
        }

        Debug.Log($"Starting FlyToTarget to {target}");
        flyCoroutine = StartCoroutine(FlyToTargetCoroutine(target));
    }

    private IEnumerator FlyToTargetCoroutine(Vector3 target)
    {
        // Play fly away sound
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource)
        {
            if (Random.value < 0.5f)
            {
                audioSource.PlayOneShot(flyAway1, 0.1f);
            }
            else
            {
                audioSource.PlayOneShot(flyAway2, 0.1f);
            }
        }

        // Set up for flying
        flying = true;
        landing = false;
        onGround = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.drag = 0.5f;
        anim.applyRootMotion = false;
        anim.SetBool(flyingBoolHash, true);
        anim.SetBool(landingBoolHash, false);

        // Wait for fly animation
        while (anim.GetCurrentAnimatorStateInfo(0).fullPathHash != flyAnimationHash)
        {
            yield return null;
        }

        // Fly directly to target
        flySpeed = 10.0f * birdScale; // Consistent speed for flight
        distanceToTarget = Vector3.Distance(transform.position, target);
        Vector3 previousPosition = transform.position; // Store the initial position

        while (distanceToTarget > 0.15f * birdScale)
        {
            if (!paused)
            {
                Vector3 directionToTarget = (target - transform.position).normalized;
                // Directly set rotation to avoid orbiting
                transform.rotation = Quaternion.LookRotation(directionToTarget);
                anim.SetFloat(flyingDirectionHash, FindBankingAngle(transform.forward, directionToTarget));

                // Smoothly interpolate position to avoid blinking
                Vector3 newPosition = Vector3.Lerp(previousPosition, transform.position + directionToTarget * flySpeed * Time.deltaTime, Time.deltaTime * 5.0f);
                rb.MovePosition(newPosition); // Use Rigidbody's MovePosition for smoother physics-based movement

                previousPosition = newPosition; // Update previous position
                distanceToTarget = Vector3.Distance(transform.position, target);

                // Prevent ground/ceiling collision
                RaycastHit hit;
                if (Physics.Raycast(transform.position, -Vector3.up, out hit, 0.15f * birdScale) && rb.velocity.y < 0)
                {
                    if (!hit.collider.isTrigger)
                    {
                        rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
                    }
                }
                if (Physics.Raycast(transform.position, Vector3.up, out hit, 0.15f * birdScale) && rb.velocity.y > 0)
                {
                    if (!hit.collider.isTrigger)
                    {
                        rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
                    }
                }
            }
            yield return null;
        }

        // Landing phase
        flying = false;
        landing = true;
        // if (solidCollider) solidCollider.enabled = false;
        anim.SetBool(flyingBoolHash, false);
        anim.SetBool(landingBoolHash, true);
        rb.velocity = Vector3.zero;
        rb.drag = 0.5f;

        // Move to exact target
        float t = 0.0f;
        Vector3 vel = Vector3.zero;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
        while (distanceToTarget > 0.05f * birdScale)
        {
            if (!paused)
            {
                //                 transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t * 4.0f);
                transform.position = Vector3.SmoothDamp(transform.position, target, ref vel, 0.5f);
                t += Time.deltaTime;
                distanceToTarget = Vector3.Distance(transform.position, target);
                if (t > 2.0f) break; // Failsafe
            }
            yield return null;
        }

        // Finalize landing
        transform.position = target;
        transform.rotation = targetRotation;
        anim.SetBool(landingBoolHash, false);
        anim.SetFloat(flyingDirectionHash, 0);
        landing = false;
        onGround = true;
        idle = true;
        anim.applyRootMotion = true;
        rb.isKinematic = true;
        flyCoroutine = null; // Clear coroutine reference
        Debug.Log("Bird landed and returned to idle");
    }

    float FindBankingAngle(Vector3 birdForward, Vector3 dirToTarget)
    {
        Vector3 cr = Vector3.Cross(birdForward, dirToTarget);
        float ang = Vector3.Dot(cr, Vector3.up);
        return ang;
    }

    void OnGroundBehaviors()
    {
        idle = anim.GetCurrentAnimatorStateInfo(0).fullPathHash == idleAnimationHash;
        if (!GetComponent<Rigidbody>().isKinematic)
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
        if (idle)
        {
            if (Random.value < Time.deltaTime * 0.33f)
            {
                float rand = Random.value;
                if (rand < 0.3f)
                {
                    DisplayBehavior(birdBehaviors.sing);
                }
                else if (rand < 0.5f)
                {
                    DisplayBehavior(birdBehaviors.peck);
                }
                else if (rand < 0.6f)
                {
                    DisplayBehavior(birdBehaviors.preen);
                }
                else if (rand < 0.7f)
                {
                    DisplayBehavior(birdBehaviors.ruffle);
                }
                else if (rand < 0.85f)
                {
                    DisplayBehavior(birdBehaviors.hopForward);
                }
                else if (rand < 0.9f)
                {
                    DisplayBehavior(birdBehaviors.hopLeft);
                }
                else if (rand < 0.95f)
                {
                    DisplayBehavior(birdBehaviors.hopRight);
                }
                else
                {
                    DisplayBehavior(birdBehaviors.hopBackward);
                }
                anim.SetFloat("IdleAgitated", Random.value);
            }
        }
    }

    public void DisplayBehavior(birdBehaviors behavior)
    {
        idle = false;
        switch (behavior)
        {
            case birdBehaviors.sing:
                anim.SetTrigger(singTriggerHash);
                break;
            case birdBehaviors.ruffle:
                anim.SetTrigger(ruffleBoolHash);
                break;
            case birdBehaviors.preen:
                anim.SetTrigger(preenBoolHash);
                break;
            case birdBehaviors.peck:
                anim.SetTrigger(peckBoolHash);
                break;
            case birdBehaviors.hopForward:
                anim.SetInteger(hopIntHash, 1);
                break;
            case birdBehaviors.hopLeft:
                anim.SetInteger(hopIntHash, -2);
                break;
            case birdBehaviors.hopRight:
                anim.SetInteger(hopIntHash, 2);
                break;
            case birdBehaviors.hopBackward:
                anim.SetInteger(hopIntHash, -1);
                break;
        }
    }

    //Animation Events
    void ResetHopInt()
    {
        anim.SetInteger(hopIntHash, 0);
    }

    //Animation Events
    void PlaySong()
    {
        if (!dead)
        {
            if (Random.value < 0.5f)
            {
                GetComponent<AudioSource>().PlayOneShot(song1, 1);
            }
            else
            {
                GetComponent<AudioSource>().PlayOneShot(song2, 1);
            }
        }
    }

    void Update()
    {
        if (onGround && !paused && !dead)
        {
            OnGroundBehaviors();
        }
    }
}