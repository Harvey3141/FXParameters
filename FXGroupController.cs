using System.Collections.Generic;
using UnityEngine;

namespace FX
{
    public class FXGroupController : MonoBehaviour
    {

        [SerializeField]
        public List<string> fxAddresses;

        [SerializeField]
        public FXParameter<float> value = new FXParameter<float>(0.0f,"Group1/value");

        public void Start()
        {
            value.OnValueChanged += SetValue;
        }

        public void SetValue(float value)
        {
            foreach (string address in fxAddresses)
            {
                string formattedAddress = address.StartsWith("/") ? address : "/" + address;
                FXManager.Instance.SetFX(formattedAddress, value);
            }
        }

    }
}
