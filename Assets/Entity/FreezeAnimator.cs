using UnityEngine;
using Catacumba.Entity;

public class FreezeAnimator : MonoBehaviour
{
    public float AnimationFreezeFrameTime = 0.15f;

    Animator animator;
    CharacterAnimator characterAnimator;

    // Animator speed reset timer
    float timeSpeedReset;
    float defaultSpeed;

    // Start is called before the first frame update
    void Start()
    {
        characterAnimator = GetComponent<CharacterAnimator>();
        if (!characterAnimator)
        {
            animator = GetComponent<Animator>();
        }
        else
        {
            animator = characterAnimator.animator;
        }
        defaultSpeed = animator.speed;
    }

    private void OnEnable()
    {
        if (!characterAnimator)
        {
            return;
        }

        characterAnimator.OnRefreshAnimator += OnRefreshAnimator;
    }

    private void OnDisable()
    {
        if (!characterAnimator)
        {
            return;
        }

        characterAnimator.OnRefreshAnimator -= OnRefreshAnimator;
    }

    private void OnRefreshAnimator(Animator anim)
    {
        animator = anim;
    }

    void Update()
    {
        if (animator == null) {
            CheckAnimator();
            return;
        }

        if (animator.speed < 1f && Time.time > timeSpeedReset + AnimationFreezeFrameTime)
        {
            animator.speed = defaultSpeed;
        }
    }

    void CheckAnimator()
    {
        if (!characterAnimator)
        {
            return;
        }
        else
        {
            animator = characterAnimator.animator;
        }
    }

    public void Freeze()
    {
        timeSpeedReset = Time.time;
        animator.speed = 0f;
    }
}
