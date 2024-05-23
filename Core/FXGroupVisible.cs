using UnityEngine;

namespace FX
{
    public class FXGroupVisible : FXGroupObjectTrigger
    {
        public FXMaterialController materialController;

        protected override void Awake()
        {
            base.Awake();
            if (GetComponent<FXMaterialController>()) materialController = GetComponent<FXMaterialController>();
        }

        protected override void Start()
        {
            base.Start();
            triggerPattern.OnValueChanged += OnTriggerPatternChanged;

            if (GetComponent<FXMaterialController>()) materialController = GetComponent<FXMaterialController>();
        }

        private void OnTriggerPatternChanged (TriggerPattern pattern)
        {
            
            if (!ShouldModifyRendererEnabledState()) return;
            foreach (var obj in controlledObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }

        protected override void ApplyEffectToObject(int index)
        {
            if (!ShouldModifyRendererEnabledState()) return;
            if (controlledObjects[index] != null)
            {
                Renderer renderer = controlledObjects[index].GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = !renderer.enabled;
                }
            }
        }

        protected override void OnFXEnabled(bool state)
        {
            base.OnFXEnabled(state);
            if (!ShouldModifyRendererEnabledState()) return;           
            foreach (var obj in controlledObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
            
        }

        private bool ShouldModifyRendererEnabledState() { 
            return (materialController == null || materialController.fxEnabled.Value); 
        }

    }
}
