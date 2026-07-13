#nullable enable
namespace UniT.UI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Extensions;
    using UnityEngine;

    public interface IUIManager
    {
        public event Action<IActivity, IReadOnlyList<IView>> Initialized;

        public event Action<IActivity, IReadOnlyList<IView>> Shown;

        public event Action<IActivity, IReadOnlyList<IView>> Hidden;

        public event Action<IActivity, IReadOnlyList<IView>> Disposed;

        public IActivity? ShowingScreen { get; }

        public IEnumerable<IActivity> ShowingPopups { get; }

        public IEnumerable<IActivity> ShowingOverlays { get; }

        public IEnumerable<IActivity> ShowingOverlayPopups { get; }

        public void LockInteraction();

        public void UnlockInteraction(bool force = false);

        public void Load(IView prefab);

        public UniTask LoadAsync(object key, int count = 1, IProgress<float>? progress = null, CancellationToken cancellationToken = default);

        public TActivity Show<TActivity>(TActivity prefab, ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithoutParams;

        public TActivity Show<TActivity, TParams>(TActivity prefab, TParams @params, ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithParams<TParams> where TParams : notnull;

        public TActivity Show<TActivity>(object key, ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithoutParams;

        public TActivity Show<TActivity, TParams>(object key, TParams @params, ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithParams<TParams> where TParams : notnull;

        public TView Show<TView>(TView prefab, IActivity activity, Transform? parent = null) where TView : IViewWithoutParams;

        public TView Show<TView, TParams>(TView prefab, TParams @params, IActivity activity, Transform? parent = null) where TView : IViewWithParams<TParams> where TParams : notnull;

        public TView Show<TView>(object key, IActivity activity, Transform? parent = null) where TView : IViewWithoutParams;

        public TView Show<TView, TParams>(object key, TParams @params, IActivity activity, Transform? parent = null) where TView : IViewWithParams<TParams> where TParams : notnull;

        public void Hide(IView instance);

        public void HideAll(IView prefab);

        public void HideAll(object key);

        public void Unload(IView prefab);

        public void Unload(object key);

        #region Implicit Key

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask LoadAsync<TView>(int count = 1, IProgress<float>? progress = null, CancellationToken cancellationToken = default) where TView : IView => this.LoadAsync(typeof(TView).GetKey(), count, progress, cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TActivity Show<TActivity>(ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithoutParams => this.Show<TActivity>(typeof(TActivity).GetKey(), mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TActivity Show<TActivity, TParams>(TParams @params, ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithParams<TParams> where TParams : notnull => this.Show<TActivity, TParams>(typeof(TActivity).GetKey(), @params, mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TView Show<TView>(IActivity activity, Transform? parent = null) where TView : IViewWithoutParams => this.Show<TView>(typeof(TView).GetKey(), activity, parent);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TView Show<TView, TParams>(TParams @params, IActivity activity, Transform? parent = null) where TView : IViewWithParams<TParams> where TParams : notnull => this.Show<TView, TParams>(typeof(TView).GetKey(), @params, activity, parent);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HideAll<TView>() where TView : IView => this.HideAll(typeof(TView).GetKey());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unload<TView>() where TView : IView => this.Unload(typeof(TView).GetKey());

        #endregion
    }
}