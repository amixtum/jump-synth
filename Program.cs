using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

using Leap;

using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace LeapSynthesis
{
    class Program
    {
        static void Main(string[] args)
        {
            Controller controller = new Leap.Controller(); // initialize the leap controller

            while (!controller.IsConnected) { } // wait for it to connect

            PhaseModulationProvider pmProvider = new PhaseModulationProvider(440, (440f / 2f), 0.5f, 0.5f, 0.2f); // initialize the pm oscillator

            MixingSampleProvider mixer = new MixingSampleProvider(new[] { pmProvider });

            mixer.ReadFully = true;

            SynthController synthController = new SynthController(pmProvider, 0.1f); // initialize the object that controls the synth with leap

            WaveOut audioOut = new WaveOut(); // hear things

            audioOut.Init(mixer); // give it the oscillator

            audioOut.Play(); // play it

            RenderWindow window = new RenderWindow(new VideoMode(200, 200), "Test Window, Please Ignore"); // make the sfml window

            window.Closed += (object sender, EventArgs e) => // so I can close it
            {
                RenderWindow w = sender as RenderWindow;
                controller.Dispose();
                w.Close();
            };

            window.KeyPressed += (object sender, KeyEventArgs e) =>
            {
                if (e.Code == Keyboard.Key.Escape)
                {
                    RenderWindow w = sender as RenderWindow;
                    w.Close();
                }
            };

            while (window.IsOpen) // main loop
            {
                synthController.HandleFrame(controller);
                window.DispatchEvents();
                window.Clear();
                window.Display();
            }
        }
    }

    class SynthController
    {
        private PhaseModulationProvider pmProvider;

        private const float yMin = 50;
        private const float yMax = 400;
        private const float xMin = -300;
        private const float xMax = 300;
        private const float zMin = -200;
        private const float zMax = 200;

        private const float A3 = 440f;

        private int[] intervals = { 2, 1, 2, 2, 1, 2, 2 };

        private float[] noteFrequencies = new float[16];

        private float[] modulationIndices = new float[16];

        private float[] modulationFractions = new float[16];

        private float[] yPartition = new float[16];

        private float[] xPartition = new float[16];

        public float InterpolationSpeed { get; set; }

        public SynthController(PhaseModulationProvider pmProvider, float interpSpeed)
        {
            InterpolationSpeed = interpSpeed;

            this.pmProvider = pmProvider;

            AssignFrequencies();

            AssignModulationFractions();

            AssignModulationIndices();

            CreateVerticalParition();

            CreateHorizontalPartition();
        }

        public void HandleFrame(Controller con)
        {
            var frameData = con.Frame();
            TransformCarrierFrequency(frameData);
            TransformModulationIndex(frameData);
            //TransformModulationFraction(frameData);
        }

        private void AssignModulationFractions()
        {
            float fMin = 0.1f;
            float fMax = 2f;
            float subIntervals = modulationFractions.Length;

            for (int i = 0; i < modulationFractions.Length; ++i)
            {
                modulationFractions[i] = fMin + ((fMax - fMin) / subIntervals) * i;
            }
        }

        private void AssignFrequencies()
        {
            //noteFrequencies[0] = A3; // set the first note frequency

            int currentInterval = 0; // will be used for the power of the constant 2^(interval/12)
            int intervalIndex = 0;

            for (int i = 0; i < noteFrequencies.Length; ++i)
            {
                noteFrequencies[i] = (float)(A3 * Math.Pow(2, ((float)currentInterval / 12f))); // see equal temperment tuning

                currentInterval += intervals[intervalIndex]; // increment the interval
                intervalIndex = (intervalIndex == intervals.Length - 1) ? 0 : intervalIndex + 1;
            }
        }

        private void AssignModulationIndices()
        {
            float iMin = 0.5f;
            float iMax = 5;
            float subIntervals = modulationIndices.Length;

            for (int i = 0; i < modulationIndices.Length; ++i)
            {
                modulationIndices[i] = iMin + ((iMax - iMin) / subIntervals) * i;
                Console.WriteLine(modulationIndices[i]);
            }
        }

        private void CreateVerticalParition()
        {
            // form the regular vertical partition
            float width = (yMax - yMin);
            float subIntervals = yPartition.Length;
            for (int i = 0; i < yPartition.Length; ++i)
            {
                yPartition[i] = yMin + (width / subIntervals) * i; // x_0 + (delta_x / n) * i
            }
        }

        private void CreateHorizontalPartition()
        {
            float width = (xMax - xMin);
            float subIntervals = xPartition.Length;
            for (int i = 0; i < xPartition.Length; ++i)
            {
                xPartition[i] = xMin + (width / subIntervals) * i;
            }
        }

        

        private void TransformCarrierFrequency(Leap.Frame frameData)
        {
            foreach (var hand in frameData.Hands)
            {
                if (hand.IsRight)
                {
                    float yPosition = hand.PalmPosition.y;

                    if (yPosition < 500 && yPosition >= 0)
                    {
                        for (int i = 0; i < yPartition.Length; ++i)
                        {
                            if (i < yPartition.Length - 1)
                            {
                                if (yPosition >= yPartition[i] && yPosition < yPartition[i + 1])
                                {
                                    pmProvider.LerpCarrierTo(noteFrequencies[i], InterpolationSpeed);
                                    pmProvider.LerpModulationTo(pmProvider.CarrierFrequency * pmProvider.ModulationFraction, InterpolationSpeed);
                                    return;
                                }
                            }
                            else
                            {
                                pmProvider.CarrierFrequency = noteFrequencies[yPartition.Length - 1];
                            }
                        }
                    }
                }
            }
        }

        private void TransformModulationFrequency(Leap.Frame frameData)
        {

        }

        private void TransformModulationFraction(Leap.Frame frameData)
        {
            foreach (var hand in frameData.Hands)
            {
                if (hand.IsLeft)
                {
                    float yPosition = hand.PalmPosition.y;

                    if (yPosition < 500 && yPosition >= 0)
                    {
                        for (int i = 0; i < yPartition.Length; ++i)
                        {
                            if (i < yPartition.Length - 1)
                            {
                                if (yPosition >= yPartition[i] && yPosition < yPartition[i + 1])
                                {
                                    pmProvider.LerpModulationFraction(modulationFractions[i], InterpolationSpeed);
                                    return;
                                }
                            }
                            else
                            {
                                pmProvider.CarrierFrequency = noteFrequencies[yPartition.Length - 1];
                            }
                        }
                    }
                }
            }
        }

        private void TransformModulationIndex(Leap.Frame frameData)
        {
            foreach (var hand in frameData.Hands)
            {
                if (hand.IsRight)
                {
                    float xPosition = hand.PalmPosition.x;

                    if (xPosition >= xMin && xPosition <= xMax)
                    {
                        for (int i = 0; i < xPartition.Length; ++i)
                        {
                            if (i < xPartition.Length - 1)
                            {
                                if (xPosition >= xPartition[i] && xPosition < xPartition[i + 1])
                                {
                                    pmProvider.LerpModulationIndex(modulationIndices[i], InterpolationSpeed);
                                    return;
                                }
                            }
                            else
                            {
                                pmProvider.LerpModulationIndex(modulationIndices[xPartition.Length - 1], InterpolationSpeed);
                            }
                        }
                    }
                }
            }
        }
    }
}
