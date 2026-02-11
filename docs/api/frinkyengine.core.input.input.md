# Input

Namespace: FrinkyEngine.Core.Input

Static polling API for keyboard and mouse input. All methods reflect the state for the current frame.

```csharp
public static class Input
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [Input](./frinkyengine.core.input.input)

## Properties

### **MousePosition**

Gets the current mouse cursor position in screen coordinates.

```csharp
public static Vector2 MousePosition { get; }
```

#### Property Value

[Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>

### **MouseDelta**

Gets the mouse movement delta since the last frame.

```csharp
public static Vector2 MouseDelta { get; }
```

#### Property Value

[Vector2](https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector2)<br>

### **MouseWheelMove**

Gets the mouse wheel vertical scroll amount for the current frame.

```csharp
public static float MouseWheelMove { get; }
```

#### Property Value

[Single](https://docs.microsoft.com/en-us/dotnet/api/system.single)<br>

## Methods

### **IsKeyDown(KeyboardKey)**

Checks whether a keyboard key is currently held down.

```csharp
public static bool IsKeyDown(KeyboardKey key)
```

#### Parameters

`key` KeyboardKey<br>
The key to check.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the key is held down this frame.

### **IsKeyPressed(KeyboardKey)**

Checks whether a keyboard key was pressed this frame (transition from up to down).

```csharp
public static bool IsKeyPressed(KeyboardKey key)
```

#### Parameters

`key` KeyboardKey<br>
The key to check.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the key was just pressed.

### **IsKeyReleased(KeyboardKey)**

Checks whether a keyboard key was released this frame (transition from down to up).

```csharp
public static bool IsKeyReleased(KeyboardKey key)
```

#### Parameters

`key` KeyboardKey<br>
The key to check.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the key was just released.

### **IsKeyUp(KeyboardKey)**

Checks whether a keyboard key is currently not pressed.

```csharp
public static bool IsKeyUp(KeyboardKey key)
```

#### Parameters

`key` KeyboardKey<br>
The key to check.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the key is up this frame.

### **IsMouseButtonDown(MouseButton)**

Checks whether a mouse button is currently held down.

```csharp
public static bool IsMouseButtonDown(MouseButton button)
```

#### Parameters

`button` MouseButton<br>
The mouse button to check.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the button is held down this frame.

### **IsMouseButtonPressed(MouseButton)**

Checks whether a mouse button was pressed this frame.

```csharp
public static bool IsMouseButtonPressed(MouseButton button)
```

#### Parameters

`button` MouseButton<br>
The mouse button to check.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the button was just pressed.

### **IsMouseButtonReleased(MouseButton)**

Checks whether a mouse button was released this frame.

```csharp
public static bool IsMouseButtonReleased(MouseButton button)
```

#### Parameters

`button` MouseButton<br>
The mouse button to check.

#### Returns

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>
`true` if the button was just released.
