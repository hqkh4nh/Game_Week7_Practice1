using UnityEngine;
using UnityEngine.InputSystem;

// Bat dau nhay khi Play, nhan Space de chuyen qua lai Idle <-> Swing Dancing
[RequireComponent(typeof(Animator))]
public class DanceToggle : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("Dance", true);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            animator.SetBool("Dance", !animator.GetBool("Dance"));
    }
}
