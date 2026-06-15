using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BoxCorp
{
    public class Grabber : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private RawImage _renderImage;

        [Header("Grabber Settings")]
        [SerializeField] private LayerMask _grabLayerMask;
        [SerializeField] private LayerMask _clickableLayerMask;
        [SerializeField] private float _maxGrabDistance = 100f;
        [SerializeField] private float _dragStrength = 15f;
        [SerializeField] private float _throwMultiplier = 5f;
        [SerializeField] private float _maxThrowSpeed = 10f;
        [SerializeField] private float _spinStrength = 0.1f;
        [SerializeField] private float _planeZ = 0f;
        [SerializeField] private float _boostStrength = 10f;

        private PhysicsGrabbable _grabbable;
        private bool _canBoost;

        private void Start()
        {
            GameManager.Instance.OnBoostThresholdReached += HandleBoostThresholdReached;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBoostThresholdReached -= HandleBoostThresholdReached;
            }
        }

        private void Update()
        {
            if (MenuController.GameIsPaused)
            {
                return;
            }

            Pointer pointer = Pointer.current;

            if (pointer != null && pointer.press.wasPressedThisFrame)
            {
                TryGrabObject();
            }
            else if (pointer != null && pointer.press.wasReleasedThisFrame)
            {
                ReleaseObject();
            }
            else if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && _canBoost)
            {
                Boost();
            }
        }

        private void FixedUpdate()
        {
            if (_grabbable == null)
            {
                return;
            }

            Vector3 targetPos = GetMouseWorldPositionFromRenderTexture();
            Vector3 desiredVelocity = (targetPos - _grabbable.Rigidbody.position) * _dragStrength;
            _grabbable.Rigidbody.linearVelocity = Vector3.Lerp(
                _grabbable.Rigidbody.linearVelocity, desiredVelocity, 0.5f);
        }

        public void LetGo()
        {
            _grabbable = null;
        }

        private void Boost()
        {
            PhysicsGrabbable grabbable = TryGetGrabbable();

            if (grabbable != null && grabbable.CanGrab)
            {
                grabbable.Boosted();
                Vector3 currentVelocity = grabbable.Rigidbody.linearVelocity;
                currentVelocity.y = _boostStrength;
                grabbable.Rigidbody.linearVelocity = currentVelocity;
            }
        }

        private PhysicsGrabbable TryGetGrabbable()
        {
            if (!TryGetPointerRay(out Ray ray))
            {
                return null;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, _maxGrabDistance, _grabLayerMask))
            {
                if (hit.collider.TryGetComponent(out PhysicsGrabbable grabbable) 
                    && !grabbable.IsGrabbed 
                    && grabbable.CanGrab)
                {
                    return grabbable;
                }
            }
            return null;
        }

        private bool TryClickClickable()
        {
            if (!TryGetPointerRay(out Ray ray))
            {
                return false;
            }

            if (Physics.Raycast(ray, out RaycastHit hit, _maxGrabDistance, _clickableLayerMask))
            {
                if (hit.collider.TryGetComponent(out IClickable clickable))
                {
                    clickable.OnClick(hit.point);
                    return true;
                }
            }
            return false;
        }

        // Converts the current pointer position into a world space ray through the render texture.
        // Returns false if the pointer is outside the raw image.
        private bool TryGetPointerRay(out Ray ray)
        {
            RectTransform rect = _renderImage.rectTransform;

            // Convert pointer position to local raw image coordinates.
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect,
                GetPointerPosition(),
                null,
                out Vector2 localPoint))
            {
                ray = default;
                return false;
            }

            // Convert to normalized UV, then map to a camera viewport ray.
            Vector2 uv = new(
                (localPoint.x + rect.rect.width / 2f) / rect.rect.width,
                (localPoint.y + rect.rect.height / 2f) / rect.rect.height
            );

            ray = _mainCamera.ViewportPointToRay(uv);
            return true;
        }

        private Vector2 GetPointerPosition() =>
            Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;

        private void TryGrabObject()
        {
            PhysicsGrabbable grabbable = TryGetGrabbable();

            if (grabbable != null)
            {
                _grabbable = grabbable;
                grabbable.Grabbed(this);
            }
            else
            {
                TryClickClickable();
            }
        }

        private void ReleaseObject()
        {
            if (_grabbable == null)
            {
                return;
            }

            // Calculate directional velocity.
            Vector3 releaseVelocity = _grabbable.Rigidbody.linearVelocity * _throwMultiplier;
            releaseVelocity = Vector3.ClampMagnitude(releaseVelocity, _maxThrowSpeed);

            // Calculate angular velocity.
            float speedFactor = releaseVelocity.magnitude * _spinStrength;
            Vector3 angularVelocity = Mathf.Sign(releaseVelocity.x) * speedFactor * Vector3.back;

            _grabbable.Released(releaseVelocity, angularVelocity);
            _grabbable = null;
        }

        private Vector3 GetMouseWorldPositionFromRenderTexture()
        {
            if (!TryGetPointerRay(out Ray ray))
            {
                return Vector3.zero;
            }

            // Draw debug ray to see where the heck it's pointing in the scene view.
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);

            // Cast against the drag plane.
            Plane plane = new(Vector3.forward, new Vector3(0, 0, _planeZ));
            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero; // Fallback if no position found.
        }

        private void HandleBoostThresholdReached()
        {
            _canBoost = true;
        }
    }
}
