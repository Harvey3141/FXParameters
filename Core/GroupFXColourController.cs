using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FX {

    public class GroupFXColourController : FXBase
    {
        public FXParameter<Color> color = new FXParameter<Color>(Color.white, "", false);
        public List<string> fxAddresses  = new List<string>();          

        protected override void Awake()
        {
            base.Awake();
            color.OnValueChanged += SetGoupColour;
        }

        protected override void Start()
        {
            base.Start();
        }

        void SetGoupColour(Color color) { 
            foreach (var address in fxAddresses)
            {
                string formattedAddress = address.StartsWith("/") ? address : "/" + address;
                FXManager.Instance.SetFX(formattedAddress, color);
            }
        }
    }
}


