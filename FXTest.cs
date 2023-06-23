using System.Net;
using System.Reflection;
using UnityEngine;

public class FXTest : MonoBehaviour
{

    public FXParameter<float>   myFloatParameter    = new FXParameter<float>(0.0f);
    public FXParameter<int>     myIntParameter      = new FXParameter<int>(1);
    public FXParameter<bool>    myBoolParameter     = new FXParameter<bool>(false);
    public FXParameter<string>  myStringParameter   = new FXParameter<string>("no");
    public FXParameter<Color>   myColorParameter    = new FXParameter<Color>(Color.black);

    [FXProperty]
    [field: SerializeField] // Find a neater method for exposing FXProperties in the instpector
    public int IntProperty { get; set; } = 0;


    public void SavePreset1(){FXManager.Instance.SavePreset("Preset1");}

    public void LoadPreset1(){FXManager.Instance.LoadPreset("Preset1");}

    public void SavePreset2(){FXManager.Instance.SavePreset("Preset2");}

    public void LoadPreset2(){FXManager.Instance.LoadPreset("Preset2");}

    private void Start()
    {
        this.AddFXElements();

        FXManager.Instance.SetFX(myFloatParameter.Address, 0.6f);
        FXManager.Instance.SetFX(myIntParameter.Address, 5);
        FXManager.Instance.SetFX(myBoolParameter.Address, true);
        FXManager.Instance.SetFX(myStringParameter.Address, "yes");
        FXManager.Instance.SetFX(myColorParameter.Address, Color.white);
        
        string address = "/Example/FXTest/MyTestIntMethod";
        //object[] args = new object[] { 5, 10 }; // Replace with your method arguments
        FXManager.Instance.SetFX(address, 3);

        FXManager.Instance.SetFX("/Example/FXTest/IntProperty", 99);
    }

    [FXMethod]
    public void MyTestIntMethod(int i) {
        Debug.Log("MyTestIntMethod - value: " + i);
    }

}
