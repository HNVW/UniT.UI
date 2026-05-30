#nullable enable
namespace UniT.UI.Utilities
{
    using UnityEngine;

    public sealed class GenericActivity : Activity
    {
        [SerializeField] private ActivityType type;

        public override ActivityType Type => this.type;
    }
}