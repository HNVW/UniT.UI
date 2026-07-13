#nullable enable
namespace UniT.UI
{
    using System;
    using DI;
    using UnityEngine;

    public interface IView : IViewLifecycle
    {
        public IDependencyContainer Container { set; }

        public IUIManager Manager { get; set; }

        public IActivity Activity { get; set; }

        public GameObject gameObject { get; }

        public Transform transform { get; }
    }

    public interface IViewWithoutParams : IView
    {
    }

    public interface IViewWithParams : IView
    {
        public object? Params { set; }
    }

    public interface IViewWithParams<in TParams> : IViewWithParams where TParams : notnull
    {
        object? IViewWithParams.Params
        {
            set => this.Params = value switch
            {
                null => default,
                TParams @params => @params,
                _ => throw new InvalidOperationException($"{this.GetType().Name} expected params of type {typeof(TParams)}, got {value.GetType().Name}"),
            };
        }

        public new TParams? Params { set; }
    }
}