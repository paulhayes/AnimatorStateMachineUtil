AnimatorStateMachineUtil
========================

Utility class that provides ability to tag methods in Monobehaviours to receive notifications about Animator state changes.

## Usage

Any monobehaviour you want to receive state notifiactions of an Animator must be present on the same GameObject as the Animator. Add the AnimatorStateMachineUtil component to the Animator's GameObject, if you want your methods to receive the Update tag without you explicitly calling the `AnimatorStateMachineUtil.StateMachineUpdate()` method, check the autoUpdate option on the component inspector.

Include the namespace at the top of your monobehaviour
```
using AnimatorStateMachineUtil;
```

Then label any methods you want to receive notification. There are three State Attributes supported, StateEnterMethod, StateUpdateMethod and StateExitMethod. The name of your method is not important, but the method must be public. You can Add these attributes to as many Monobehaviours as you want on the Animator's GameObject.
```	
	[StateEnterMethod("Layer.StateName")]
	public void EnteredStateName()
	{
		Debug.Log("Called when Animator enters state StateName");
	}
	
	[StateUpdateMethod("Layer.StateName")]
	public void UpdateOnStateName()
	{
		Debug.Log("Called every frame the Animator is in state StateName");
		
	}

	[StateExitMethod("Layer.StateName")]
	public void ExitStateName()
	{
		Debug.Log("Called when Animator leaves state StateName");
		
	}

```

Multiple state attrbiutes are supported on individual methods. Mutliple methods can be called on any state, like so:

```	

	[StateEnterMethod("Layer.FirstState")]
	[StateEnterMethod("Layer.AnotherState")]
	public void AMethod()
	{
		Debug.Log("Called when Animator enters FirstState or AnotherState");
	}
	
	[StateEnterMethod("Layer.AnotherState")]
	public void AnotherMethod()
	{
		Debug.Log("Called when Animator enters AnotherState");
		
	}

```
