#nullable enable
namespace UniT.UI
{
    using System;
    using UniT.DI;
    using UnityEngine;

    public interface IView : IViewLifecycle
    {
        public IDependencyContainer Container { set; }

        public IUIManager Manager { get; set; }

        public IActivity Activity { get; set; }

        // ReSharper disable once InconsistentNaming
        public GameObject gameObject { get; }

        // ReSharper disable once InconsistentNaming
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
                null            => default,
                TParams @params => @params,
                _               => throw new InvalidCastException($"{this.GetType().Name} expected params of type {typeof(TParams)}, got {value.GetType().Name}"),
            };
        }

        public new TParams? Params { set; }
    }
}