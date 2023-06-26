using System.Collections.Generic;
using UnityEngine;

namespace FX
{
    public class FXGroupController : MonoBehaviour
    {
        // Store a list of addresses
        public List<string> fxAddresses = new List<string>();

        [FXProperty]
        [SerializeField]
        public float FloatValue{ set { SetValue(value); }}

        //public float FloatProperty { get; set; } = 0;


        // Set a value to the given FX
        public void SetValue(float value)
        {
            foreach (string address in fxAddresses)
            {
                FXManager.Instance.SetFX(address, value);
            }
        }
    }
}
