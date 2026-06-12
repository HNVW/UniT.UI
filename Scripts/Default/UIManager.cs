#nullable enable
namespace UniT.UI.Default
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UniT.DI;
    using UniT.Extensions;
    using UniT.Logging;
    using UniT.Pooling;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Scripting;
    using ILogger = UniT.Logging.ILogger;
    using Object = UnityEngine.Object;

    public sealed class UIManager : IUIManager
    {
        #region Constructor

        private readonly EventSystem          eventSystem;
        private readonly IDependencyContainer container;
        private readonly IObjectPoolManager   objectPoolManager;
        private readonly ILogger              logger;

        private readonly IReadOnlyDictionary<ActivityType, Transform> activityRoots;

        private readonly Transform                     root              = new GameObject(nameof(UIManager)).DontDestroyOnLoad().transform;
        private readonly HashSet<object>               trackingKeys      = new();
        private readonly HashSet<GameObject>           trackingPrefabs   = new();
        private readonly HashSet<IActivity>            showingActivities = new();
        private readonly Dictionary<GameObject, IView> objToViews        = new();
        private readonly Dictionary<IView, IView[]>    viewToChildren    = new();

        [Preserve]
        public UIManager(Canvas canvas, EventSystem eventSystem, IDependencyContainer container, IObjectPoolManager objectPoolManager, ILoggerManager loggerManager)
        {
            this.eventSystem       = eventSystem;
            this.container         = container;
            this.objectPoolManager = objectPoolManager;
            this.logger            = loggerManager.GetLogger(this);

            var canvasTransform = canvas.transform;

            canvasTransform.parent            = this.root;
            this.eventSystem.transform.parent = this.root;

            this.activityRoots = Enum.GetValues(typeof(ActivityType))
                .Cast<ActivityType>()
                .ToDictionary(
                    type => type,
                    type =>
                    {
                        var child = new GameObject(type.ToString()).AddComponent<RectTransform>();
                        child.SetParent(canvasTransform, false);
                        child.anchorMin = Vector2.zero;
                        child.anchorMax = Vector2.one;
                        child.sizeDelta = Vector2.zero;
                        return (Transform)child;
                    }
                );

            this.objectPoolManager.Instantiated += this.OnInstantiated;
            this.objectPoolManager.Spawned      += this.OnSpawned;
            this.objectPoolManager.Recycled     += this.OnRecycled;
            this.objectPoolManager.CleanedUp    += this.OnCleanedUp;

            this.logger.Debug("Constructed");
        }

        #endregion

        #region Public

        event Action<IActivity, IReadOnlyList<IView>> IUIManager.Initialized { add => this.initialized += value; remove => this.initialized -= value; }
        event Action<IActivity, IReadOnlyList<IView>> IUIManager.Shown       { add => this.shown += value;       remove => this.shown -= value; }
        event Action<IActivity, IReadOnlyList<IView>> IUIManager.Hidden      { add => this.hidden += value;      remove => this.hidden -= value; }
        event Action<IActivity, IReadOnlyList<IView>> IUIManager.Disposed    { add => this.disposed += value;    remove => this.disposed -= value; }

        IActivity? IUIManager.            ShowingScreen        => this.showingActivities.SingleOrDefault(activity => activity.Type is ActivityType.Screen);
        IEnumerable<IActivity> IUIManager.ShowingPopups        => this.showingActivities.Where(activity => activity.Type is ActivityType.Popup);
        IEnumerable<IActivity> IUIManager.ShowingOverlays      => this.showingActivities.Where(activity => activity.Type is ActivityType.Overlay);
        IEnumerable<IActivity> IUIManager.ShowingOverlayPopups => this.showingActivities.Where(activity => activity.Type is ActivityType.OverlayPopup);

        void IUIManager.LockInteraction()
        {
            if (++this.lockCount != 1) return;
            this.eventSystem.enabled = false;
            this.logger.Debug("Interaction locked");
        }

        void IUIManager.UnlockInteraction(bool force)
        {
            if (this.lockCount <= 0) return;
            if (force) this.lockCount = 1;
            if (--this.lockCount != 0) return;
            this.eventSystem.enabled = true;
            this.logger.Debug("Interaction unlocked");
        }

        void IUIManager.Load(IView prefab)
        {
            this.trackingPrefabs.Add(prefab.gameObject);
            this.objectPoolManager.Load(prefab.gameObject);
        }

        UniTask IUIManager.LoadAsync(object key, int count, IProgress<float>? progress, CancellationToken cancellationToken)
        {
            this.trackingKeys.Add(key);
            return this.objectPoolManager.LoadAsync(key, count, progress, cancellationToken);
        }

        TActivity IUIManager.Show<TActivity>(TActivity prefab, ActivityShowMode mode)
        {
            if (mode is ActivityShowMode.Single) this.objectPoolManager.RecycleAll(prefab.gameObject);
            return this.objectPoolManager.Spawn<TActivity>(prefab.gameObject);
        }

        TActivity IUIManager.Show<TActivity, TParams>(TActivity prefab, TParams @params, ActivityShowMode mode)
        {
            if (mode is ActivityShowMode.Single) this.objectPoolManager.RecycleAll(prefab.gameObject);
            this.nextParams = @params;
            return this.objectPoolManager.Spawn<TActivity>(prefab.gameObject);
        }

        TActivity IUIManager.Show<TActivity>(object key, ActivityShowMode mode)
        {
            if (mode is ActivityShowMode.Single) this.objectPoolManager.RecycleAll(key);
            return this.objectPoolManager.Spawn<TActivity>(key);
        }

        TActivity IUIManager.Show<TActivity, TParams>(object key, TParams @params, ActivityShowMode mode)
        {
            if (mode is ActivityShowMode.Single) this.objectPoolManager.RecycleAll(key);
            this.nextParams = @params;
            return this.objectPoolManager.Spawn<TActivity>(key);
        }

        TView IUIManager.Show<TView>(TView prefab, IActivity activity, Transform? parent)
        {
            this.nextActivity = activity;
            return this.objectPoolManager.Spawn<TView>(prefab.gameObject, parent: parent, spawnInWorldSpace: false);
        }

        TView IUIManager.Show<TView, TParams>(TView prefab, TParams @params, IActivity activity, Transform? parent)
        {
            this.nextParams   = @params;
            this.nextActivity = activity;
            return this.objectPoolManager.Spawn<TView>(prefab.gameObject, parent: parent, spawnInWorldSpace: false);
        }

        TView IUIManager.Show<TView>(object key, IActivity activity, Transform? parent)
        {
            this.nextActivity = activity;
            return this.objectPoolManager.Spawn<TView>(key, parent: parent, spawnInWorldSpace: false);
        }

        TView IUIManager.Show<TView, TParams>(object key, TParams @params, IActivity activity, Transform? parent)
        {
            this.nextParams   = @params;
            this.nextActivity = activity;
            return this.objectPoolManager.Spawn<TView>(key, parent: parent, spawnInWorldSpace: false);
        }

        void IUIManager.Hide(IView instance)
        {
            if (instance.Equals(null)) return;
            this.objectPoolManager.Recycle(instance.gameObject);
        }

        void IUIManager.HideAll(IView prefab) => this.objectPoolManager.RecycleAll(prefab.gameObject);

        void IUIManager.HideAll(object key) => this.objectPoolManager.RecycleAll(key);

        void IUIManager.Unload(IView prefab)
        {
            this.trackingPrefabs.Remove(prefab.gameObject);
            this.objectPoolManager.Unload(prefab.gameObject);
        }

        void IUIManager.Unload(object key)
        {
            this.trackingKeys.Remove(key);
            this.objectPoolManager.Unload(key);
        }

        void IDisposable.Dispose()
        {
            this.trackingKeys.Clear(this.objectPoolManager.Unload);
            this.trackingPrefabs.Clear(this.objectPoolManager.Unload);
            if (this.root) Object.Destroy(this.root.gameObject);

            this.objectPoolManager.Instantiated -= this.OnInstantiated;
            this.objectPoolManager.Spawned      -= this.OnSpawned;
            this.objectPoolManager.Recycled     -= this.OnRecycled;
            this.objectPoolManager.CleanedUp    -= this.OnCleanedUp;

            this.logger.Debug("Disposed");
        }

        #endregion

        #region Private

        private Action<IActivity, IReadOnlyList<IView>>? initialized;
        private Action<IActivity, IReadOnlyList<IView>>? shown;
        private Action<IActivity, IReadOnlyList<IView>>? hidden;
        private Action<IActivity, IReadOnlyList<IView>>? disposed;

        private int       lockCount;
        private object    nextParams   = null!;
        private IActivity nextActivity = null!;

        private void OnInstantiated(GameObject instance)
        {
            if (!instance.TryGetComponent<IView>(out var view)) return;
            this.objToViews.Add(instance, view);
            var children = view.gameObject.GetComponentsInChildren<IView>();
            this.viewToChildren.Add(view, children);
            var root = view as IActivity ?? this.nextActivity;
            foreach (var child in children.AsSpan())
            {
                child.Container = this.container;
                child.Manager   = this;
                child.Activity  = root;
            }
            foreach (var child in children.AsSpan()) child.OnInitialize();
            if (view is not IActivity activity) return;
            this.initialized?.Invoke(activity, children);
        }

        private void OnSpawned(GameObject instance)
        {
            if (!this.objToViews.TryGetValue(instance, out var view)) return;
            if (view is IActivity { Type: var type })
            {
                if (type is ActivityType.Screen)
                {
                    this.showingActivities.Where(activity => activity.Type is not ActivityType.Overlay)
                        .SafeForEach(static (activity, objectPoolManager) => objectPoolManager.Recycle(activity.gameObject), this.objectPoolManager);
                }
                view.gameObject.transform.SetParent(this.activityRoots[type], false);
            }
            if (view is IViewWithParams viewWithParams)
            {
                viewWithParams.Params = this.nextParams;
            }
            var children = this.viewToChildren[view];
            foreach (var child in children.AsSpan()) child.OnShow();
            if (view is not IActivity activity) return;
            this.showingActivities.Add(activity);
            this.shown?.Invoke(activity, children);
        }

        private void OnRecycled(GameObject instance)
        {
            if (!this.objToViews.TryGetValue(instance, out var view)) return;
            var children = this.viewToChildren[view];
            foreach (var child in children.AsSpan()) child.OnHide();
            if (view is IViewWithParams viewWithParams)
            {
                viewWithParams.Params = null;
            }
            if (view is not IActivity activity) return;
            this.showingActivities.Remove(activity);
            this.hidden?.Invoke(activity, children);
        }

        private void OnCleanedUp(GameObject instance)
        {
            if (!this.objToViews.Remove(instance, out var view)) return;
            this.viewToChildren.Remove(view, out var children);
            foreach (var child in children.AsSpan()) child.OnDispose();
            if (view is not IActivity activity) return;
            this.disposed?.Invoke(activity, children);
        }

        #endregion
    }
}