#nullable enable
namespace UniT.UI.Utilities
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public sealed class HideButton : GenericButton
    {
        protected override void OnClick()
        {
            this.Manager.Hide(this.Activity);
        }
    }
}