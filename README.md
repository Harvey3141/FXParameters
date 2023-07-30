# FXParameters

FXParameters is a tool that allows developers to expose and manipulate parameters, properties, and methods in Unity. This library also introduces a mechanism to handle events and control the enable state of the FX system.

## 📚 FXExample

Checkoiut the `FXExample` class which demonstrates the use of FXManager in Unity. The class implements the IFXTriggerable interface which provides methods that can be triggered by the FX system. It also maintains a state that can be controlled by the FX system.

### FXScaledParameter

FXScaledParameter instances represent parameters with values scaled between two limits. In the `FXExample` class, these are represented by `colorParam` and `floatParam`.

```csharp
public FXScaledParameter<Color> colorParam = new FXScaledParameter<Color>(0.5f, Color.red, Color.blue);
public FXScaledParameter<float> floatParam = new FXScaledParameter<float>(0.5f, 0.0f, 10.0f);
```

### FXParameter with Event Handling

In the `FXExample` class, an `FXParameter` with event handling is demonstrated with `myFloatParameterWithEvent`. This parameter triggers an event handler, `HandleFloatValueChanged`, whenever its value changes.

```csharp
public FXParameter<float> myFloatParameterWithEvent = new FXParameter<float>(0.0f);

// In the Start() method
myFloatParameterWithEvent.OnValueChanged += HandleFloatValueChanged;

private void HandleFloatValueChanged(float newValue)
{
    // Handle the value change here
    Debug.Log($"Float value changed: {newValue}");
}
```

### FXEnabledParameter

The `FXEnabledParameter` instance, `fxEnabled`, represents the enabled state of the FX system. The `FXOnEnabled` method is registered as an event handler that is triggered whenever the enabled state changes.

```csharp
public FXEnabledParameter fxEnabled = new FXEnabledParameter(true);

// In the Start() method
fxEnabled.OnValueChanged += FXOnEnabled;

public void FXOnEnabled(bool value)
{
    Debug.Log($"Enabled value changed: {value}");
}
```

### FXMethod

`FXMethod` is a custom attribute that marks methods which can be triggered by the FX system. In `FXExample`, the `MyTestIntMethod` and `MyTestStringMethod` are marked with this attribute.

```csharp
[FXMethod]
public void MyTestIntMethod(int i)
{
    Debug.Log("MyTestIntMethod - value: " + i);
}

[FXMethod]
public void MyTestStringMethod(string s)
{
    Debug.Log("MyTestStringMethod - value: " + s);
}
```

### IFXTriggerable

`FXExample` implements the `IFXTriggerable` interface which means it has an `FXTrigger` method that can be triggered by the FX system.

## 🧪 Usage

On start, `FXExample` adds all its FX elements (FXParameters, FXProperties, FXMethods) to the FX system. It then sets the values of the FXParameters and calls an FXMethod using the FX system. Event handlers are registered for the `myFloatParameterWithEvent` and `fxEnabled` instances to handle their value change events.

## 🔐 Preset System
FXManager's preset system enables you to store and retrieve the state of FXParameters. It serialises the data into JSON, providing a reliable and efficient means to save and load presets.

By default, the preset system serialises all FXParameters within your Unity scene. However, you can also opt to manually choose which parameters to include in the preset. This is particularly useful when you have a large number of FXParameters and only need to persist a subset of them.

When saving a preset, the system collects the current values of the specified FXParameters, serialises them into a JSON string, and stores this string. Loading a preset is as simple as retrieving the JSON string, deserialising it, and applying the values to the corresponding FXParameters.

## 📄 License

FXManager is licensed under the MIT License.
