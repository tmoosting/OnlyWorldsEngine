using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

public class Utilities : MonoBehaviour
{
 
    
    public static void ExecuteWithDelay(Action action)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(DelayRoutine(action));
    }

    private static IEnumerator DelayRoutine(Action action)
    {
        yield return new EditorWaitForSeconds(0.02f);
        action.Invoke();
    }
}
