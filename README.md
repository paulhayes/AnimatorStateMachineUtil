AnimatorStateMachineUtil
========================

Utility class that provides ability to tag methods in Monobehaviours to receive notifications about Animator state changes.

## Usage

Include the namespace at the top of your monobehaviour
```
using AnimatorStateMachineUtil;
```


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
