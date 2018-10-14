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

namespace AnimatorStateMachineUtil
{
    public class FSMUtil : MonoBehaviour
    {
        public bool autoUpdate;

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

        private Animator _animator;
        private Dictionary<int, List<Action>> stateHashToUpdateMethod = new Dictionary<int, List<Action>>();
        private Dictionary<int, List<Action>> stateHashToEnterMethod = new Dictionary<int, List<Action>>();
        private Dictionary<int, List<Action>> stateHashToExitMethod = new Dictionary<int, List<Action>>();
        private int[] _lastStateLayers;

        void Awake() {
            _lastStateLayers = new int[Animator.layerCount];
            DiscoverStateMethods();
        }

        void Update() {
            if (autoUpdate) {
                StateMachineUpdate();
            }
        }

        void OnValidate() {
            DiscoverStateMethods();
        }

        public void StateMachineUpdate() {
            List<Action> actions;

            for (int layer = 0; layer < _lastStateLayers.Length; layer++) {
                int _lastState = _lastStateLayers[layer];
                int stateId = Animator.GetCurrentAnimatorStateInfo(layer).fullPathHash;
                if (_lastState != stateId) {

                    if (stateHashToExitMethod.TryGetValue(_lastState, out actions)) {
                        foreach (Action action in actions) {
                            action.Invoke();
                        }
                    }

                    if (stateHashToEnterMethod.TryGetValue(stateId, out actions)) {
                        foreach (Action action in actions) {
                            action.Invoke();
                        }
                    }
                }

                if (stateHashToUpdateMethod.TryGetValue(stateId, out actions)) {
                    foreach (Action action in actions) {
                        action.Invoke();
                    }
                }

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
