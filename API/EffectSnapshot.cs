using Exiled.API.Enums;

namespace Replacer.API
{
    public readonly struct EffectSnapshot
    {
        public EffectSnapshot(EffectType type, byte intensity, float duration)
        {
            Type = type;
            Intensity = intensity;
            Duration = duration;
        }

        public EffectType Type { get; }

        public byte Intensity { get; }

        public float Duration { get; }
    }
}
