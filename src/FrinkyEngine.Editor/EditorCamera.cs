using System.Numerics;
using Raylib_cs;

namespace FrinkyEngine.Editor;

public class EditorCamera
{
    private Vector3 _position = new(5f, 5f, 5f);
    private float _yaw = -135f;
    private float _pitch = -30f;
    private float _moveSpeed = 10f;
    private float _lookSensitivity = 0.15f;
    private float _scrollSpeed = 2f;
    private bool _cursorDisabled;
    private Vector2 _savedCursorPos;

    public Camera3D Camera3D { get; private set; }

    public EditorCamera()
    {
        UpdateCamera3D();
    }

    public void Update(float dt, bool isViewportHovered)
    {
        bool rightMouse = Raylib.IsMouseButtonDown(MouseButton.Right);

        // Start capture only when hovering viewport, but keep going until right-click is released
        if (rightMouse && (isViewportHovered || _cursorDisabled))
        {
            if (!_cursorDisabled)
            {
                _savedCursorPos = Raylib.GetMousePosition();
                Raylib.DisableCursor();
                _cursorDisabled = true;
            }

            var delta = Raylib.GetMouseDelta();
            _yaw += delta.X * _lookSensitivity;
            _pitch -= delta.Y * _lookSensitivity;
            _pitch = Math.Clamp(_pitch, -89f, 89f);

            var forward = GetForward();
            var right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
            var up = Vector3.UnitY;

            float speed = _moveSpeed * dt;
            if (Raylib.IsKeyDown(KeyboardKey.LeftShift)) speed *= 2.5f;

            if (Raylib.IsKeyDown(KeyboardKey.W)) _position += forward * speed;
            if (Raylib.IsKeyDown(KeyboardKey.S)) _position -= forward * speed;
            if (Raylib.IsKeyDown(KeyboardKey.D)) _position += right * speed;
            if (Raylib.IsKeyDown(KeyboardKey.A)) _position -= right * speed;
            if (Raylib.IsKeyDown(KeyboardKey.E)) _position += up * speed;
            if (Raylib.IsKeyDown(KeyboardKey.Q)) _position -= up * speed;
        }
        else if (_cursorDisabled)
        {
            Raylib.EnableCursor();
            Raylib.SetMousePosition((int)_savedCursorPos.X, (int)_savedCursorPos.Y);
            _cursorDisabled = false;
        }

        if (isViewportHovered)
        {
            float scroll = Raylib.GetMouseWheelMove();
            if (scroll != 0)
            {
                _position += GetForward() * scroll * _scrollSpeed;
            }
        }

        UpdateCamera3D();
    }

    private Vector3 GetForward()
    {
        float yawRad = _yaw * FrinkyEngine.Core.FrinkyMath.Deg2Rad;
        float pitchRad = _pitch * FrinkyEngine.Core.FrinkyMath.Deg2Rad;

        return Vector3.Normalize(new Vector3(
            MathF.Cos(pitchRad) * MathF.Cos(yawRad),
            MathF.Sin(pitchRad),
            MathF.Cos(pitchRad) * MathF.Sin(yawRad)));
    }

    private void UpdateCamera3D()
    {
        var forward = GetForward();
        Camera3D = new Camera3D
        {
            Position = _position,
            Target = _position + forward,
            Up = Vector3.UnitY,
            FovY = 60f,
            Projection = CameraProjection.Perspective
        };
    }
}
