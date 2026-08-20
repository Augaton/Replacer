using ZoneShilari.Common.Hints;

namespace Replacer.API
{
    public static class HintBridge
    {
        private static readonly HintChannel Channel = new HintChannel("replacer", 600f);

        public static float YCoordinate
        {
            get { return Channel.YCoordinate; }
            set { Channel.YCoordinate = value; }
        }

        public static int FontSize
        {
            get { return Channel.FontSize; }
            set { Channel.FontSize = value; }
        }

        public static void Show(Exiled.API.Features.Player player, string text, float duration)
        {
            Channel.Show(player, text, duration);
        }

        public static void Remove(Exiled.API.Features.Player player) => Channel.Remove(player);

        public static void Clear() => Channel.Clear();
    }
}
