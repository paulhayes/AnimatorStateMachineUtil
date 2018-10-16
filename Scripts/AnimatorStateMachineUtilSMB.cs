using UnityEngine;
using System.Collections.Generic;

namespace AnimatorStateMachineLibrary
{
    public class AnimatorStateMachineUtilSMB : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex) {
            IList<AnimatorStateMachineUtil> utils = AnimatorStateMachineUtil.GetFSMUtilsFromAnimator(_animator);
            foreach (AnimatorStateMachineUtil util in utils) {
                util.CallEnterMethods(_stateInfo.fullPathHash);
            }
        }

        public override void OnStateExit(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex) {
            IList<AnimatorStateMachineUtil> utils = AnimatorStateMachineUtil.GetFSMUtilsFromAnimator(_animator);
            foreach (AnimatorStateMachineUtil util in utils) {
                util.CallExitMethod(_stateInfo.fullPathHash);
            }
        }

        public override void OnStateUpdate(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex) {
            IList<AnimatorStateMachineUtil> utils = AnimatorStateMachineUtil.GetFSMUtilsFromAnimator(_animator);
            foreach (AnimatorStateMachineUtil util in utils) {
                util.CallUpdateMethods(_stateInfo.fullPathHash);
            }
        }
    }
}