using Swed64;

using CS2Cheats.Features;

namespace CS2Cheats
{
    class Cheats
    {  
        // cancellation token sources for each task
        private static CancellationTokenSource _antiFlashCTS;
        private static CancellationTokenSource _radarCTS;
        private static CancellationTokenSource _bhopCTS;
        private static CancellationTokenSource _triggerbotCTS;
        private static CancellationTokenSource _aimbotCTS;
        private static CancellationTokenSource _fovAimbotCTS;

        public static async Task initOffsets()
        {
            await Utils.Offsets.UpdateOffsets();
        }


        static void Main()
        {
            // init Swed
            Swed swed;

            // check if CS2 is running
            try
            {
                swed = new Swed("cs2");
            } catch (Exception e) {
                Console.WriteLine("CS2 process is not running!");
                return;
            }


            // init offsets every time this program is run (offsets change almost weekly)
            Task.Run(async () => await initOffsets()).Wait();

            // get client.dll base address
            IntPtr client = swed.GetModuleBase("client.dll");

            // init ImGui and overlay
            Renderer renderer = new Renderer();
            renderer.Start().Wait();

            // main loop
            while (true)
            {
                // initialization of other cheats

                // antiFlash
                // if antiFlash checkbox was turned on for the very first time, meaning the task is not running(cts is null) but the checkbox is on, so we wanna start the task (this is only being executed once).
                if (renderer.antiFlash && _antiFlashCTS == null)
                {
                    _antiFlashCTS = new CancellationTokenSource(); // create a new cts indicating status of task (running or not)
                    Task.Run(() => AntiFlashC.AntiFlash(swed, client, _antiFlashCTS.Token));
                    renderer.antiFlashRunning = 1; 
                }
                
                // if (checkbox is not checked and there is no task running) | (checkbox is checked and task is running) => We dont need to do anything
                if (!renderer.antiFlash && renderer.antiFlashRunning == 0 || renderer.antiFlash && renderer.antiFlashRunning == 1) { }

                // if the antiFlash checkbox was *just* turned off but the task is still running so we wanna cancel the task and toggle antiFlashRunning
                else if (!renderer.antiFlash && _antiFlashCTS != null)
                {
                    _antiFlashCTS.Cancel();
                    _antiFlashCTS.Dispose();
                    renderer.antiFlashRunning = 0;
                }

                // if the checkbox was turned on again after it being turned off, we have to cancel the old task, dispose the cts and start a new one
                else if (renderer.antiFlash && _antiFlashCTS != null)
                {   
                    renderer.antiFlashRunning = 1;
                    _antiFlashCTS = new CancellationTokenSource();
                    Task.Run(() => AntiFlashC.AntiFlash(swed, client, _antiFlashCTS.Token));
                }

                // radar
                // same logic as antiFlash. 
                if (renderer.radar && _radarCTS == null)
                {
                    _radarCTS = new CancellationTokenSource();
                    Task.Run(() => RadarC.Radar(swed, client, _radarCTS.Token));
                    renderer.radarRunning = 1;
                }

                if (!renderer.radar && renderer.radarRunning == 0 || renderer.radar && renderer.radarRunning == 1) { }

                else if (!renderer.radar && _radarCTS != null)
                {
                    _radarCTS.Cancel();
                    _radarCTS.Dispose();
                    renderer.radarRunning = 0;
                }

                else if (renderer.radar && _radarCTS != null)
                {
                    renderer.radarRunning = 1;
                    _radarCTS = new CancellationTokenSource();
                    Task.Run(() => RadarC.Radar(swed, client, _radarCTS.Token));
                }

                // bhop
                // you get it. 
                if (renderer.bhop && _bhopCTS == null)
                {
                    _bhopCTS = new CancellationTokenSource();
                    Task.Run(() => BhopC.Bhop(swed, client, _bhopCTS.Token));
                    renderer.bhopRunning = 1;
                }

                if (!renderer.bhop && renderer.bhopRunning == 0 || renderer.bhop && renderer.bhopRunning == 1) { }

                else if (!renderer.bhop && _bhopCTS != null)
                {
                    _bhopCTS.Cancel();
                    _bhopCTS.Dispose();
                    renderer.bhopRunning = 0;
                }

                else if (renderer.bhop && _bhopCTS != null)
                {
                    renderer.bhopRunning = 1;
                    _bhopCTS = new CancellationTokenSource();
                    Task.Run(() => BhopC.Bhop(swed, client, _bhopCTS.Token));
                }

                // triggerbot
                if (renderer.triggerbot && _triggerbotCTS == null)
                {
                    _triggerbotCTS = new CancellationTokenSource();
                    Task.Run(() => TriggerbotC.Triggerbot(swed, client, _triggerbotCTS.Token));
                    renderer.triggerbotRunning = 1;
                }

                if (!renderer.triggerbot && renderer.triggerbotRunning == 0 || renderer.triggerbot && renderer.triggerbotRunning == 1) { }

                else if (!renderer.triggerbot && _triggerbotCTS != null)
                {
                    _triggerbotCTS.Cancel();
                    _triggerbotCTS.Dispose();
                    renderer.triggerbotRunning = 0;
                }
                else if (renderer.triggerbot && _triggerbotCTS != null)
                {
                    renderer.triggerbotRunning = 1;
                    _triggerbotCTS = new CancellationTokenSource();
                    Task.Run(() => TriggerbotC.Triggerbot(swed, client, _triggerbotCTS.Token));
                }

                // aimbot
                if (renderer.aimbot && _aimbotCTS == null)
                {
                    _aimbotCTS = new CancellationTokenSource();
                    Task.Run(() => AimbotC.Aimbot(renderer, swed, client, _aimbotCTS.Token));
                    renderer.aimbotRunning = 1;
                }

                if (!renderer.aimbot && renderer.aimbotRunning == 0 || renderer.aimbot && renderer.aimbotRunning == 1) { }

                else if (!renderer.aimbot && _aimbotCTS != null)
                {
                    _aimbotCTS.Cancel();
                    _aimbotCTS.Dispose();
                    renderer.aimbotRunning = 0;
                }

                else if (renderer.aimbot && _aimbotCTS != null)
                {
                    renderer.aimbotRunning = 1;
                    _aimbotCTS = new CancellationTokenSource();
                    Task.Run(() => AimbotC.Aimbot(renderer, swed, client, _aimbotCTS.Token));
                }

                // fov aimbot
                if (renderer.fovAimbot && _fovAimbotCTS == null)
                {
                    _fovAimbotCTS = new CancellationTokenSource();
                    Task.Run(() => AimbotC.Aimbot(renderer, swed, client, _fovAimbotCTS.Token));
                    renderer.fovAimbotRunning = 1;
                }

                if (!renderer.fovAimbot && renderer.fovAimbotRunning == 0 || renderer.fovAimbot && renderer.fovAimbotRunning == 1) { }

                else if (!renderer.fovAimbot && _fovAimbotCTS != null)
                {
                    _fovAimbotCTS.Cancel();
                    _fovAimbotCTS.Dispose();
                    renderer.fovAimbotRunning = 0;
                }

                else if (renderer.fovAimbot && _fovAimbotCTS != null)
                {
                    renderer.fovAimbotRunning = 1;
                    _fovAimbotCTS = new CancellationTokenSource();
                    Task.Run(() => AimbotC.Aimbot(renderer, swed, client, _fovAimbotCTS.Token));
                }

                Thread.Sleep(10);
            }
        }
    }
}
