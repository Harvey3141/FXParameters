using UnityEngine;
using System.Collections.Generic;

public class FXSerialiser
{
    public string SerializeParameters(Dictionary<string, (FXManager.FXItemInfoType type, object item, object fxInstance)> fxItemsByAddress)
    {
        var fxParameters = new List<FXParameterInfo>();

        foreach (var item in fxItemsByAddress)
        {
            if (item.Value.type == FXManager.FXItemInfoType.Parameter)
            {
                var parameter = item.Value.item as IFXParameter;
                if (parameter != null)
                {
                    FXParameterInfo fxParameterInfo = new FXParameterInfo();
                    fxParameterInfo.address = item.Key;
                    fxParameterInfo.value = parameter.ObjectValue;
                    fxParameters.Add(fxParameterInfo);
                }
                else
                {
                    Debug.LogWarning($"Item at address {item.Key} is not an FXParameter.");
                }
            }
        }

        string json = JsonUtility.ToJson(new Wrapper() { fxParameters = fxParameters });
        return json;
    }

    // Add your deserialization methods here...

    [System.Serializable]
    private class Wrapper
    {
        public List<FXParameterInfo> fxParameters;
    }

    [System.Serializable]
    public class FXParameterInfo
    {
        public string address;
        public object value;
    }
}
