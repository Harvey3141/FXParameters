# FXManager - Unity Integration Guide

FXManager is a tool that allows developers to expose and manipulate parameters, properties, and methods in Unity. This library also introduces a mechanism to handle events and control the enable state of the FX system.

## 📚 FXExample: A Comprehensive Usage Example

Checkoiut the `FXExample` class which demonstrates the use of FXManager with MonoBehaviour in Unity. The class implements the IFXTriggerable interface which provides methods that can be triggered by the FX system. It also maintains a state that can be controlled by the FX system.

### FXParameters

In `FXExample`, `FXParameter` instances expose parameters to the FX system. These parameters can be manipulated through the FX system and their current values can be stored and retrieved via the FX system. Parameters are defined for different data types such as `float`, `int`, `bool`, `string`, and `Color`.

### FXScaledParameter

`FXScaledParameter` instances represent parameters with values scaled between two limits. In `FXExample`, these are represented by `colorParam` and `floatParam`.

### FXParameter with Event Handling

In `FXExample`, an `FXParameter` with event handling is demonstrated with `myFloatParameterWithEvent`. This parameter triggers an event handler, `HandleFloatValueChanged`, whenever its value changes.

### FXEnabledParameter

The `FXEnabledParameter` instance, `fxEnabled`, represents the enabled state of the FX system. The `FXOnEnabled` method is registered as an event handler that is triggered whenever the enabled state changes.

### FXMethod

`FXMethod` is a custom attribute that marks methods which can be triggered by the FX system. In `FXExample`, the `MyTestIntMethod` and `MyTestStringMethod` are marked with this attribute.

### IFXTriggerable

`FXExample` implements the `IFXTriggerable` interface which means it has an `FXTrigger` method that can be triggered by the FX system.

## 🧪 Usage

On start, `FXExample` adds all its FX elements (FXParameters, FXProperties, FXMethods) to the FX system. It then sets the values of the FXParameters and calls an FXMethod using the FX system. Event handlers are registered for the `myFloatParameterWithEvent` and `fxEnabled` instances to handle their value change events.

## 🤝 Support and Contributions

If you have any issues or if you'd like to contribute to the project, feel free to open a pull request or issue on the project's GitHub page.

## 📄 License

FXManager is licensed under the MIT License.
