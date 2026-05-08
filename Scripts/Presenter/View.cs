#nullable enable
namespace UniT.UI.Presenter
{
    using System;

    public abstract class View<TPresenter> : View, IViewWithPresenter where TPresenter : IPresenter
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

    public abstract class View<TParams, TPresenter> : UI.View<TParams>, IViewWithPresenter where TPresenter : IPresenter where TParams : notnull
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