using System.Xml.Schema;
using UnityEngine;

namespace FX
{
    public class FXExtrudingWall : FXGroupObjectTrigger
    {
        public FXScaledParameter<float> animationSpeed = new FXScaledParameter<float>(0.5f, 0.1f, 1.0f);
        public FXParameter<bool> emissiveLightsEnabled = new FXParameter<bool>(false);
        public FXParameter<Color> color = new FXParameter<Color>(Color.white);


        public Animation[] animations;
        public Light[] lights;

        public int maxNumRandomTriggers = 10;

        public Material defaultMaterial;
        public Material emissiveMaterial;

        protected override void Start()
        {
            animations = new Animation[controlledObjects.Length];
            lights = new Light[controlledObjects.Length];

            for (int i = 0; i < controlledObjects.Length; i++) {
                animations[i] = controlledObjects[i].GetComponent<Animation>();
                lights[i] = animations[i].GetComponent<Light>();
            }

            base.Start();
            triggerPattern.OnValueChanged       += OnTriggerPatternChanged;
            animationSpeed.OnScaledValueChanged += OnAnimationSpeedChanged;
            emissiveLightsEnabled.OnValueChanged += SetEmissiveLightsEnabled;

        }

        private void Update()
        {
            if (emissiveLightsEnabled.Value) {
                for (int i = 0; i < animations.Length; i++)
                {
                    float time = GetAnimationNormalisedTime(animations[i], "extrude");
                    lights[i].intensity = time;
                    lights[i].color = color.Value;
                    animations[i].gameObject.GetComponent<Renderer>().material.SetColor("_EmissiveColor", color.Value * Mathf.GammaToLinearSpace(time * 2.0f));
                }
            }

        }

        protected override void GenerateRandomIndices(int count)
        {
            count = Mathf.Min(count, maxNumRandomTriggers);
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

        private void OnTriggerPatternChanged(TriggerPattern pattern)
        {
            foreach (var a in animations)
            {
                a.Rewind();               
            }
        }

        protected override void ApplyEffectToObject(int index)
        {
            if (animations[index] != null)
            {
                animations[index].Play();              
            }
        }

        protected override void OnFXEnabled(bool state)
        {
            base.OnFXEnabled(state);
            foreach (var obj in controlledObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = state;
                }
            }
        }

        private void OnAnimationSpeedChanged(float value) {
            for (int i = 0; i < animations.Length; i++)
            {
                animations[i]["extrude"].speed = value;
            }
        }

        float GetAnimationNormalisedTime(Animation animation, string clipName)
        {
            if (animation == null) return 0;

            AnimationState state = animation[clipName];
            if (state != null)
            {
                return state.time / state.length;
            }

            return 0;
        }

        public void SetEmissiveLightsEnabled(bool value)
        {
            for (int i = 0; i < controlledObjects.Length; i++)
            {
                controlledObjects[i].GetComponent<Renderer>().material = value ? emissiveMaterial : defaultMaterial;
            }
        }
    }
}
