using System.Media;
using System.Windows;

namespace Vegraris.Wpf
{
    class SoundEffectPlayer
    {
        public SoundEffectPlayer()
        {
            readyMessagePlayer = LoadPlayer("/Sounds/readyMessage.wav");
            lineClearPlayer = LoadPlayer("/Sounds/lineClear.wav");
            lockDownPlayer = LoadPlayer("/Sounds/lockDown.wav");
        }

        private static SoundPlayer? LoadPlayer(string uri)
        {
            try
            {
                var result = new SoundPlayer(Application.GetResourceStream(new Uri(uri, UriKind.Relative)).Stream);
                result.Load();
                return result;
            }
            catch
            {
                return null;
            }
        }

        private readonly SoundPlayer? readyMessagePlayer;
        private readonly SoundPlayer? lineClearPlayer;
        private readonly SoundPlayer? lockDownPlayer;

        public void PlayLineClearEffect() => lineClearPlayer?.Play();
        public void PlayReadyMessage() => readyMessagePlayer?.Play();
        public void PlayLockDownEffect() => lockDownPlayer?.Play();
    }
}
