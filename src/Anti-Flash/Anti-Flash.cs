using Swed64;
class AntiFlash {
    static void Main() {
        Swed swed = new Swed("cs2");

        IntPtr client = swed.GetModuleBase("client.dll");

        while (true) {
            IntPtr localPlayerPawn = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);

            float flashDuration = swed.ReadFloat(localPlayerPawn, Offsets.m_flFlashBangTime); 

            if (flashDuration > 0) {
                swed.WriteFloat(localPlayerPawn, Offsets.m_flFlashBangTime, 0);
                Console.WriteLine("Avoided Flash!"); 
            }
            Thread.Sleep(2);
        }
    }   

}