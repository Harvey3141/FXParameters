using System.Collections.Generic;
using UnityEngine;

namespace FX
{
    public class FXGroupController : MonoBehaviour
    {

        [SerializeField]
        public List<string> fxAddresses;

        public void SetValue(float value)
        {
            foreach (string address in fxAddresses)
            {
                FXManager.Instance.SetFX(address, value);
            }
        }
    }
}
