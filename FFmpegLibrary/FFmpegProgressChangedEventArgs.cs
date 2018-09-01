using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFmpegLibrary
{
    public class FFmpegProgressChangedEventArgs : EventArgs
    {
        public TimeSpan _duration { get; set; }
        public int _currentSize { get; set; }
        public TimeSpan _currentDuration { get; set; }
        public double _currentBitrate { get; set; }
        public double _currentSpeed { get; set; }

        public FFmpegProgressChangedEventArgs(TimeSpan duration, int currentSize, TimeSpan currentDuration, double currentBitrate, double currentSpeed)
        {
            this._duration = duration;
            this._currentSize = currentSize;
            this._currentDuration = currentDuration;
            this._currentBitrate = currentBitrate;
            this._currentSpeed = currentSpeed;
        }
    }
}
