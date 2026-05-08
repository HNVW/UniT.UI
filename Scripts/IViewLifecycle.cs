#nullable enable
namespace UniT.UI
{
    public interface IViewLifecycle
    {
        public void OnInitialize();

        public void OnShow();

        public void OnHide();

        public void OnDispose();
    }
}