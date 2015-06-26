using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LeapSynthesis
{
    class Metronome
    {
        private Stopwatch watch = new Stopwatch();

        private long millisecondsPerBeat;

        private long currentBeat = 0;

        public long BeatsPerMinute { get; set; }
        
        // initialize the metronome with the given beats per minute
        public Metronome(long bpm)
        {
            BeatsPerMinute = bpm;

            float millisecondsPerMinute = 60000;

            millisecondsPerBeat = (long)(millisecondsPerMinute / bpm);
        }

        // starts the metronome
        public void Start()
        {
            watch.Start();
        }

        // resets the time, and starts the metronome again
        public void Restart()
        {
            watch.Restart();
        }

        // resets the time, and stops the metronome
        public void Reset()
        {
            watch.Reset();
        }

        // stops the metronome, but does not reset the time
        // equivalent to pause
        public void Stop()
        {
            watch.Stop();
        }

        // must be called to use the beat functionality
        public void UpdateBeat()
        {
            if (watch.IsRunning)
            {
                if (watch.ElapsedMilliseconds % millisecondsPerBeat == 0 ||
                    watch.ElapsedMilliseconds % millisecondsPerBeat == watch.ElapsedMilliseconds)
                {
                    currentBeat++;
                }
            }
        }

        // get the current beat
        public long GetBeat()
        {
            return currentBeat;
        }
    }
}
