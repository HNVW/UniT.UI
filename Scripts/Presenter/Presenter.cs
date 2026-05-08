#nullable enable
namespace UniT.UI.Presenter
{
    public abstract class Presenter<TView> : IPresenter where TView : IViewWithPresenter
    {
        IView IPresenter.View { set => this.View = (TView)value; }

        protected TView View { get; private set; } = default!;

        protected IUIManager Manager => this.View.Manager;

        protected IActivity Activity => this.View.Activity;

        void IViewLifecycle.OnInitialize() => this.OnInitialize();

        void IViewLifecycle.OnShow() => this.OnShow();

        void IViewLifecycle.OnHide() => this.OnHide();

        void IViewLifecycle.OnDispose() => this.OnDispose();

        protected virtual void OnInitialize() { }

        protected virtual void OnShow() { }

        protected virtual void OnHide() { }

        protected virtual void OnDispose() { }
    }
}