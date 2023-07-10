using LiteralRepository;
using UnityEngine;

public class JumpSubState : StateMachineBehaviour
{
    private PlayerInputController _playerInputController;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _playerInputController = animator.GetComponentInParent<PlayerInputController>();
    }
    
    // JumpState동안 Dive 키를 누르면 Dive로 전이할 수 있게 한다.
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_playerInputController.IsDive)
        {
            animator.SetBool(AnimLiteral.IsDiving, true);
        }
    }
}
