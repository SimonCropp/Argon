﻿public static class ModuleInit
{
    [ModuleInitializer]
    public static void Init() =>
        VerifierSettings.IgnoreStackTrace();
}