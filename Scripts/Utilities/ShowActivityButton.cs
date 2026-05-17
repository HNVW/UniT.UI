#nullable enable
namespace UniT.UI.Utilities
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByPrefabButton<TActivity> : View where TActivity : IActivityWithoutParams
    {
        [SerializeField] private TActivity        prefab = default!;
        [SerializeField] private ActivityShowMode mode   = ActivityShowMode.Single;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.prefab, this.mode));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByPrefabButton<TActivity, TParams> : View where TActivity : IActivityWithParams<TParams> where TParams : notnull
    {
        [SerializeField]     private TActivity        prefab  = default!;
        [SerializeReference] private TParams          @params = default!;
        [SerializeField]     private ActivityShowMode mode    = ActivityShowMode.Single;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show(this.prefab, this.@params, this.mode));
        }
    }

    #if !UNITY_WEBGL
    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByKeyButton<TActivity> : View where TActivity : IActivityWithoutParams
    {
        [SerializeField] private string           key  = string.Empty;
        [SerializeField] private ActivityShowMode mode = ActivityShowMode.Single;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show<TActivity>(this.key, this.mode));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByKeyButton<TActivity, TParams> : View where TActivity : IActivityWithParams<TParams> where TParams : notnull
    {
        [SerializeField]     private string           key     = string.Empty;
        [SerializeReference] private TParams          @params = default!;
        [SerializeField]     private ActivityShowMode mode    = ActivityShowMode.Single;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show<TActivity, TParams>(this.key, this.@params, this.mode));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByTypeButton<TActivity> : View where TActivity : IActivityWithoutParams
    {
        [SerializeField] private ActivityShowMode mode = ActivityShowMode.Single;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show<TActivity>(this.mode));
        }
    }

    [RequireComponent(typeof(Button))]
    public abstract class ShowActivityByTypeButton<TActivity, TParams> : View where TActivity : IActivityWithParams<TParams> where TParams : notnull
    {
        [SerializeReference] private TParams          @params = default!;
        [SerializeField]     private ActivityShowMode mode    = ActivityShowMode.Single;

        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Show<TActivity, TParams>(this.@params, this.mode));
        }
    }
    #endif
}