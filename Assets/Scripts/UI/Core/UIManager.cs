using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Escalatopia.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private List<UIView> registeredViews = new List<UIView>();
        private Stack<UIView> viewHistory = new Stack<UIView>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Auto-register children views
            registeredViews.AddRange(GetComponentsInChildren<UIView>(true));
        }

        public void RegisterView(UIView view)
        {
            if (!registeredViews.Contains(view))
            {
                registeredViews.Add(view);
            }
        }

        public T GetView<T>() where T : UIView
        {
            return registeredViews.OfType<T>().FirstOrDefault();
        }

        public UIView GetView(string viewId)
        {
            return registeredViews.FirstOrDefault(v => v.viewId == viewId);
        }

        public void OpenView<T>() where T : UIView
        {
            T view = GetView<T>();
            if (view != null)
            {
                OpenView(view);
            }
            else
            {
                Debug.LogWarning($"[UIManager] View of type {typeof(T).Name} not found.");
            }
        }

        public void OpenView(string viewId)
        {
            UIView view = GetView(viewId);
            if (view != null)
            {
                OpenView(view);
            }
            else
            {
                Debug.LogWarning($"[UIManager] View with ID {viewId} not found.");
            }
        }

        private void OpenView(UIView view)
        {
            // If blocking input, maybe pause game? context dependent.
            view.Open();
            viewHistory.Push(view);
        }

        public void CloseCurrentView()
        {
            if (viewHistory.Count > 0)
            {
                UIView view = viewHistory.Pop();
                view.Close();
            }
        }

        public void CloseAllViews()
        {
            while (viewHistory.Count > 0)
            {
                CloseCurrentView();
            }
        }
    }
}
