using System;
using System.Threading;
using System.Threading.Tasks;
using DiCore.Lib.NDT.DataProviders.CDPA;

namespace Diascan.TestMemoryAllocator
{
    public class TestMemoryAllocator
    {
        private CancellationTokenSource cts;
        public event Action<string> CallBack;                      //  отчет 
        public event Action<string, ConsoleColor> CallBackColor;   //  отчет Color
        private Task[] testMemoryTasks;
        private float[] data;
        public TestMemoryAllocator()
        {
            testMemoryTasks = new Task[5];
            data = new float[]{50.23f, 42.15f , 30.13f , 20.38f , 16.73f , 16.56f , 20.49f , 30.14f , 40.76f , 50.84f};
        }

        private unsafe void СreatureCDPAEcho(CDPASensorData* ptrCdpaSensorData)
        {
            var echos = ptrCdpaSensorData->Echos;
            var random = new Random();
            for (var j = 1; j < ptrCdpaSensorData->EchoCount; j++)
            {
                echos++;
                echos->Amplitude = data[random.Next(0, 9)];
                echos->Time      = data[random.Next(0, 9)];
            }
        }

        private unsafe void СreatureCDPASensorData(CDPASensorItem * ptrCDPASensorItem, DataHandleCDpa resource)
        {
            var ptrCdpaSensorData = ptrCDPASensorItem->Data;
            var random = new Random();
            for (var i = 0; i < ptrCDPASensorItem->RayCount; i++)
            {
                ptrCdpaSensorData->RuleId = (ushort)random.Next(1, 256);
                ptrCdpaSensorData->EchoCount = (ushort)random.Next(32, 64);
                ptrCdpaSensorData->Echos = resource.Allocate<CDPAEcho>(ptrCdpaSensorData->EchoCount);

                СreatureCDPAEcho(ptrCdpaSensorData);

                ptrCdpaSensorData++;
            }

        }

        private unsafe void СreatureCDPASensorItem(int row, int col, DataHandleCDpa resource)
        {
            var ptr = resource.GetDataPointer(row, col);
            ushort rayCount = 4;
            ptr->RayCount = rayCount;

            ptr->Data = resource.Allocate<CDPASensorData>(ptr->RayCount);

            СreatureCDPASensorData(ptr, resource);
        }


        private unsafe void Work(CancellationToken ct, ConsoleColor consoleColor)
        {
            while (true)
            {
                if (ct.IsCancellationRequested) break;
                try
                {
                    using (var resource = new DataHandleCDpa(256, 2000))
                    {
                        CallBackColor($"Память выделена", consoleColor);
                        var random = new Random();
                        for (var row = 0; row < resource.RowCount; row++)
                        {
                            for (var col = 0; col < resource.ColCount; col++)
                            {
                                СreatureCDPASensorItem(row, col, resource);
                            }
                        }
                        CallBackColor($"Память заполнина", consoleColor);
                        var sleep = random.Next(10000, 15000);
                        CallBackColor($"Поток засыпает  {sleep}c", consoleColor);
                        Thread.Sleep(sleep);
                        CallBackColor($"Поток проснулся", consoleColor);
                    }
                }
                catch (Exception e)
                {
                    CallBack(e.ToString());
                    throw;
                }
            }
        }

        private bool WorkManage(CancellationTokenSource cts)
        {
            try
            {
                for (var i = 0; i < testMemoryTasks.Length; i++)
                {
                    var random = new Random();
                    var consoleColor = (ConsoleColor)i + random.Next(2, 10);
                    testMemoryTasks[i] = Task.Factory.StartNew(() => Work(cts.Token, consoleColor), cts.Token);
                }

                Task.WaitAll(testMemoryTasks);
            }
            catch (Exception e)
            {
                CallBack($"{e}");
                return false;
            }
            return true;
        }

        private async Task<T> ProcessTask<T>(Func<CancellationTokenSource, T> task)
        {
            var res = default(T);

            try
            {
                cts = new CancellationTokenSource();

                res = await Task.Factory.StartNew(() => task(cts));

            }
            catch (Exception e)
            {
                CallBack($"{e}");
                return await Task.FromResult(res);
            }

            return await Task.FromResult(res);
        }

        public void Stop()
        {
            if (cts == null || cts.IsCancellationRequested) return;
            CallBack($"Stop");
            cts.Cancel(false);
        }

        public async void Start()
        {
            var res = await ProcessTask(WorkManage);
            Console.Clear();
            CallBack($"Завершил работу.");
        }
    }
}
