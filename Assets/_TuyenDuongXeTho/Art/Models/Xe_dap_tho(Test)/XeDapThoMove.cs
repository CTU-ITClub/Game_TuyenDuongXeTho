using UnityEngine;

public class XeDapThoMove : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
        {
            TurnLeft();
        }
        else if (Input.GetKey(KeyCode.A))
        {
            TurnLeft();
        }
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
        {
            TurnRight();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            TurnRight();
        }
        else if (Input.GetKey(KeyCode.W))
        {
            Run();
        }
        else
        {
            Idle();
        }
    }

    void ResetAllState()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isTurningLeft", false);
        animator.SetBool("isTurningRight", false);
    }

    public void Idle()
    {
        ResetAllState();
        animator.SetBool("isIdle", true);
    }

    public void Run()
    {
        ResetAllState();
        animator.SetBool("isRunning", true);
    }

    public void TurnLeft()
    {
        ResetAllState();
        animator.SetBool("isTurningLeft", true);
    }

    public void TurnRight()
    {
        ResetAllState();
        animator.SetBool("isTurningRight", true);
    }
}
