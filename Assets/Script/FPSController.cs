using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float _yawSpeed;
    [SerializeField] private float _pitchSpeed;
    [SerializeField] private float _minPitch;
    [SerializeField] private float _maxPitch;
    [SerializeField] private GameObject _pitchController;
    [SerializeField] private bool _invertPitch;
    [SerializeField] private bool _invertYaw;

    [Header("Movement")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private KeyCode _frontKey;
    [SerializeField] private KeyCode _backKey;
    [SerializeField] private KeyCode _rightKey;
    [SerializeField] private KeyCode _leftKey;
    [SerializeField] private KeyCode _shift;
    [SerializeField] private float _runMultiplier;
    private bool _dashReady = true;

    [Header("Jump")]
    [SerializeField] private KeyCode _space;
    [SerializeField] private float _timeToMaxHeight;
    [SerializeField] private float _height;
    [SerializeField] private bool _onGround;
    [SerializeField] private bool _onContactCeiling;

    float _verticalSpeed;
    float _currYaw;
    float _currPitch;
    float _gravity;
    float _startingGravity;
    float _jumpSpeed;
    Vector3 _speed;

    float _xMousePos = 0;
    float _yMousePos = 0;

    float _xRecoil = 0;
    float _yRecoil = 0;


    //Fijar el ratón y que se quede invisible
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;
    public KeyCode m_DebugLockKeyCode = KeyCode.O;

    [SerializeField] bool m_AngleLocked = false;
    [SerializeField] bool m_AimLocked = false;

    //[SerializeField] private Animation _anim;

    void Awake()
    {
        _currYaw = transform.rotation.eulerAngles.y;
        _currPitch = _pitchController.transform.rotation.eulerAngles.x;

        _startingGravity = -2 * _height / (_timeToMaxHeight * _timeToMaxHeight);
        _gravity = _startingGravity;
        _jumpSpeed = 2 * _height / _timeToMaxHeight;
    }

    void FixedUpdate()
    {
        Move();
        if (!m_AngleLocked)
            Rotate();
    }

    private void Update()
    {
        InputRotate();
        LockCursor();

    }

    private void LockCursor()
    {
        if (Input.GetKeyDown(m_DebugLockAngleKeyCode))
            m_AngleLocked = !m_AngleLocked;
        if (Input.GetKeyDown(m_DebugLockKeyCode))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
            m_AimLocked = Cursor.lockState == CursorLockMode.Locked;
        }
    }

    private void Move()
    {
        /*
         * Computes the forward and right vector by trigonometry.
         * If _currYaw is 0, forward would be (0,0,1), if _currYaw is 90 degrees then forward is (1,0,0).
         * The same applies with right vector, only with a 90 degree offset.
         */
        Vector3 forward = new Vector3(Mathf.Sin(_currYaw * Mathf.Deg2Rad), 0.0f, Mathf.Cos(_currYaw * Mathf.Deg2Rad));
        Vector3 right = new Vector3(Mathf.Sin((_currYaw + 90.0f) * Mathf.Deg2Rad), 0.0f, Mathf.Cos((_currYaw + 90.0f) * Mathf.Deg2Rad));
        Vector3 force = new Vector3();

        /*
         * Get the front/back input. If going backwards magnitude is the same but with inverse direction
         * (represented with a substraction)
         */
        if (Input.GetKey(_frontKey))
        {
            force += forward;
        }
        else if (Input.GetKey(_backKey))
        {
            force -= forward;
        }

        /*
         * The same applies here
         */
        if (Input.GetKey(_leftKey))
        {
            force -= right;
        }
        else if (Input.GetKey(_rightKey))
        {
            force += right;
        }

        if (_onGround && Input.GetKey(_space))
        {
            _gravity = _startingGravity;
            _verticalSpeed = _jumpSpeed;
        }

        /*
         * Make sure vector always has magnitude <= 1. If not, we could be going faster by going diagonally,
         * as magnitude of (1,0,1) is NOT 1.
         */
        force.Normalize();

        /*
         * lMovement is our direction. We can now multiply it by a scalar number to increase the magnitude,
         * and with that, the speed of our character.
         */
        _speed += force * _moveSpeed * Time.fixedDeltaTime;

        //if (force.magnitude < 0.1) _anim.CrossFade("Idle");
        //else _anim.CrossFade("Walking");


        if (Input.GetKey(_shift) && _dashReady)
        {
            _dashReady = false;
            StartCoroutine(readyDash());
            _speed *= 8;
        }
        _speed *= 0.8f;
        Vector3 fPos = _speed * 5 * Time.fixedDeltaTime;

        _verticalSpeed += _gravity * Time.fixedDeltaTime;
        fPos.y = _verticalSpeed * Time.fixedDeltaTime;

        //Apply the movement of the vector to our character controller.
        CollisionFlags colls = _characterController.Move(fPos);

        _onGround = (colls & CollisionFlags.Below) != 0;
        _onContactCeiling = (colls & CollisionFlags.Above) != 0;

        if (_onGround) _verticalSpeed = 0.0f;
        if (_onContactCeiling && _verticalSpeed > 0.0f) _verticalSpeed = 0.0f;
        if (_verticalSpeed < 0.0f)
        {
            _gravity = _startingGravity * 1.7f;
        }


    }

    private IEnumerator readyDash()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        _dashReady = true;
    }
    private void InputRotate()
    {
        /*
         * Get the difference between frames of mouse position. Both axis. DO NOT
         * USE delta time after, you are already using frame specific results.
         */
        _xMousePos += Input.GetAxis("Mouse X");
        _yMousePos += Input.GetAxis("Mouse Y");

    }

    private void Rotate()
    {

        /*
         * Use the mouse variables to change the angles of our character.
         * If invert variables == true, then the result will be multiplied by -1
         * Clamp is to limit a number between a minimum and a maximum (avoids the WIIIIII effect)
         */
        _currYaw += (_xMousePos + _xRecoil) * _yawSpeed * (_invertYaw ? -1 : 1);
        _currPitch -= (_yMousePos + _yRecoil) * _pitchSpeed * (_invertPitch ? -1 : 1);
        _currPitch = Mathf.Clamp(_currPitch, _minPitch, _maxPitch);

        _xRecoil *= 0.6f;
        _yRecoil *= 0.6f;
        /*
         * Set final yaw angle to our player character.
         * IMPORTANT: We set the pitch angle to our pitch controller INSTEAD of our character
         */
        transform.rotation = Quaternion.Euler(0.0f, _currYaw, 0.0f);
        _pitchController.transform.localRotation = Quaternion.Euler(_currPitch, 0, 0);

        _xMousePos = 0;
        _yMousePos = 0;
    }

    public void giveRecoil(float x, float y)
    {
        _xRecoil += x;
        _yRecoil += y;
    }


}
