using UnityEngine;
using System.Collections.Generic;

namespace FX
{
    public class FXGroupObjectTrigger : FXBaseWithEnabled, IFXTriggerable
    {
        private bool triggerOddEvenState = false;
        private int triggerSequentialIndex = 0;
        private int triggerRandomIndex = 0;
        private int triggerCurrentRow = 0;
        private int triggerCurrentColumn = 0;
        public FXParameter<TriggerPattern> triggerPattern = new FXParameter<TriggerPattern>(TriggerPattern.ALL);
        protected List<int> triggerRandomIndices = new List<int>();
        public GameObject[] controlledObjects;
        public int numRows = 1;
        public int numColumns = 1; 

        [FXMethod]
        public void FXTrigger()
        {
            if (!fxEnabled.Value)
            {
                return;
            }

            triggerOddEvenState    = !triggerOddEvenState;
            triggerSequentialIndex = (triggerSequentialIndex + 1) % controlledObjects.Length;
            GenerateRandomIndices(UnityEngine.Random.Range(0, controlledObjects.Length));
            triggerRandomIndex     = UnityEngine.Random.Range(0, controlledObjects.Length);
            triggerCurrentRow      = (triggerCurrentRow + 1) % numRows;
            triggerCurrentColumn   = (triggerCurrentColumn + 1) % numColumns;

            TriggerSet();
        }

        protected virtual void TriggerSet()
        {
            for (int i = 0; i < controlledObjects.Length; i++)
            {
                if (ShouldApply(i))
                {
                    ApplyEffectToObject(i);
                }
            }
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
                    return (index == triggerSequentialIndex);
                case TriggerPattern.RANDOM_SINGLE:
                    return (triggerRandomIndex == index);
                case TriggerPattern.RANDOM_MULTI:
                    return triggerRandomIndices.Contains(index);
                case TriggerPattern.ROWS:
                    return (index / numColumns) == triggerCurrentRow;
                case TriggerPattern.COLUMNS:
                    return (index % numColumns) == triggerCurrentColumn;
                default:
                    return false;
            }
        }

        protected virtual void GenerateRandomIndices(int count)
        {
            triggerRandomIndices.Clear();
            while (triggerRandomIndices.Count < count)
            {
                int randomIndex = UnityEngine.Random.Range(0, controlledObjects.Length);
                if (!triggerRandomIndices.Contains(randomIndex))
                {
                    triggerRandomIndices.Add(randomIndex);
                }
            }
        }

        protected virtual void ApplyEffectToObject(int index)
        {
            // This method should be overridden with the specific effect logic for each object
            // Example: controlledObjects[index].SetActive(!controlledObjects[index].activeSelf);
        }
    }
}
