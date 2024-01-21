using UnityEngine;
using UnityEditor;
using System.Reflection;
using FX;

[CustomEditor(typeof(MonoBehaviour), true)]
public class FXMethodEditor : Editor
{
    private bool? hasFXMethod = null;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!hasFXMethod.HasValue) 
        {
            hasFXMethod = HasFXMethod(target as MonoBehaviour);
        }

        if (hasFXMethod.Value) // Use cached value
        {
            if (GUILayout.Button("Trigger FX"))
            {
                MethodInfo methodInfo = target.GetType().GetMethod("FXTrigger");
                
                methodInfo.Invoke(target, null);
            }
        }
    }

    private bool HasFXMethod(MonoBehaviour monoBehaviour)
    {
        MethodInfo methodInfo = monoBehaviour.GetType().GetMethod("FXTrigger");
        if (methodInfo != null)
        {
            return methodInfo.GetCustomAttribute<FXMethodAttribute>() != null;
        }
        return false;
    }

    public override bool RequiresConstantRepaint()
    {
        hasFXMethod = null;
        return false;
    }
}
