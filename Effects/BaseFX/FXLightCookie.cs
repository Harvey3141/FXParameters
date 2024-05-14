using FX;
using UnityEngine;

public class FXLightCookie : FXBaseWithEnabled, IFXTriggerable
{
    public Light targetLight; 
    public Texture[] cookies;

    public FXParameter<int> cookieIndex = new FXParameter<int>(0, 0, 1);

    protected override void Awake()
    {
        base.Awake();
        cookieIndex.OnValueChanged += OnCookieIndexChanged;
    }

    protected override void Start()
    {
        base.Start();
        if (targetLight == null) targetLight = GetComponent<Light>();   
        LoadCookies();
        cookieIndex.SetMaxValue(cookies.Length-1);
    }

    void LoadCookies()
    {
        cookies = Resources.LoadAll<Texture>("Cookies"); 
        if (cookies.Length == 0)
        {
            Debug.LogError("No cookies found");
        }
    }

    void OnCookieIndexChanged(int value) {
        targetLight.cookie = cookies[value];
    }

    void ApplyRandomCookie()
    {
        if (cookies.Length > 0 && targetLight != null)
        {
            int index = Random.Range(0, cookies.Length);
            targetLight.cookie = cookies[index]; 
        }
    }

    [FXMethod]
    public void FXTrigger() {
        if (!fxEnabled.Value) return;
        ApplyRandomCookie();
    }

    protected override void OnFXEnabled(bool state)
    {
        base.OnFXEnabled(state);
        if (state) 
        {
            targetLight.cookie = cookies[cookieIndex.Value];
        }
        else
        {
            targetLight.cookie = null;
        }
    }
}
