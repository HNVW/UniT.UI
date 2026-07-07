#nullable enable
namespace UniT.UI.Utilities
{
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class SimpleViewAdapter<TParams, TView> : View where TView : IViewWithParams<TParams> where TParams : notnull
    {
        [SerializeField] private RectTransform content = null!;
        [SerializeField] private TView prefab = default!;

        protected override void OnInitialize()
        {
            this.Manager.Load(this.prefab);
        }

        public void Set(IEnumerable<TParams> allParams)
        {
            this.Manager.HideAll(this.prefab);
            foreach (var @params in allParams)
            {
                this.Manager.Show(this.prefab, @params, this.Activity, this.content);
            }
        }

        protected override void OnHide()
        {
            this.Manager.HideAll(this.prefab);
        }

        protected override void OnDispose()
        {
            this.Manager.Unload(this.prefab);
        }
    }
}