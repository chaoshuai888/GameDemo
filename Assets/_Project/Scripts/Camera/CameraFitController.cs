using LawnDefense.Grid;
using UnityEngine;

namespace LawnDefense.CameraTools
{
    public sealed class CameraFitController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private float padding = 1f;

        private void Start()
        {
            if (gridSystem != null)
            {
                Fit(gridSystem);
            }
        }

        public void Fit(GridSystem targetGrid)
        {
            Camera cameraToFit = targetCamera != null ? targetCamera : Camera.main;
            if (cameraToFit == null || targetGrid == null || targetGrid.Rows <= 0 || targetGrid.Columns <= 0)
            {
                return;
            }

            cameraToFit.orthographic = true;

            Vector3 bottomLeft = targetGrid.GridToWorld(new GridCoordinate(0, 0));
            Vector3 topRight = targetGrid.GridToWorld(new GridCoordinate(targetGrid.Rows - 1, targetGrid.Columns - 1));
            Vector3 center = (bottomLeft + topRight) * 0.5f;
            center.z = cameraToFit.transform.position.z;
            cameraToFit.transform.position = center;

            float gridHeight = Mathf.Abs(topRight.y - bottomLeft.y) + targetGrid.CellSize.y + padding * 2f;
            float gridWidth = Mathf.Abs(topRight.x - bottomLeft.x) + targetGrid.CellSize.x + padding * 2f;
            float aspect = cameraToFit.aspect > 0f ? cameraToFit.aspect : 16f / 9f;
            float verticalSize = gridHeight * 0.5f;
            float horizontalSize = gridWidth / aspect * 0.5f;
            cameraToFit.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
        }
    }
}
