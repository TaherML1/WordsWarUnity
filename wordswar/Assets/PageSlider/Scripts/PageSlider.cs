#region Includes
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

#endregion

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TS.PageSlider
{
    public class PageSlider : MonoBehaviour
    {
        #region Variables

        [Header("References")]
        [Tooltip("Optional reference to a PageDotsIndicator to display dots for each page")]
        [SerializeField] private PageDotsIndicator _dotsIndicator;

        [Header("Navigation Buttons")]
        [SerializeField] private List<Button> navigationButtons; // List of buttons in the navigation bar

        [Header("Children")]
        [Tooltip("A list of PageContainer components representing the pages managed by the PageSlider")]
        [SerializeField] private List<PageContainer> _pages;

        [Header("Configuration")]
        [Tooltip("The index of the page to show at start")]
        [SerializeField] private int _startPageIndex;

        [Header("Events")]
        public UnityEvent<PageContainer> OnPageChanged;

        public Rect Rect { get { return ((RectTransform)transform).rect; } }

        private PageScroller _scroller;

        #endregion

        private void Awake()
        {
            _scroller = FindScroller();
        }

        private IEnumerator Start()
        {
            _scroller.OnPageChangeStarted.AddListener(PageScroller_PageChangeStarted);
            _scroller.OnPageChangeEnded.AddListener(PageScroller_PageChangeEnded);

            yield return new WaitForEndOfFrame();

            if (_startPageIndex == 0) yield break;
            _scroller.SetPage(_startPageIndex);

            for (int i = 0; i < navigationButtons.Count; i++)
            {
                int index = i; // Capture the index in a local variable for the closure
                navigationButtons[i].onClick.AddListener(() => _scroller.NavigateToPage(index));
            }

            // Ensure the correct button is highlighted at startup
            UpdateNavigationButtonAppearance(_startPageIndex);
        }

        public void AddPage(RectTransform content)
        {
            if (_scroller == null)
            {
                _scroller = FindScroller();
            }

            _pages ??= new List<PageContainer>();

            var page = new GameObject($"Page_{_pages.Count}", typeof(RectTransform), typeof(PageContainer));
            page.transform.SetParent(_scroller.Content);

            var rectTransform = page.GetComponent<RectTransform>();
            rectTransform.sizeDelta = _scroller.Rect.size;
            rectTransform.localScale = Vector3.one;

            var pageView = page.GetComponent<PageContainer>();
            pageView.AssignContent(content);

            if (_pages.Count == 0)
            {
                pageView.ChangingToActiveState();
                pageView.ChangeActiveState(true);
            }

            _pages.Add(pageView);

            if (_dotsIndicator != null)
            {
                _dotsIndicator.Add();
                _dotsIndicator.IsVisible = _pages.Count > 1;
            }

#if UNITY_EDITOR
            if (Application.isPlaying) { return; }
            EditorUtility.SetDirty(this);
#endif
        }

        public void Clear()
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                if (_pages[i] == null) { continue; }
#if UNITY_EDITOR
                DestroyImmediate(_pages[i].gameObject);
#else
                Destroy(_pages[i].gameObject);
#endif
            }
            _pages.Clear();

            if (_dotsIndicator != null)
            {
                _dotsIndicator.Clear();
            }
        }

        private void PageScroller_PageChangeStarted(int fromIndex, int toIndex)
        {
            _pages[fromIndex].ChangingToInactiveState();
            _pages[toIndex].ChangingToActiveState();
        }

        private void UpdateNavigationButtonAppearance(int activePageIndex)
        {
            for (int i = 0; i < navigationButtons.Count; i++)
            {
                var button = navigationButtons[i];
                if (i == activePageIndex)
                {
                    // Animate the active button moving up
                    StartCoroutine(AnimateButtonPosition(button.GetComponent<RectTransform>(), 100f, 0.25f)); // move up by 100 units over 0.25 seconds
                }
                else
                {
                    // Animate the inactive button returning to its original position
                    StartCoroutine(AnimateButtonPosition(button.GetComponent<RectTransform>(), 50f, 0.25f)); // move back to original position over 0.25 seconds
                }
            }
        }

        private void PageScroller_PageChangeEnded(int fromIndex, int toIndex)
        {
            _pages[fromIndex].ChangeActiveState(false);
            _pages[toIndex].ChangeActiveState(true);

            if (_dotsIndicator != null)
            {
                _dotsIndicator.ChangeActiveDot(fromIndex, toIndex);
            }
            UpdateNavigationButtonAppearance(toIndex);
            OnPageChanged?.Invoke(_pages[toIndex]);
        }

        private PageScroller FindScroller()
        {
            var scroller = GetComponentInChildren<PageScroller>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (scroller == null)
            {
                Debug.LogError("Missing PageScroller in Children");
            }
#endif
            return scroller;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(PageSlider))]
        public class PageControllerEditor : Editor
        {
            private PageSlider _target;
            private RectTransform _contentPrefab;

            private void OnEnable()
            {
                _target = (PageSlider)target;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Editor");

                _contentPrefab = (RectTransform)EditorGUILayout.ObjectField(_contentPrefab, typeof(RectTransform), false);
                if (GUILayout.Button("Add Page"))
                {
                    _target.AddPage((RectTransform)PrefabUtility.InstantiatePrefab(_contentPrefab));
                }
                if (GUILayout.Button("Clear"))
                {
                    _target.Clear();
                }
            }
        }
#endif

        /// <summary>
        /// Coroutine to animate a button's Y position.
        /// </summary>
        /// <param name="rectTransform">The RectTransform of the button.</param>
        /// <param name="targetY">The target Y position relative to its original position.</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        /// <returns></returns>
        private IEnumerator AnimateButtonPosition(RectTransform rectTransform, float targetY, float duration)
        {
            Vector3 initialPosition = rectTransform.anchoredPosition;
            Vector3 targetPosition = new Vector3(initialPosition.x, targetY, initialPosition.z);
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                rectTransform.anchoredPosition = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            rectTransform.anchoredPosition = targetPosition;
        }
    }
}
