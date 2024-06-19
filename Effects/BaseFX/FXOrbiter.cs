using UnityEngine;

namespace FX
{
    public class FXOrbiter : FXBaseWithEnabled, IFXTriggerable
    {
        public enum RotationAxisType { X, Y, Z }

        public FXParameter<RotationAxisType> rotationAxis = new FXParameter<RotationAxisType>(RotationAxisType.X);

        public FXScaledParameter<float> orbitSpeed = new FXScaledParameter<float>(0.1f, 0f, 100f);
        public FXScaledParameter<float> orbitRadius = new FXScaledParameter<float>(0.1f, 0f, 10f);

        public Transform target;
        private float orbitAngle = 0;

        protected override void Start()
        {
            base.Start();
        }

        void Update()
        {
            if (!fxEnabled.Value) return;


            if (orbitSpeed.ScaledValue != 0f)
            {
                orbitAngle += orbitSpeed.ScaledValue * Time.deltaTime;
                orbitAngle %= 360;

                float x = target.position.x;
                float y = target.position.y;
                float z = target.position.z;

                switch (rotationAxis.Value)
                {
                    case RotationAxisType.X:
                        y += orbitRadius.ScaledValue * Mathf.Cos(orbitAngle * Mathf.Deg2Rad);
                        z += orbitRadius.ScaledValue * Mathf.Sin(orbitAngle * Mathf.Deg2Rad);
                        break;
                    case RotationAxisType.Y:
                        x += orbitRadius.ScaledValue * Mathf.Cos(orbitAngle * Mathf.Deg2Rad);
                        z += orbitRadius.ScaledValue * Mathf.Sin(orbitAngle * Mathf.Deg2Rad);
                        break;
                    case RotationAxisType.Z:
                        x += orbitRadius.ScaledValue * Mathf.Cos(orbitAngle * Mathf.Deg2Rad);
                        y += orbitRadius.ScaledValue * Mathf.Sin(orbitAngle * Mathf.Deg2Rad);
                        break;
                }

                transform.position = new Vector3(x, y, z);
            }

            transform.LookAt(target);
        }

        [FXMethod]
        public void FXTrigger()
        {
            float randomAngle = Random.Range(0f, 360f);

            float x = target.position.x;
            float y = target.position.y;
            float z = target.position.z;

            switch (rotationAxis.Value)
            {
                case RotationAxisType.X:
                    y += orbitRadius.ScaledValue * Mathf.Cos(randomAngle * Mathf.Deg2Rad);
                    z += orbitRadius.ScaledValue * Mathf.Sin(randomAngle * Mathf.Deg2Rad);
                    break;
                case RotationAxisType.Y:
                    x += orbitRadius.ScaledValue * Mathf.Cos(randomAngle * Mathf.Deg2Rad);
                    z += orbitRadius.ScaledValue * Mathf.Sin(randomAngle * Mathf.Deg2Rad);
                    break;
                case RotationAxisType.Z:
                    x += orbitRadius.ScaledValue * Mathf.Cos(randomAngle * Mathf.Deg2Rad);
                    y += orbitRadius.ScaledValue * Mathf.Sin(randomAngle * Mathf.Deg2Rad);
                    break;
            }

            transform.position = new Vector3(x, y, z);
        }

    }
}
