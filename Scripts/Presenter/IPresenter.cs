#nullable enable
namespace UniT.UI.Presenter
{
    public interface IPresenter : IViewLifecycle
    {
        public IView View { set; }
    }
}