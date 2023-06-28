using System.Collections.Generic;
using UnityEngine;

namespace FX
{
    public class FXGroupController : MonoBehaviour
    {
        // Store a list of addresses
        //public List<string> fxAddresses = new List<string>();

        public List<string> fxAddresses;

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
