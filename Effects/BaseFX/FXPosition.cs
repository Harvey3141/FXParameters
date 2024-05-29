using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace FX 
{
    public enum TriggerPositionPattern
    {
        SEQUENTIAL,
        RANDOM
    }

    public class FXPosition : FXBase, IFXTriggerable
    {       
        
        public Vector3[] positions;

        public FXParameter<TriggerPositionPattern> triggerPattern = new FXParameter<TriggerPositionPattern>(TriggerPositionPattern.SEQUENTIAL);

        private int currentIndex = 0;

        public GameObject lookAt;

        [FXMethod]
        public void FXTrigger() {
            currentIndex = GetTriggerIndex();
            gameObject.transform.position = positions[currentIndex];
            if (lookAt != null)
            {
                transform.LookAt(lookAt.transform);
            }
        }

        public int GetTriggerIndex () {
            switch (triggerPattern.Value) { 
              case TriggerPositionPattern.SEQUENTIAL:
                    return (currentIndex + 1) % positions.Length;
              case TriggerPositionPattern.RANDOM:
                    return Random.Range(0, positions.Length);
              default: return currentIndex;
            }
            
        }
    }
}


