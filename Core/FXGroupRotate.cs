using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FX
{
    public class FXGroupRotate : FXGroupObjectTrigger
    {
        public enum RotationDirection
        {
            Clockwise,
            Anticlockwise,
            Random
        }

        public enum RotationAxis
        {
            X,
            Y,
            Z
        }

        public FXParameter<RotationDirection> rotationDirection = new FXParameter<RotationDirection>(RotationDirection.Clockwise);
        public FXParameter<RotationAxis> rotationAxis = new FXParameter<RotationAxis>(RotationAxis.Y);
        public FXScaledParameter<float> rotationDuration = new FXScaledParameter<float>(0.5f, 0.1f, 1.0f);

        private Dictionary<GameObject, Quaternion> initialRotations = new Dictionary<GameObject, Quaternion>();
        private Dictionary<GameObject, Coroutine> rotationCoroutines = new Dictionary<GameObject, Coroutine>();

        int directionMultiplier = -1;

        protected override void Start()
        {
            base.Start();
            foreach (var obj in controlledObjects)
            {
                if (obj != null)
                {
                    initialRotations[obj] = obj.transform.localRotation;
                }
            }
        }

        public void ResetRotations()
        {
            foreach (var obj in controlledObjects)
            {
                if (obj != null && initialRotations.ContainsKey(obj))
                {
                    obj.transform.localRotation = initialRotations[obj];
                }
            }
        }

        protected override void OnFXEnabled(bool state)
        {
            ResetRotations();
        }

        protected override void ApplyEffectToObject(int index)
        {
            GameObject obj = controlledObjects[index];
            if (obj != null)
            {
                if (!rotationCoroutines.ContainsKey(obj) || rotationCoroutines[obj] == null)
                {
                    rotationCoroutines[obj] = StartCoroutine(RotateObjectLocal(obj.transform, rotationDuration.Value, directionMultiplier));
                }
            }
        }

        protected override void TriggerSet()
        {
            directionMultiplier = DetermineRotationMultiplier();
            base.TriggerSet();
        }

        private int DetermineRotationMultiplier()
        {
            switch (rotationDirection.Value)
            {
                case RotationDirection.Clockwise:
                    return -1;
                case RotationDirection.Anticlockwise:
                    return 1;
                case RotationDirection.Random:
                    return Random.Range(0, 2) * 2 - 1;
                default:
                    return 1;
            }
        }

        private IEnumerator RotateObjectLocal(Transform objectTransform, float duration, int directionMultiplier)
        {
            Quaternion initialRotation = objectTransform.localRotation;
            Vector3 rotationAxisVector = DetermineRotationAxis();
            Quaternion finalRotation = initialRotation * Quaternion.Euler(rotationAxisVector * 90 * directionMultiplier);

            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                objectTransform.localRotation = Quaternion.Lerp(initialRotation, finalRotation, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            objectTransform.localRotation = finalRotation;
            rotationCoroutines[objectTransform.gameObject] = null;
        }

        private Vector3 DetermineRotationAxis()
        {
            switch (rotationAxis.Value)
            {
                case RotationAxis.X:
                    return Vector3.right;
                case RotationAxis.Y:
                    return Vector3.up;
                case RotationAxis.Z:
                    return Vector3.forward;
                default:
                    return Vector3.up;
            }
        }
    }
}
