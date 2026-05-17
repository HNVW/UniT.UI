#nullable enable
namespace UniT.UI.Utilities
{
    using System;
    using System.Collections.Generic;
    using UniT.Extensions;
    using UnityEngine;

    public abstract class SimpleViewAdapter<TParams, TView> : View where TView : IViewWithParams
    {
        [SerializeField] private RectTransform content = null!;
        [SerializeField] private TView         prefab  = default!;

        private readonly Dictionary<IView, IView[]> views        = new();
        private readonly Stack<TView>               pooledViews  = new();
        private readonly HashSet<TView>             spawnedViews = new();

        public void Set(IEnumerable<TParams> allParams)
        {
            this.OnHide();
            foreach (var @params in allParams)
            {
                var view = this.pooledViews.PopOrDefault(static @this =>
                {
                    var view       = Instantiate(@this.prefab.gameObject, @this.content).GetComponentOrThrow<TView>();
                    var childViews = view.gameObject.GetComponentsInChildren<IView>();
                    @this.views.Add(view, childViews);
                    foreach (var childView in childViews.AsSpan())
                    {
                        childView.Container = @this.Container;
                        childView.Manager   = @this.Manager;
                        childView.Activity  = @this.Activity;
                    }
                    foreach (var childView in @this.views[view].AsSpan()) childView.OnInitialize();
                    return view;
                }, this);
                view.transform.SetAsLastSibling();
                view.gameObject.SetActive(true);
                view.Params = @params!;
                foreach (var childView in this.views[view].AsSpan()) childView.OnShow();
                this.spawnedViews.Add(view);
            }
        }

        protected override void OnHide()
        {
            this.spawnedViews.Clear(view =>
            {
                view.gameObject.SetActive(false);
                foreach (var childView in this.views[view].AsSpan()) childView.OnHide();
                this.pooledViews.Push(view);
            });
        }

        protected override void OnDispose()
        {
            this.pooledViews.Clear(view =>
            {
                Destroy(view.gameObject);
                foreach (var childView in this.views[view].AsSpan()) childView.OnDispose();
            });
        }
    }
}