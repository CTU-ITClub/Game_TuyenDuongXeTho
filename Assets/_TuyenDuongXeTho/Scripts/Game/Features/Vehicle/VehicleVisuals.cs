using UnityEngine;

namespace Game.Features.Vehicle
{
    public class VehicleVisuals : MonoBehaviour
    {
        #region SerializeField Property
        [Header("Physics Ref")]
        [SerializeField] private WheelCollider _steerWheel;

        [Header("Visual Parts")]
        [SerializeField] private Transform _handlebarPivot;
        [SerializeField] private Transform _frontPivot;   // QUAN TRỌNG
        [SerializeField] private Transform _frontWheel;
        [SerializeField] private Transform _backWheel;

        [Header("Settings")]
        [SerializeField] private float _smoothSteerSpeed = 10f;
        [SerializeField] private float _wheelRadius = 0.35f;

        [Header("Model Fix")]
        [Tooltip("Fix model bị lệch (ví dụ: -90, 180...)")]
        [SerializeField] private Vector3 _steerOffset = new Vector3(-90f, 0f, 0f);
        #endregion

        #region Private Property
        private float _currentSteer;
        private float _spin;
        private Vector3 _lastPos;
        private Quaternion _frontInitRot;
        private Quaternion _backInitRot;
        #endregion

        private void Start()
        {
            _lastPos = transform.position;

            if (_frontWheel != null) _frontInitRot = _frontWheel.localRotation;
            if (_backWheel != null) _backInitRot = _backWheel.localRotation;
        }

        private void LateUpdate()
        {
            UpdateSpin();
            UpdateSteer();
            Apply();
        }
        //cập nhật góc xoay của bánh xe
        private void UpdateSpin()
        {
            Vector3 delta = transform.position - _lastPos;
            //tính quãng đường
            float distance = Vector3.Dot(delta, transform.forward);
            //2piR
            float circumference = 2f * Mathf.PI * _wheelRadius;
            float spinDelta = (distance / circumference) * 360f;
            //giới hạn 360 độ
            _spin += spinDelta;
            _spin %= 360f;

            _lastPos = transform.position;
        }

        //cập nhật việc bánh xe xoay qua trái phải
        private void UpdateSteer()
        {
            float target = _steerWheel != null ? _steerWheel.steerAngle : 0f;
            _currentSteer = Mathf.Clamp(_currentSteer, -15, 20);
            _currentSteer = Mathf.Lerp(
                _currentSteer,
                target,
                Time.deltaTime * _smoothSteerSpeed
            );
        }

        private void Apply()
        {
            //Cập nhật cổ xe xoay theo bánh
            if (_handlebarPivot != null)
            {
                _handlebarPivot.localRotation =
                    Quaternion.Euler(_steerOffset + new Vector3(0, _currentSteer, 0));
            }

            //Cập nhật bánh xe trước  xoay qua trái phải
            if (_frontPivot != null)
            {
                _frontPivot.localRotation =
                    Quaternion.Euler(_steerOffset + new Vector3(0, _currentSteer, 0));
            }
            //Cập nhật bánh xe trước xoay tròn
            if (_frontWheel != null)
            {
                _frontWheel.localRotation =
                    _frontInitRot * Quaternion.Euler(_spin, 0, 0);
            }

            //cập nhật xoay bánh sau
            if (_backWheel != null)
            {
                _backWheel.localRotation =
                    _backInitRot * Quaternion.Euler(_spin, 0, 0);
            }
        }
    }
}