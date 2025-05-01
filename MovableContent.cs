using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Immersal.Samples.ContentPlacement
{
    public enum MovementMode
    {
        Raycast,
        Traditional
    }

    public class MovableContent : MonoBehaviour
    {
        [SerializeField]
        private float m_ClickHoldTime = 0.1f;

        private float m_timeHold = 0f;
        private bool m_EditingContent = false;
        private Transform m_CameraTransform;
        private float m_MovePlaneDistance;

        private ARRaycastManager arRaycastManager;
        private List<ARRaycastHit> arHits = new List<ARRaycastHit>();

        private MovementModeManager modeManager;

        public static MovableContent CurrentActive { get; private set; }

        // Переменные для двухпальцевого поворота
        private bool isRotating = false;
        private int fingerId1, fingerId2;
        private Vector2 prevTouch1, prevTouch2;

        // Переменные для перетаскивания без скачка
        private Vector3 initialObjectPosition;
        private Vector3 initialRaycastPosition;
        private Vector3 initialProjectionPosition;

        private void Start()
        {
            m_CameraTransform = Camera.main.transform;
            StoreContent();

            arRaycastManager = FindObjectOfType<ARRaycastManager>();
            if (arRaycastManager == null)
            {
                Debug.LogWarning("ARRaycastManager not found - Raycast mode disabled");
            }

            modeManager = FindObjectOfType<MovementModeManager>();
            if (modeManager == null)
            {
                Debug.LogWarning("MovementModeManager not found on scene!");
            }
        }

        private void Update()
        {
            // Двухпальцевый поворот только для активной модели
            if (Input.touchCount == 2 && CurrentActive == this)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                if (!isRotating && touch0.phase == TouchPhase.Began && touch1.phase == TouchPhase.Began)
                {
                    isRotating = true;
                    fingerId1 = touch0.fingerId;
                    fingerId2 = touch1.fingerId;
                    prevTouch1 = touch0.position;
                    prevTouch2 = touch1.position;
                }
                else if (isRotating)
                {
                    Vector2 currentTouch1Pos = Vector2.zero;
                    Vector2 currentTouch2Pos = Vector2.zero;
                    bool found1 = false, found2 = false;
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Touch touch = Input.GetTouch(i);
                        if (touch.fingerId == fingerId1 && touch.phase != TouchPhase.Canceled)
                        {
                            currentTouch1Pos = touch.position;
                            found1 = true;
                        }
                        else if (touch.fingerId == fingerId2 && touch.phase != TouchPhase.Canceled)
                        {
                            currentTouch2Pos = touch.position;
                            found2 = true;
                        }
                    }
                    if (found1 && found2)
                    {
                        Vector2 prevVector = prevTouch2 - prevTouch1;
                        Vector2 currentVector = currentTouch2Pos - currentTouch1Pos;
                        float prevAngle = Mathf.Atan2(prevVector.y, prevVector.x) * Mathf.Rad2Deg;
                        float currentAngle = Mathf.Atan2(currentVector.y, currentVector.x) * Mathf.Rad2Deg;
                        float deltaAngle = currentAngle - prevAngle;
                        transform.Rotate(transform.up, deltaAngle);
                        prevTouch1 = currentTouch1Pos;
                        prevTouch2 = currentTouch2Pos;
                    }
                    else
                    {
                        isRotating = false;
                    }
                }
            }
            else
            {
                isRotating = false;
            }
        }

        private void OnMouseDown()
        {
            CurrentActive = this;
            Debug.Log($"Active object set: {gameObject.name} (Tag: {gameObject.tag})");
        }

        private void OnMouseDrag()
        {
            if (CurrentActive != this) return;
            if (Input.touchCount != 1) return; // Перемещение только при одном касании
            m_timeHold += Time.deltaTime;
            if (m_timeHold >= m_ClickHoldTime && !m_EditingContent)
            {
                initialObjectPosition = transform.position; // Записываем начальную позицию модели
                if (modeManager != null)
                {
                    if (modeManager.CurrentMode == MovementMode.Traditional)
                    {
                        m_MovePlaneDistance = Vector3.Dot(
                            transform.position - m_CameraTransform.position,
                            m_CameraTransform.forward
                        ) / m_CameraTransform.forward.sqrMagnitude;
                        // Записываем начальную точку касания на плоскости
                        initialProjectionPosition = Camera.main.ScreenToWorldPoint(
                            new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_MovePlaneDistance)
                        );
                    }
                    else if (modeManager.CurrentMode == MovementMode.Raycast && arRaycastManager != null)
                    {
                        if (arRaycastManager.Raycast(Input.mousePosition, arHits, TrackableType.Planes))
                        {
                            initialRaycastPosition = arHits[0].pose.position;
                        }
                    }
                }
                m_EditingContent = true;
            }
            if (m_EditingContent)
            {
                if (modeManager != null && modeManager.CurrentMode == MovementMode.Raycast && arRaycastManager != null)
                {
                    UpdatePositionWithRaycast();
                }
                else
                {
                    UpdatePositionTraditional();
                }
            }
        }

        private void OnMouseUp()
        {
            StoreContent();
            ResetState();
        }

        private void ResetState()
        {
            m_timeHold = 0f;
            m_EditingContent = false;
        }

        private void StoreContent()
        {
            if (!MultipleContentStorageManager.Instance.contentList.Contains(this))
            {
                MultipleContentStorageManager.Instance.contentList.Add(this);
            }
            MultipleContentStorageManager.Instance.SaveContents();
        }

        public void RemoveContent()
        {
            if (MultipleContentStorageManager.Instance.contentList.Contains(this))
            {
                MultipleContentStorageManager.Instance.contentList.Remove(this);
            }
            MultipleContentStorageManager.Instance.SaveContents();
            Destroy(gameObject);
        }

        private void UpdatePositionWithRaycast()
        {
            if (arRaycastManager.Raycast(Input.mousePosition, arHits, TrackableType.Planes))
            {
                Vector3 currentRaycastPosition = arHits[0].pose.position;
                transform.position = initialObjectPosition + (currentRaycastPosition - initialRaycastPosition);
            }
        }

        private void UpdatePositionTraditional()
        {
            Vector3 currentProjection = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_MovePlaneDistance)
            );
            transform.position = initialObjectPosition + (currentProjection - initialProjectionPosition);
        }
    }
}
