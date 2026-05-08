#nullable enable
namespace UniT.UI.Presenter
{
    using System;

    public abstract class Activity<TPresenter> : Activity, IViewWithPresenter where TPresenter : IPresenter
    {
        protected virtual Type PresenterType => typeof(TPresenter);

        protected TPresenter Presenter { get; private set; } = default!;

        void IViewLifecycle.OnInitialize()
        {
            var presenter = (TPresenter)this.Container.Instantiate(this.PresenterType);
            presenter.View = this;
            this.Presenter = presenter;
            this.OnInitialize();
            this.Presenter.OnInitialize();
        }

        void IViewLifecycle.OnShow()
        {
            this.OnShow();
            this.Presenter.OnShow();
        }

        void IViewLifecycle.OnHide()
        {
            this.OnHide();
            this.Presenter.OnHide();
        }

        void IViewLifecycle.OnDispose()
        {
            this.OnDispose();
            this.Presenter.OnDispose();
        }
    }

    public abstract class Activity<TParams, TPresenter> : UI.Activity<TParams>, IViewWithPresenter where TPresenter : IPresenter where TParams : notnull
    {
        protected virtual Type PresenterType => typeof(TPresenter);

        protected TPresenter Presenter { get; private set; } = default!;

        void IViewLifecycle.OnInitialize()
        {
            var presenter = (TPresenter)this.Container.Instantiate(this.PresenterType);
            presenter.View = this;
            this.Presenter = presenter;
            this.OnInitialize();
            this.Presenter.OnInitialize();
        }

        void IViewLifecycle.OnShow()
        {
            this.OnShow();
            this.Presenter.OnShow();
        }

        void IViewLifecycle.OnHide()
        {
            this.OnHide();
            this.Presenter.OnHide();
        }

        void IViewLifecycle.OnDispose()
        {
            this.OnDispose();
            this.Presenter.OnDispose();
        }
    }
}