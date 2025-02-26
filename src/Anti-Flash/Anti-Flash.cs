using Swed64;
class AntiFlash {
    static void Main() {
        Swed swed = new Swed("cs2");

        IntPtr client = swed.GetModuleBase("client.dll");

        // offsets
        int dwLocalPlayerPawn = 0x1889F20;
        int m_flFlashBangTime = 0x13F8;

        while (true) {
            IntPtr localPlayerPawn = swed.ReadPointer(client, dwLocalPlayerPawn);

            float flashDuration = swed.ReadFloat(localPlayerPawn, m_flFlashBangTime); 

            if (flashDuration > 0) {
                swed.WriteFloat(localPlayerPawn, m_flFlashBangTime, 0);
                Console.WriteLine("Avoided Flash!"); 
            }
            Thread.Sleep(2);
        }
    }   

}