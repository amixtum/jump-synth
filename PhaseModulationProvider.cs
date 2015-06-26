using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace LeapSynthesis
{
    public class PhaseModulationProvider : WaveProvider32
    {
        private float carrierPhase = 0;
        private float modulationPhase = 0;

        private float carrierIncrement;
        private float modulationIncrement;

        public float CarrierFrequency { get; set; }
        public float ModulationFraction { get; set; }
        public float ModulationFrequency { get; set; }
        public float ModulationIndex { get; set; }
        public float Amplitude { get; set; }

        public PhaseModulationProvider()
        {
            SetWaveFormat(44100, 1);
            SetParameters(0, 0, 0, 0, 0);
        }

        public PhaseModulationProvider(float carrierFreq, float modFreq, float modIndex, float modFraction, float amp)
        {
            SetWaveFormat(44100, 1);
            SetParameters(carrierFreq, modFreq, modIndex, modFraction, amp);
        }
        
        public void SetParameters(float carrierFreq, float modFreq, float modIndex, float modFraction, float amp)
        {
            CarrierFrequency = carrierFreq;
            ModulationFrequency = modFreq;
            ModulationIndex = modIndex;
            ModulationFraction = modFraction;
            Amplitude = amp;

            ModulationFrequency = CarrierFrequency * ModulationFraction;

            carrierIncrement = (float)((Math.PI * 2 * ModulationFrequency) / WaveFormat.SampleRate);
            modulationIncrement = (float)((Math.PI * 2 * CarrierFrequency / WaveFormat.SampleRate));
        }

        public float Compute()
        {
            float modIndex = ModulationIndex;

            return (float)(Math.Sin(carrierPhase + modIndex*Math.Cos(modulationPhase)));
        }

        public void LerpCarrierTo(float target, float frac)
        {
            CarrierFrequency = Lerp(CarrierFrequency, target, frac);

            carrierIncrement = (float)((Math.PI * 2 * ModulationFrequency) / WaveFormat.SampleRate);
        }

        public void LerpModulationTo(float target, float frac)
        {
            ModulationFrequency = Lerp(ModulationFrequency, target, frac);

            modulationIncrement = (float)((Math.PI * 2 * CarrierFrequency / WaveFormat.SampleRate));
        }

        public void LerpModulationIndex(float target, float frac)
        {
            ModulationIndex = Lerp(ModulationIndex, target, frac);
        }

        public void LerpModulationFraction(float target, float frac)
        {
            ModulationFraction = Lerp(ModulationFraction, target, frac);
        }

        public float Lerp(float current, float target, float frac)
        {
            return current + (target - current) * frac;
        }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int sampleRate = this.WaveFormat.SampleRate;

            for (int i = 0; i < sampleCount; ++i)
            {
                buffer[i + offset] = Amplitude * Compute();

                carrierPhase += carrierIncrement;
                modulationPhase += modulationIncrement;

                if (carrierPhase >= Math.PI * 2)
                {
                    carrierPhase -= (float)(Math.PI * 2);
                }
                if (modulationPhase >= Math.PI * 2)
                {
                    modulationPhase -= (float)(Math.PI * 2);
                }
                if (carrierPhase < 0.0f)
                {
                    carrierPhase += (float)(Math.PI * 2);
                }
                if (modulationPhase < 0.0f)
                {
                    modulationPhase += (float)(Math.PI * 2);
                }
            }

            return sampleCount;
        }
    }
}
