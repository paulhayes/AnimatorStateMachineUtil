/**
The MIT License (MIT)

Copyright (c) 2014 Paul Hayes

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 **/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AnimatorStateMachineLibrary
{
    public class AnimatorStateMachineUtil : MonoBehaviour
    {
        private static Dictionary<Animator, List<AnimatorStateMachineUtil>> fsmUtilsByAnimator = new Dictionary<Animator, List<AnimatorStateMachineUtil>>();
        public static AnimatorStateMachineUtil GetFSMUtilFromAnimator(Animator _animator) {
            List<AnimatorStateMachineUtil> utils;
            fsmUtilsByAnimator.TryGetValue(_animator, out utils);
            return (utils != null && utils.Count > 0) ? utils[0] : null;
        }

        public static IList<AnimatorStateMachineUtil> GetFSMUtilsFromAnimator(Animator _animator) {
            List<AnimatorStateMachineUtil> utils;
            fsmUtilsByAnimator.TryGetValue(_animator, out utils);
            return utils;
        }

        public bool autoUpdate = false;

        [SerializeField] private Animator animator;
        public Animator Animator {
            get {
                if (_animator == null) {
                    if (animator == null) {
                        _animator = GetComponent<Animator>();
                    }
                    else {
                        _animator = animator;
                    }
                }
                return _animator;
            }
        }

        public bool verbose;

        private Animator _animator;
        private Dictionary<int, List<Action>> stateHashToUpdateMethod = new Dictionary<int, List<Action>>();
        private Dictionary<int, List<Action>> stateHashToEnterMethod = new Dictionary<int, List<Action>>();
        private Dictionary<int, List<Action>> stateHashToExitMethod = new Dictionary<int, List<Action>>();
        private int[] _lastStateLayers;

        void Awake() {
            _lastStateLayers = new int[Animator.layerCount];

            List<AnimatorStateMachineUtil> utils;
            if (!fsmUtilsByAnimator.TryGetValue(Animator, out utils)) {
                utils = new List<AnimatorStateMachineUtil>();
                fsmUtilsByAnimator.Add(Animator, utils);
            }
            utils.Add(this);

            DiscoverStateMethods();
        }

        void OnDestroy() {
            List<AnimatorStateMachineUtil> utils;
            if (fsmUtilsByAnimator.TryGetValue(Animator, out utils)) {
                utils.Remove(this);
            }
        }

        void Update() {
            if (autoUpdate) {
                StateMachineUpdate();
            }
        }

        void OnValidate() {
            DiscoverStateMethods();
        }

        public void CallEnterMethods(int _stateHash) {
            List<Action> actions;
            if (stateHashToEnterMethod.TryGetValue(_stateHash, out actions)) {
                foreach (Action action in actions) {
                    action.Invoke();
                }
            }
        }

        public void CallUpdateMethods(int _stateHash) {
            List<Action> actions;
            if (stateHashToUpdateMethod.TryGetValue(_stateHash, out actions)) {
                foreach (Action action in actions) {
                    action.Invoke();
                }
            }
        }

        public void CallExitMethod(int _stateHash) {
            List<Action> actions;
            if (stateHashToExitMethod.TryGetValue(_stateHash, out actions)) {
                foreach (Action action in actions) {
                    action.Invoke();
                }
            }
        }

        public void StateMachineUpdate() {
            for (int layer = 0; layer < _lastStateLayers.Length; layer++) {
                int _lastState = _lastStateLayers[layer];
                int stateId = Animator.GetCurrentAnimatorStateInfo(layer).fullPathHash;
                if (_lastState != stateId) {
                    if (verbose) {
                        Debug.LogWarningFormat("State changed for layer {0}", layer);
                    }

                    CallExitMethod(_lastState);
                    CallEnterMethods(stateId);
                }

                CallUpdateMethods(stateId);

                _lastStateLayers[layer] = stateId;
            }
        }

        void DiscoverStateMethods() {
            MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();

            stateHashToUpdateMethod.Clear();
            stateHashToEnterMethod.Clear();
            stateHashToExitMethod.Clear();

            foreach (MonoBehaviour component in components) {
                if (component != null) {
                    Type type = component.GetType();
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);

                    foreach (MethodInfo method in methods) {
                        object[] attributes;

                        attributes = method.GetCustomAttributes(typeof(StateUpdateMethodAttribute), true);
                        foreach (StateUpdateMethodAttribute attribute in attributes) {
                            ParameterInfo[] parameters = method.GetParameters();
                            if (parameters.Length == 0) {
                                AddStateMethod(stateHashToUpdateMethod, attribute.state, method, component);
                            }
                        }

                        attributes = method.GetCustomAttributes(typeof(StateEnterMethodAttribute), true);
                        foreach (StateEnterMethodAttribute attribute in attributes) {
                            ParameterInfo[] parameters = method.GetParameters();
                            if (parameters.Length == 0) {
                                AddStateMethod(stateHashToEnterMethod, attribute.state, method, component);
                            }
                        }

                        attributes = method.GetCustomAttributes(typeof(StateExitMethodAttribute), true);
                        foreach (StateExitMethodAttribute attribute in attributes) {
                            ParameterInfo[] parameters = method.GetParameters();
                            if (parameters.Length == 0) {
                                AddStateMethod(stateHashToExitMethod, attribute.state, method, component);
                            }
                        }
                    }
                }
            }
        }

        private void AddStateMethod(Dictionary<int, List<Action>> _dictionnary, string _state, MethodInfo _method, MonoBehaviour _component) {
            int stateHash = Animator.StringToHash(_state);

            List<Action> actions = null;
            if (!_dictionnary.TryGetValue(stateHash, out actions)) {
                actions = new List<Action>();
                _dictionnary[stateHash] = actions;
            }

            actions.Add(() => {
                _method.Invoke(_component, null);
            });
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class StateUpdateMethodAttribute : Attribute
    {
        public string state;

        public StateUpdateMethodAttribute(string state) {
            this.state = state;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class StateEnterMethodAttribute : Attribute
    {
        public string state;

        public StateEnterMethodAttribute(string state) {
            this.state = state;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class StateExitMethodAttribute : Attribute
    {
        public string state;

        public StateExitMethodAttribute(string state) {
            this.state = state;
        }
    }
}
