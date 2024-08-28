using UnityEngine;

namespace PixelWater
{
    [RequireComponent(typeof(Camera))]
    public class WaterReflectionCamera : MonoBehaviour
    {
        public Transform followedCameraTransform;

        Transform waterPlane;
        Camera reflectionCamera;
        Vector2Int waterResolution = new Vector2Int(-1, -1); // State variable for updates

        void OnEnable()
        {
            reflectionCamera = GetComponent<Camera>();

            if (transform.parent == null)
            {
                Debug.LogError("The water reflection camera should have a parent that is the water plane.");
            }
            waterPlane = transform.parent;

            if (Camera.main == null)
            {
                Debug.LogError("There is no main camera found! Set the tag in editor.");
            }
            if (followedCameraTransform == null)
            {
                followedCameraTransform = Camera.main.transform;
            }

            ApplyNewRenderTexture();
        }
        private void OnDisable()
        {
            if (reflectionCamera.targetTexture != null)
            {
                reflectionCamera.targetTexture.Release();
            }
        }

        private void LateUpdate()
        {

            transform.position = PlanarReflectionProbe.GetPosition(followedCameraTransform.position, waterPlane.position, waterPlane.up);

            transform.LookAt(transform.position + Vector3.Reflect(followedCameraTransform.forward, waterPlane.up), Vector3.Reflect(followedCameraTransform.up, waterPlane.up));

            reflectionCamera.projectionMatrix = PlanarReflectionProbe.GetObliqueProjection(reflectionCamera, waterPlane.position, waterPlane.up);

            reflectionCamera.orthographicSize = Camera.main.orthographicSize;
            if (Camera.main.targetTexture != null)
            {
                if (Camera.main.targetTexture.width != waterResolution.x || Camera.main.targetTexture.height != waterResolution.y)
                {
                    ApplyNewRenderTexture();
                }
            }
        }
        void ApplyNewRenderTexture()
        {
            var textureResolution = Camera.main.targetTexture == null ? new Vector2Int(Camera.main.pixelWidth, Camera.main.pixelHeight) : new Vector2Int(Camera.main.targetTexture.width, Camera.main.targetTexture.height);
            var newTexture = NewCameraTargetTexture(textureResolution);
            SetCameraTexture(reflectionCamera, newTexture);

            var materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetTexture("_WaterReflectionTexture", newTexture);
            waterPlane.GetComponent<MeshRenderer>().SetPropertyBlock(materialPropertyBlock);

            waterResolution = textureResolution; // state
        }

        static RenderTexture NewCameraTargetTexture(Vector2Int textureSize) // Creates similar texture as main camera has
        {
            RenderTexture newTexture = new RenderTexture(textureSize.x, textureSize.y, 32, RenderTextureFormat.ARGB32);
            newTexture.filterMode = FilterMode.Point;
            newTexture.Create();

            return newTexture;
        }

        static void SetCameraTexture(Camera camera, RenderTexture renderTexture)
        {
            if (camera.targetTexture != null)
            {
                camera.targetTexture.Release();
            }

            camera.targetTexture = renderTexture;
        }
    }
}