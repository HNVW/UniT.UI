#nullable enable
namespace UniT.UI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UniT.Extensions;

    public interface IUIManager : IDisposable
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

        public void Load(IActivity prefab);

        public UniTask LoadAsync(object key, int count = 1, IProgress<float>? progress = null, CancellationToken cancellationToken = default);

        public TActivity Show<TActivity>(TActivity prefab, ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithoutParams;

        public TActivity Show<TActivity, TParams>(TActivity prefab, TParams @params, ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithParams<TParams> where TParams : notnull;

        public TActivity Show<TActivity>(object key, ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithoutParams;

        public TActivity Show<TActivity, TParams>(object key, TParams @params, ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithParams<TParams> where TParams : notnull;

        public void Hide(IActivity instance);

        public void HideAll(IActivity prefab);

        public void HideAll(object key);

        public void Unload(IActivity prefab);

        public void Unload(object key);

        #region Implicit Key

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask LoadAsync<TActivity>(int count = 1, IProgress<float>? progress = null, CancellationToken cancellationToken = default) where TActivity : IActivity => this.LoadAsync(typeof(TActivity).GetKey(), count, progress, cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TActivity Show<TActivity>(ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithoutParams => this.Show<TActivity>(typeof(TActivity).GetKey(), mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TActivity Show<TActivity, TParams>(TParams @params, ActivityShowMode mode = ActivityShowMode.Single) where TActivity : IActivityWithParams<TParams> where TParams : notnull => this.Show<TActivity, TParams>(typeof(TActivity).GetKey(), @params, mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HideAll<TActivity>() where TActivity : IActivity => this.HideAll(typeof(TActivity).GetKey());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unload<TActivity>() where TActivity : IActivity => this.Unload(typeof(TActivity).GetKey());

        #endregion
    }
}