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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AnimatorStateMachineUtil
{
    public class AnimatorStateMachineUtil : MonoBehaviour
    {
        public bool autoUpdate;
        public Animator Animator {
            get {
                return _animator;
            }
        }

        protected Animator _animator;
        protected Lookup<int,Action> stateHashToUpdateMethod;
        protected Lookup<int,Action> stateHashToEnterMethod;
        protected Lookup<int,Action> stateHashToExitMethod;
        protected Dictionary<int,string> hashToAnimString;
        protected int[] _lastStateLayers;

        void Awake()
        {
            _animator = GetComponent<Animator>();
            _lastStateLayers = new int[_animator.layerCount];

            DiscoverStateMethods();
        }
        
        void Update()
        {
            if (autoUpdate)
            {
                StateMachineUpdate();
            }

        }
        
		void OnValidate(){		
			DiscoverStateMethods();
		}

        public void StateMachineUpdate()
        {
            for(int layer=0;layer<_lastStateLayers.Length;layer++){
                int _lastState = _lastStateLayers[layer];
                int stateId = _animator.GetCurrentAnimatorStateInfo(layer).fullPathHash;
                if (_lastState != stateId)
                {

                    if (stateHashToExitMethod.Contains(_lastState))
                    {
                        foreach( Action action in stateHashToExitMethod[_lastState])
                        {
                            action.Invoke();
                        }
                    }

                    if (stateHashToEnterMethod.Contains(stateId))
                    {
                        foreach( Action action in stateHashToEnterMethod[stateId])
                        {
                            action.Invoke();
                        }
                       
                    }
                }
                
                if (stateHashToUpdateMethod.Contains(stateId))
                {
                    foreach( Action action in stateHashToUpdateMethod[stateId])
                    {
                        action.Invoke();
                    }

                }

                _lastStateLayers[layer] = stateId;
            }

        }
        
        void DiscoverStateMethods()
        {
            

            hashToAnimString = new Dictionary<int, string>();
            var components = gameObject.GetComponents<MonoBehaviour>();

            List<StateMethod> enterStateMethods = new List<StateMethod>();
            List<StateMethod> updateStateMethods = new List<StateMethod>();
            List<StateMethod> exitStateMethods = new List<StateMethod>();


            foreach (var component in components)
            {
                if (component == null) continue;
                
	            var type = component.GetType();	
	            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.InvokeMethod);
	
	            foreach (var method in methods)
	            {
	                object[] attributes;
	
	                attributes = method.GetCustomAttributes(typeof (StateUpdateMethod), true);
	                foreach (StateUpdateMethod attribute in attributes)
	                {
	                    var parameters = method.GetParameters();
	                    if (parameters.Length == 0)
	                    {
	                        updateStateMethods.Add(CreateStateMethod(attribute.state, method, component));
	                    }
	                }
	
	
	                attributes = method.GetCustomAttributes(typeof (StateEnterMethod), true);
	                foreach (StateEnterMethod attribute in attributes)
	                {
	
	                    var parameters = method.GetParameters();
	                    if (parameters.Length == 0)
	                    {
	                        enterStateMethods.Add(CreateStateMethod(attribute.state, method, component));
	
	                    }
	                }
	
	                attributes = method.GetCustomAttributes(typeof (StateExitMethod), true);
	                foreach (StateExitMethod attribute in attributes)
	                {
	
	                    var parameters = method.GetParameters();
	                    if (parameters.Length == 0)
	                    {
	                        exitStateMethods.Add(CreateStateMethod(attribute.state, method, component));
	
	                    }
	                }
                    
                }
            }  


            stateHashToUpdateMethod = (Lookup<int,Action>)updateStateMethods.ToLookup<StateMethod,int,Action>( p=>p.stateHash, p=>p.method );
            stateHashToEnterMethod = (Lookup<int,Action>)enterStateMethods.ToLookup<StateMethod,int,Action>( p=>p.stateHash, p=>p.method ) ;
            stateHashToExitMethod = (Lookup<int,Action>)exitStateMethods.ToLookup<StateMethod,int,Action>( p=>p.stateHash, p=>p.method );

        }

        StateMethod CreateStateMethod(string state, MethodInfo method, MonoBehaviour component )
        {
            int stateHash = Animator.StringToHash(state);
            hashToAnimString[stateHash]=state;
            StateMethod stateMethod = new StateMethod();
            stateMethod.stateHash = stateHash;
            stateMethod.method = () => 
            { 
                method.Invoke(component, null);
            };
            return stateMethod;
        }
    }


    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true)]
    public class StateUpdateMethod : System.Attribute
    {
        public string state;
        
        public StateUpdateMethod (string state)
        {
            this.state = state;
        }
    }

    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true)]
    public class StateEnterMethod : System.Attribute
    {
        public string state;
        
        public StateEnterMethod (string state)
        {
            this.state = state;
        }
    }
    
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = true)]
    public class StateExitMethod : System.Attribute
    {
        public string state;
        
        public StateExitMethod (string state)
        {
            this.state = state;
        }
    }

    public class StateMethod
    {
        public int stateHash;
        public Action method;
    }
}

