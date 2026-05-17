#nullable enable
namespace UniT.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniT.DI;
    using UniT.Extensions;
    using UniT.Logging;
    using UniT.Pooling;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Scripting;
    using ILogger = UniT.Logging.ILogger;
    using Object = UnityEngine.Object;
    #if UNIT_UNITASK
    using System.Threading;
    using Cysharp.Threading.Tasks;
    #else
    using System.Collections;
    #endif

    public sealed class UIManager : IUIManager
    {
        #region Constructor

        private readonly RootUICanvas         canvas;
        private readonly EventSystem          eventSystem;
        private readonly IDependencyContainer container;
        private readonly IObjectPoolManager   objectPoolManager;
        private readonly ILogger              logger;

        private readonly Transform                         root              = new GameObject(nameof(UIManager)).DontDestroyOnLoad().transform;
        private readonly HashSet<object>                   trackingKeys      = new();
        private readonly HashSet<GameObject>               trackingPrefabs   = new();
        private readonly HashSet<IActivity>                showingActivities = new();
        private readonly Dictionary<GameObject, IActivity> objToActivity     = new();
        private readonly Dictionary<IActivity, IView[]>    activityToViews   = new();

        [Preserve]
        public UIManager(RootUICanvas canvas, EventSystem eventSystem, IDependencyContainer container, IObjectPoolManager objectPoolManager, ILoggerManager loggerManager)
        {
            this.canvas            = canvas;
            this.eventSystem       = eventSystem;
            this.container         = container;
            this.objectPoolManager = objectPoolManager;
            this.logger            = loggerManager.GetLogger(this);

            this.canvas.transform.parent      = this.root;
            this.eventSystem.transform.parent = this.root;

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

        void IUIManager.Load(IActivity prefab)
        {
            this.trackingPrefabs.Add(prefab.gameObject);
            this.objectPoolManager.Load(prefab.gameObject);
        }

        #if !UNITY_WEBGL
        void IUIManager.Load(object key, int count)
        {
            this.trackingKeys.Add(key);
            this.objectPoolManager.Load(key, count);
        }
        #endif

        #if UNIT_UNITASK
        UniTask IUIManager.LoadAsync(object key, int count, IProgress<float>? progress, CancellationToken cancellationToken)
        {
            this.trackingKeys.Add(key);
            return this.objectPoolManager.LoadAsync(key, count, progress, cancellationToken);
        }
        #else
        IEnumerator IUIManager.LoadAsync(object key, int count, Action? callback, IProgress<float>? progress)
        {
            this.trackingKeys.Add(key);
            return this.objectPoolManager.LoadAsync(key, count, callback, progress);
        }
        #endif

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

        void IUIManager.Hide(IActivity activity) => this.objectPoolManager.Recycle(activity.gameObject);

        void IUIManager.HideAll(IActivity prefab) => this.objectPoolManager.RecycleAll(prefab.gameObject);

        void IUIManager.HideAll(object key) => this.objectPoolManager.RecycleAll(key);

        void IUIManager.Unload(IActivity prefab)
        {
            this.trackingPrefabs.Remove(prefab.gameObject);
            this.objectPoolManager.Unload(prefab.gameObject);
        }

        void IUIManager.Unload(object key)
        {
            this.trackingKeys.Remove(key);
            this.objectPoolManager.Unload(key);
        }

        #endregion

        #region Private

        private Action<IActivity, IReadOnlyList<IView>>? initialized;
        private Action<IActivity, IReadOnlyList<IView>>? shown;
        private Action<IActivity, IReadOnlyList<IView>>? hidden;
        private Action<IActivity, IReadOnlyList<IView>>? disposed;

        private int    lockCount;
        private object nextParams = null!;

        private void OnInstantiated(GameObject instance)
        {
            if (!instance.TryGetComponent<IActivity>(out var activity)) return;
            this.objToActivity.Add(instance, activity);
            var views = activity.gameObject.GetComponentsInChildren<IView>();
            this.activityToViews.Add(activity, views);
            foreach (var view in views.AsSpan())
            {
                view.Container = this.container;
                view.Manager   = this;
                view.Activity  = activity;
            }
            foreach (var view in views.AsSpan()) view.OnInitialize();
            this.initialized?.Invoke(activity, views);
        }

        private void OnSpawned(GameObject instance)
        {
            if (!this.objToActivity.TryGetValue(instance, out var activity)) return;
            if (activity.Type is ActivityType.Screen)
            {
                this.showingActivities.Where(activity => activity.Type is not ActivityType.Overlay)
                    .SafeForEach(static (activity, objectPoolManager) => objectPoolManager.Recycle(activity.gameObject), this.objectPoolManager);
            }
            activity.transform.SetParent(activity.Type switch
            {
                ActivityType.Screen       => this.canvas.Screens,
                ActivityType.Popup        => this.canvas.Popups,
                ActivityType.Overlay      => this.canvas.Overlays,
                ActivityType.OverlayPopup => this.canvas.OverlayPopups,
                _                         => throw new ArgumentOutOfRangeException(nameof(activity.Type), activity.Type, null),
            }, false);
            if (activity is IActivityWithParams activityWithParams)
            {
                activityWithParams.Params = this.nextParams;
            }
            var views = this.activityToViews[activity];
            foreach (var view in views.AsSpan()) view.OnShow();
            this.showingActivities.Add(activity);
            this.shown?.Invoke(activity, views);
        }

        private void OnRecycled(GameObject instance)
        {
            if (!this.objToActivity.TryGetValue(instance, out var activity)) return;
            var views = this.activityToViews[activity];
            foreach (var view in views.AsSpan()) view.OnHide();
            if (activity is IActivityWithParams activityWithParams)
            {
                activityWithParams.Params = null;
            }
            this.showingActivities.Remove(activity);
            this.hidden?.Invoke(activity, views);
        }

        private void OnCleanedUp(GameObject instance)
        {
            if (!this.objToActivity.Remove(instance, out var activity)) return;
            this.activityToViews.Remove(activity, out var views);
            foreach (var view in views.AsSpan()) view.OnDispose();
            this.disposed?.Invoke(activity, views);
        }

        #endregion

        #region Finalizer

        private void Dispose()
        {
            this.trackingKeys.Clear(this.objectPoolManager.Unload);
            this.trackingPrefabs.Clear(this.objectPoolManager.Unload);
            if (this.root) Object.Destroy(this.root.gameObject);
        }

        void IDisposable.Dispose()
        {
            this.Dispose();
            this.logger.Debug("Disposed");
        }

        ~UIManager()
        {
            this.Dispose();
            this.logger.Debug("Finalized");
        }

        #endregion
    }
}