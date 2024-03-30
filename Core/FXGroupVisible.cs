using UnityEngine;

namespace FX
{
    public class FXGroupVisible : FXGroupObjectTrigger
    {

        protected override void Start()
        {
            base.Start();
            triggerPattern.OnValueChanged += OnTriggerPatternChanged;
        }

        private void OnTriggerPatternChanged (TriggerPattern pattern)
        {
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
            foreach (var obj in controlledObjects) {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }


    }
}
