#nullable enable
namespace UniT.UI.Utilities
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public abstract class GenericButton : View
    {
        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(this.OnClick);
        }

        protected override void OnDispose()
        {
            this.GetComponent<Button>().onClick.RemoveListener(this.OnClick);
        }

        protected abstract void OnClick();
    }
}