using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BenchmarkHashes;
using Diascan.Agent.ModelDB;
using Diascan.Agent.Manager;

namespace CompareHashesConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var agentManager = new AgentManager();
            agentManager.Start();

            //var list = agentManager.controller.buffCalcCollection[0].Helper.Frames;

            //var sw = Stopwatch.StartNew();

            //CombineFrames(null);

            //sw.Stop();
            //Console.WriteLine(sw.Elapsed);
            //Console.ReadKey();
        }

        private static Rect[] Array(Rect[] frames)
        {
            var i = 0;
            var j = 1;

            const double allowScanCount = 0;
            const double allowAngleCount = 0;

            while (i < frames.Length)
            {
                if (j == frames.Length)
                {
                    i++;
                    j = i + 1;
                }

                while (j < frames.Length)
                {
                    //   надо перевернуть

                    var frame = frames[i];
                    var nextFrame = frames[j];

                    var frameRight = frame.Left + frame.Width;
                    var nextFrameRight = nextFrame.Left + nextFrame.Width;

                    var frameTop = (frame.Top + 360) % 360;
                    var nextFrameTop = (nextFrame.Top + 360) % 360;

                    var frameBottom = (frame.Top + frame.Height + 360) % 360;
                    var nextFrameBottom = (nextFrame.Top + nextFrame.Height + 360) % 360;

                    //  первая рамка нормальная
                    if (frameTop < frameBottom)
                    {
                        //  вторая рамка нормальная
                        if (nextFrameTop < nextFrameBottom)
                        {
                            if (nextFrameBottom < frameTop || frameBottom < nextFrameTop ||
                                frameRight < nextFrame.Left || frame.Left > nextFrameRight)
                            {
                                //  если рамки не пересекаются
                                j++;
                                continue;
                            }

                            frame.Y = frameTop < nextFrameTop ? frameTop : nextFrameTop;
                            frame.Height = frameBottom > nextFrameBottom ? frameBottom - frame.Y : nextFrameBottom - frame.Y;
                            frame.X = frame.Left - nextFrame.Left > 0 ? nextFrame.Left : frame.Left;
                            frame.Width = frameRight - nextFrameRight > 0 ? frameRight - frame.X : nextFrameRight - frame.X;
                            frames[i] = frame;
                            nextFrame.Height = 0;
                            nextFrame.Width = 0;
                            frames[j] = nextFrame;
                            break;
                        }
                        else //  вторая рамка перевернутая
                        {
                            if ((frameBottom < nextFrameTop && nextFrameBottom < frameTop) ||
                                frameRight < nextFrame.Left || frame.Left > nextFrameRight)
                            {
                                //  если рамки не пересекаются
                                j++;
                                continue;
                            }

                            if ((nextFrameTop < nextFrameBottom && nextFrameTop > frameTop) ||
                                (frameTop < nextFrameBottom && frameBottom > nextFrameTop))
                            {
                                frame.Y = 0;
                                frame.Height = 360;
                            }
                            else
                            {
                                frame.Y = (frameTop > nextFrameBottom && frameTop < nextFrameTop) ? frameTop : nextFrameTop;
                                frame.Height = 360 - frame.Y + ((frameBottom > nextFrameBottom && frameBottom < nextFrameTop) ? frameBottom : nextFrameBottom);
                            }

                            frame.X = frame.Left - nextFrame.Left > 0 ? nextFrame.Left : frame.Left;
                            frame.Width = frameRight - nextFrameRight > 0 ? frame.Width : nextFrameRight - frame.X;
                            frames[i] = frame;
                            nextFrame.Height = 0;
                            nextFrame.Width = 0;
                            frames[j] = nextFrame;
                            break;
                        }
                    }
                    else // первая рамка перевернутая
                    {
                        //  вторая рамка нормальная
                        if (nextFrameTop < nextFrameBottom)
                        {
                            if ((frameBottom < nextFrameTop && nextFrameBottom < frameTop) ||
                                frameRight < nextFrame.Left || frame.Left > nextFrameRight)
                            {
                                //  если рамки не пересекаются
                                j++;
                                continue;
                            }

                            if ((nextFrameTop < nextFrameBottom && nextFrameTop > frameTop) ||
                                (frameTop < nextFrameBottom && frameBottom > nextFrameTop))
                            {
                                frame.Y = 0;
                                frame.Height = 360;
                            }
                            else
                            {
                                frame.Y = (frameTop > nextFrameTop && frameTop < nextFrameBottom)
                                    ? nextFrameTop
                                    : frameTop;
                                frame.Height = 360 - frame.Y +
                                               ((frameBottom > nextFrameTop && frameBottom < nextFrameBottom)
                                                   ? nextFrameBottom
                                                   : frameBottom);
                            }

                            frame.X = frame.Left - nextFrame.Left > 0 ? nextFrame.Left : frame.Left;
                            frame.Width = frameRight - nextFrameRight > 0 ? frame.Width : nextFrameRight - frame.X;
                            frames[i] = frame;
                            nextFrame.Height = 0;
                            nextFrame.Width = 0;
                            frames[j] = nextFrame;
                            break;
                        }
                        else //  вторая рамка перевернутая
                        {
                            if ((frameTop > nextFrameTop && frameBottom > nextFrameTop) ||
                                (frameTop < nextFrameBottom && frameBottom > nextFrameTop))
                            {
                                frame.Y = 0;
                                frame.Height = 360;
                            }
                            else
                            {
                                frame.Y = frameTop > nextFrameTop ? nextFrameTop : frameTop;
                                frame.Height = 360 - frame.Y + (frameBottom > nextFrameBottom ? frameBottom : nextFrameBottom);
                            }

                            frame.X = frame.Left - nextFrame.Left > 0 ? nextFrame.Left : frame.Left;
                            frame.Width = frameRight - nextFrameRight > 0 ? frame.Width : nextFrameRight - frame.X;
                            frames[i] = frame;
                            nextFrame.Height = 0;
                            nextFrame.Width = 0;
                            frames[j] = nextFrame;
                            break;
                        }
                    }
                }
            }
            return frames.Where(q => q.Width != 0 && q.Height != 0).ToArray();
        }
    }
}

