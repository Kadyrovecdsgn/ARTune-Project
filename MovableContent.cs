using UnityEngine;

namespace Immersal.Samples.ContentPlacement
{
    public class MovableContent : MonoBehaviour
    {
        [SerializeField]
        private float m_ClickHoldTime = 0.1f; // Время удержания для перемещения
        [SerializeField]
        private float m_RotationSpeed = 10f; // Скорость вращения в градусах в секунду
        [SerializeField]
        private float m_DoubleClickTime = 0.3f; // Максимальный интервал между кликами для двойного нажатия
        [SerializeField]
        private float m_RotationHoldTime = 0.5f; // Время удержания после двойного нажатия для вращения

        private float m_timeHold = 0f; // Время удержания кнопки
        private bool m_EditingContent = false; // Флаг для перемещения
        private bool m_RotatingContent = false; // Флаг для вращения
        private Transform m_CameraTransform; // Трансформ камеры
        private float m_MovePlaneDistance; // Расстояние до плоскости перемещения

        private float m_LastClickTime = 0f; // Время последнего клика
        private bool m_DoubleClickDetected = false; // Флаг обнаружения двойного нажатия

        private void Start()
        {
            m_CameraTransform = Camera.main.transform;
            StoreContent();
        }

        private void Update()
        {
            // Перемещение объекта
            if (m_EditingContent)
            {
                Vector3 projection = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_MovePlaneDistance));
                transform.position = projection;
            }

            // Вращение объекта
            if (m_RotatingContent)
            {
                transform.Rotate(Vector3.up, m_RotationSpeed * Time.deltaTime);
            }
        }

        private void OnMouseDown()
        {
            // Проверка на двойное нажатие
            float timeSinceLastClick = Time.time - m_LastClickTime;
            if (timeSinceLastClick < m_DoubleClickTime)
            {
                m_DoubleClickDetected = true;
            }
            else
            {
                m_DoubleClickDetected = false;
            }
            m_LastClickTime = Time.time;
        }

        private void OnMouseDrag()
        {
            m_timeHold += Time.deltaTime;

            if (m_DoubleClickDetected)
            {
                // Активация вращения после двойного нажатия и удержания 0.5 секунды
                if (m_timeHold >= m_RotationHoldTime && !m_RotatingContent)
                {
                    m_RotatingContent = true;
                }
            }
            else
            {
                // Активация перемещения при обычном удержании 0.1 секунды
                if (m_timeHold >= m_ClickHoldTime && !m_EditingContent)
                {
                    m_MovePlaneDistance = Vector3.Dot(transform.position - m_CameraTransform.position, m_CameraTransform.forward) / m_CameraTransform.forward.sqrMagnitude;
                    m_EditingContent = true;
                }
            }
        }

        private void OnMouseUp()
        {
            // Сброс всех состояний при отпускании кнопки
            StoreContent();
            m_timeHold = 0f;
            m_EditingContent = false;
            m_RotatingContent = false;
            m_DoubleClickDetected = false;
        }

        private void StoreContent()
        {
            if (!ContentStorageManager.Instance.contentList.Contains(this))
            {
                ContentStorageManager.Instance.contentList.Add(this);
            }
            ContentStorageManager.Instance.SaveContents();
        }

        public void RemoveContent()
        {
            if (ContentStorageManager.Instance.contentList.Contains(this))
            {
                ContentStorageManager.Instance.contentList.Remove(this);
            }
            ContentStorageManager.Instance.SaveContents();
            Destroy(gameObject);
        }
    }
}