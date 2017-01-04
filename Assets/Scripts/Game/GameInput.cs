using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public static class GameInput
{
    public static Vector2 sensitivity = new Vector2(0.05f, 0.05f);
    public static Vector2 smoothing = new Vector2(3f, 3f);
    private static Vector3 mouseScale = new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y);
    private static Vector2 _movement;
    private static Vector2 _camera;
    private static Vector2 _rawMouse;
    private static Vector2 _smoothMouse;

    public static bool Jumping
    {
        get { return CrossPlatformInputManager.GetButton("Jump"); }
    }

    public static bool Boosting
    {
        get { return CrossPlatformInputManager.GetButton("Boost"); }
    }

    public static bool CreateBlock
    {
        get { return CrossPlatformInputManager.GetButton("Create"); }
    }

    public static bool GroundPound
    {
        get { return CrossPlatformInputManager.GetButton("Pound"); }
    }

    public static bool SwapInputs
    {
        get { return CrossPlatformInputManager.GetButtonDown("Swap"); }
    }

    public static bool Pause
    {
        get { return CrossPlatformInputManager.GetButtonDown("Pause"); }
    }

    public static bool View
    {
        get { return CrossPlatformInputManager.GetButtonDown("View"); }
    }

    public static Vector2 Movement
    {
        get
        {
            if (Config.SwapInputs)
            {
                return GetCameraAsMovement();
            }

            return GetMovement();
        }
    }

    public static Vector2 Camera
    {
        get
        {
            if (Config.SwapInputs)
            {
                return GetMovementAsCamera();
            }

            return GetCamera();
        }
    }

    static Vector2 GetMovement()
    {
        _movement.x = CrossPlatformInputManager.GetAxis("HorizontalCamera");
        _movement.y = CrossPlatformInputManager.GetAxis("VerticalCamera");

        _movement.x += CrossPlatformInputManager.GetAxis("Left Thumb X");
        _movement.y += -CrossPlatformInputManager.GetAxis("Left Thumb Y");

        return _movement;
    }

    static Vector2 GetMovementAsCamera()
    {
        _movement.x = CrossPlatformInputManager.GetAxis("HorizontalCamera");
        _movement.y = -CrossPlatformInputManager.GetAxis("VerticalCamera");

        _movement.x += CrossPlatformInputManager.GetAxis("Left Thumb X");
        _movement.y += CrossPlatformInputManager.GetAxis("Left Thumb Y");

        return _movement;
    }

    static Vector2 GetCamera()
    {
        _camera.x = CrossPlatformInputManager.GetAxis("Right Thumb X") * 0.5f;
        _camera.y = CrossPlatformInputManager.GetAxis("Right Thumb Y") * 0.5f;

        _camera.x += -CrossPlatformInputManager.GetAxis("Horizontal") * 0.5f;
        _camera.y += CrossPlatformInputManager.GetAxis("Vertical") * 0.25f;

        _rawMouse.x = CrossPlatformInputManager.GetAxisRaw("Mouse X");
        _rawMouse.y = CrossPlatformInputManager.GetAxisRaw("Mouse Y");

        // Scale input against the sensitivity setting and multiply that against the smoothing value
        _rawMouse = Vector2.Scale(_rawMouse, mouseScale);

        // Interpolate mouse movement over time to apply smoothing delta
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, _rawMouse.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, _rawMouse.y, 1f / smoothing.y); 

        _camera.x += _smoothMouse.x;
        _camera.y += _smoothMouse.y;

        return _camera;
    }

        static Vector2 GetCameraAsMovement()
    {
        _camera.x = -CrossPlatformInputManager.GetAxis("Right Thumb X");
        _camera.y = -CrossPlatformInputManager.GetAxis("Right Thumb Y");

        _camera.x += -CrossPlatformInputManager.GetAxis("Horizontal");
        _camera.y += -CrossPlatformInputManager.GetAxis("Vertical");

        _rawMouse.x = CrossPlatformInputManager.GetAxisRaw("Mouse X");
        _rawMouse.y = CrossPlatformInputManager.GetAxisRaw("Mouse Y");

        // Scale input against the sensitivity setting and multiply that against the smoothing value
        _rawMouse = Vector2.Scale(_rawMouse, mouseScale);

        // Interpolate mouse movement over time to apply smoothing delta
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, _rawMouse.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, _rawMouse.y, 1f / smoothing.y); 

        _camera.x += _smoothMouse.x;
        _camera.y += _smoothMouse.y;

        return _camera;
    }
}