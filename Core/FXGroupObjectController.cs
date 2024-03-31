using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FX
{
    public enum TriggerPattern
    {
        ALL,
        ODD_EVEN,
        SEQUENTIAL,
        RANDOM_SINGLE,
        RANDOM_MULTI,
        ROWS,
        COLUMNS
    }

    public abstract class FXGroupObjectController : FXBaseWithEnabled, IFXTriggerable
    {
        private bool isTriggerCoroutineActive = false;
        private bool triggerOddEvenState = false;
        private int triggerSequencialIndex = 0;
        private int triggerRandomIndex = 0;
        public FXParameter<TriggerPattern> triggerPattern = new FXParameter<TriggerPattern>(TriggerPattern.ALL);
        private List<int> triggerRandomIndices = new List<int>();
        public FXScaledParameter<float> triggerDuration = new FXScaledParameter<float>(0.05f, 0.0f, 1.0f);
        public FXScaledParameter<float> triggerValue = new FXScaledParameter<float>(0.0f, 0.0f, 1.0f);

        //protected float triggerStartValue = 0.0f;
        //protected float triggerEndValue = 0.0f;
        public GameObject[] controlledObjects;


        [FXMethod]
        public virtual void FXTrigger() {
            if (!isTriggerCoroutineActive)
            {
                triggerOddEvenState = !triggerOddEvenState;
                triggerSequencialIndex = (triggerSequencialIndex + 1) % controlledObjects.Length;
                GenerateRandomIndices(UnityEngine.Random.Range(0, controlledObjects.Length));
                triggerRandomIndex = UnityEngine.Random.Range(0, controlledObjects.Length);
                StartCoroutine(LerpTriggerValue());
            }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        private IEnumerator LerpTriggerValue()
        {
            isTriggerCoroutineActive = true;
            float halfDuration = triggerDuration.ScaledValue / 2.0f;
            float timer = 0.0f;

            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float intensity = Mathf.Lerp(triggerValue.ScaledValue, triggerValue.ValueAtOne, timer / halfDuration);
                SetLerpValue(intensity);
                yield return null;
            }

            timer = 0.0f;
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float intensity = Mathf.Lerp(triggerValue.ValueAtOne, triggerValue.ScaledValue, timer / halfDuration);
                SetLerpValue(intensity);
                yield return null;
            }

            isTriggerCoroutineActive = false;
        }

        private void SetLerpValue(float value)
        {
            for (int i = 0; i < controlledObjects.Length; i++)
            {
                if (ShouldApply(i))
                {
                    SetLerpValueToObject(i, value); 
                }
            }
        }

        protected virtual void SetLerpValueToObject(int index, float value) { 
        
        }


        private bool ShouldApply(int index)
        {
            switch (triggerPattern.Value)
            {
                case TriggerPattern.ALL:
                    return true;
                case TriggerPattern.ODD_EVEN:
                    return (index % 2 == 0) ? triggerOddEvenState : !triggerOddEvenState;
                case TriggerPattern.SEQUENTIAL:
                    return (index == triggerSequencialIndex);
                case TriggerPattern.RANDOM_SINGLE:
                    return (triggerRandomIndex == index);
                case TriggerPattern.RANDOM_MULTI:
                    return triggerRandomIndices.Contains(index);
            }
            return false;
        }

        private void GenerateRandomIndices(int count)
        {
            triggerRandomIndices.Clear();
            for (int i = 0; i < count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, controlledObjects.Length);
                if (!triggerRandomIndices.Contains(randomIndex))
                {
                    triggerRandomIndices.Add(randomIndex);
                }
            }
        }
    }

}

