using UnityEngine;
using FX;
using UnityEngine.Rendering.HighDefinition; 


public class FXDirectionalLight : FXBaseWithEnabled, IFXTriggerable
{
    private HDAdditionalLightData lightData; 

    public FXScaledParameter<float> intensity = new FXScaledParameter<float>(0.0f, 0.0f, 2.0f);
    public FXParameter<Color> color = new FXParameter<Color>(Color.white);

    public FXParameter<float> rotationX = new FXParameter<float>(0.0f, 0.0f, 360f);
    public FXParameter<float> rotationY = new FXParameter<float>(0.0f, 0.0f, 360f);
    public FXScaledParameter<float> rotationSpeedX = new FXScaledParameter<float>(0.0f,0.0f,100.0f);
    public FXScaledParameter<float> rotationSpeedY = new FXScaledParameter<float>(0.0f, 0.0f, 100.0f);

    private Light lightComp;
    private Quaternion initRot;
    public float clampX = 360f;
    public float clampY = 360f;


    protected override void Awake()
    {
        base.Awake();

        initRot = transform.localRotation;

        intensity.OnScaledValueChanged += SetIntensity;
        color.OnValueChanged += SetLightColour;
        rotationX.OnValueChanged += SetRotationX;
        rotationY.OnValueChanged += SetRotationY;

        if (GetComponent<Light>()) lightComp = GetComponent<Light>();
        else
        {
            lightComp = gameObject.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            

        }
        lightData = gameObject.GetComponent<HDAdditionalLightData>();

    }

    private void Update()
    {

        rotationX.Value += Mathf.Clamp(rotationSpeedX.ScaledValue, 0, 1);
        if (rotationX.Value > clampX) rotationX.Value = 0;
        rotationY.Value += Mathf.Clamp(rotationSpeedY.ScaledValue, 0, 1);
        if (rotationY.Value > clampY) rotationY.Value = 0;

        transform.rotation = initRot * Quaternion.Euler(new Vector3((rotationX.Value - 0.5f), (rotationY.Value - 0.5f), 0.0f));
        
        lightComp.color = color.Value;
    }

    protected override void OnFXEnabled(bool state)
    {
        if (state)
        {
            lightComp.enabled = true;
        }
        else
        {
            lightComp.enabled = false;
        }
    }

    void SetIntensity(float value)
    {
        if (lightData) lightData.intensity = value;
    }

    void SetLightColour(Color colour)
    {
        lightComp.color = colour;
    }

    void SetRotationX(float value)
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.x = value;
        transform.rotation = Quaternion.Euler(currentRotation);
    }

    void SetRotationY(float value)
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.y = value;
        transform.rotation = Quaternion.Euler(currentRotation);
    }

    [FXMethod]
    public void FXTrigger() {
        rotationX.Value = (Random.Range(initRot.eulerAngles.x, initRot.eulerAngles.x + clampX));
        rotationY.Value = (Random.Range(initRot.eulerAngles.y, initRot.eulerAngles.y + clampY));
    }
}
