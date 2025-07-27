using UnityEngine;
using UnityEngine.UI;

namespace BoxCorp
{
    public class Grabber : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private RawImage renderImage;

        [Header("Grabber Settings")]
        [SerializeField] private LayerMask grabLayerMask;
        [SerializeField] private LayerMask clickableLayerMask;
        [SerializeField] private float maxGrabDistance = 100f;
        [SerializeField] private float dragStrength = 15f;
        [SerializeField] private float throwMultiplier = 5f;
        [SerializeField] private float maxThrowSpeed = 10f;
        [SerializeField] private float spinStrength = 0.1f;
        [SerializeField] private float planeZ = 0f;
        [SerializeField] private float boostStrength = 10f;

        private PhysicsGrabbable grabbable;

        public void LetGo()
        {
            grabbable = null;
        }

        private void Update()
        {
            if (MenuController.GameIsPaused)
            {
                return;
            }

            // Using old input manager for convenience ;)
            if (Input.GetMouseButtonDown(0))
            {
                TryGrabObject();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ReleaseObject();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                Boost();
            }
        }

        private void FixedUpdate()
        {
            if (grabbable == null)
            {
                return;
            }

            Vector3 targetPos = GetMouseWorldPositionFromRenderTexture();
            Vector3 desiredVelocity = (targetPos - grabbable.Rb.position) * dragStrength;
            grabbable.Rb.linearVelocity = Vector3.Lerp(grabbable.Rb.linearVelocity, desiredVelocity, 0.5f);
        }

        private void Boost()
        {
            PhysicsGrabbable grabbable = TryGetGrabbable();

            if (grabbable != null && grabbable.CanGrab)
            {
                Vector3 currentVelocity = grabbable.Rb.linearVelocity;
                currentVelocity.y = boostStrength;
                grabbable.Rb.linearVelocity = currentVelocity;
                grabbable.Boosted();
            }
        }

        private PhysicsGrabbable TryGetGrabbable()
        {
            Ray ray = GetRayFromRenderTexture();

            if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, grabLayerMask))
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

        private IClickable TryGetClickable()
        {
            Ray ray = GetRayFromRenderTexture();

            if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, clickableLayerMask))
            {
                if (hit.collider.TryGetComponent(out IClickable clickable))
                {
                    clickable.OnClick();
                    return clickable;
                }
            }
            return null;
        }

        private Ray GetRayFromRenderTexture()
        {
            RectTransform rect = renderImage.rectTransform;

            // Convert mouse position to local raw image coordinates
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect, 
                Input.mousePosition, 
                null, 
                out Vector2 localPoint))
            {
                return new Ray(Vector3.zero, Vector3.forward);
            }

            // Convert to normalized UV
            Vector2 uv = new(
                (localPoint.x + rect.rect.width / 2f) / rect.rect.width,
                (localPoint.y + rect.rect.height / 2f) / rect.rect.height
            );

            // Map UV to camera viewport coordinates
            return mainCamera.ViewportPointToRay(uv);
        }

        private void TryGrabObject()
        {
            PhysicsGrabbable grabbable = TryGetGrabbable();

            if (grabbable != null)
            {
                this.grabbable = grabbable;
                grabbable.Grabbed(this);
            }

            TryGetClickable();
        }

        private void ReleaseObject()
        {
            if (grabbable == null)
            {
                return;
            }

            // Calculate directional velocity
            Vector3 releaseVelocity = grabbable.Rb.linearVelocity * throwMultiplier;
            releaseVelocity = Vector3.ClampMagnitude(releaseVelocity, maxThrowSpeed);

            // Calculate angular velocity
            float speedFactor = releaseVelocity.magnitude * spinStrength;
            Vector3 angularVelocity = Mathf.Sign(releaseVelocity.x) * speedFactor * Vector3.back;

            grabbable.Released(releaseVelocity, angularVelocity);
            grabbable = null;
        }

        private Vector3 GetMouseWorldPositionOnPlane()
        {
            Plane plane = new(Vector3.forward, new Vector3(0, 0, planeZ));
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        private Vector3 GetMouseWorldPositionFromRenderTexture()
        {
            // 1. Get local coordinates relative to the raw image
            RectTransform rect = renderImage.rectTransform;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect, 
                Input.mousePosition, 
                null, 
                out Vector2 localPoint))
            {
                return Vector3.zero;
            }

            // 2. Convert local point to normalized UV
            Vector2 uv = new(
                (localPoint.x + rect.rect.width / 2f) / rect.rect.width,
                (localPoint.y + rect.rect.height / 2f) / rect.rect.height
            );

            // 3. Map UV to camera viewport space
            Ray ray = mainCamera.ViewportPointToRay(uv);

            // 3.5. Draw debug ray to see where the heck it's pointing
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red);

            // 4. Cast against the plane
            Plane plane = new(Vector3.forward, new Vector3(0, 0, planeZ));
            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero; // Fallback if no position found
        }
    }
}