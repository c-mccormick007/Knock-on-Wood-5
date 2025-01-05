using System.Collections;
using UnityEngine;

namespace bandcProd.UIHelpers
{
    public class Panel : MonoBehaviour
    {
        [Tooltip("Distance to move the panel down when hidden")]
        public float hideDistance = 100f;

        [Tooltip("Time (in seconds) for the show/hide animation")]
        public float animationTime = 0.5f;

        private Vector3 _originalPosition;
        private bool _isVisible = true;
        private Coroutine _currentAnimation;

        [Tooltip("Drop the Panel here:")]
        [SerializeField]
        private RectTransform _transform;

        [Tooltip("Drop the Panel here:")]
        [SerializeField]
        private GameObject _self;

        public bool isVisible
        {
            get { return _isVisible; }
        }

        private void Awake()
        {
            _originalPosition = _transform.localPosition;
        }

        private void Start()
        {
            _transform.localPosition = _originalPosition - new Vector3(0, hideDistance, 0);
        }

        /// <summary>
        /// Toggles the visibility of the panel.
        /// </summary>
        public void TogglePanel()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _isVisible = !_isVisible;

            if (_isVisible)
            {
                _currentAnimation = StartCoroutine(MoveToPosition(_originalPosition));
            }
            else
            {
                _currentAnimation = StartCoroutine(MoveToPosition(_originalPosition - new Vector3(0, hideDistance, 0)));
            }
        }

        public void ToggleActivationFalse()
        {
            _isVisible = !_isVisible;
            _self.SetActive(false);
        }

        public void ToggleActivationTrue()
        {
            _isVisible = !_isVisible;
            _self.SetActive(false);
        }

        /// <summary>
        /// Smoothly moves the panel to a target position.
        /// </summary>
        private IEnumerator MoveToPosition(Vector3 targetPosition)
        {
            Vector3 startPosition = _transform.localPosition;
            float elapsedTime = 0f;

            while (elapsedTime < animationTime)
            {
                transform.localPosition = Vector3.Lerp(startPosition, targetPosition, elapsedTime / animationTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = targetPosition;
        }
    }
}