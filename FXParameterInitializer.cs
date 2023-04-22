using UnityEngine;
using System.Reflection;

public class FXParameterInitializer : MonoBehaviour
{
    private void Start()
    {
        RegisterFXParametersInScene();
    }

    private void RegisterFXParametersInScene()
    {
        foreach (var component in FindObjectsOfType<MonoBehaviour>())
        {
            AddParameters(component);
        }
    }

    private void AddParameters(MonoBehaviour component)
    {
        var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(FXParameter<>))
            {
                //dynamic parameter = field.GetValue(component);
                //if (parameter != null)
                //{
                //    string address = parameter.Address;
                //    FXManager.Instance.AddFXParameter(address, field);
                //}
            }
        }
    }
}
