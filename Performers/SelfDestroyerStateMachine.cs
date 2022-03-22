using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroyerStateMachine : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Instantiator.RemovePerformer(animator.gameObject.name.Replace("[dead]", string.Empty));
        Destroy(animator.gameObject);
    }
}
