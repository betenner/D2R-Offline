using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using D2ROffline.Tools;

namespace D2ROffline
{
    public class Program : NativeMethods
    {
        public static string version = "v2.0.8";

        static void Main(string[] args)
        {
            // print ASCII logo (PNG's are overrated, change my mind)
            PrintASCIIArt();

            // arguemnts logic
            if (!HandlerArgs(args))
            {
                ConsolePrint("Press any key to exit...", ConsoleColor.Yellow);
                Console.ReadKey();
                return;
            }

            ConsolePrint("Done!", ConsoleColor.Green);
#if !DEBUG
            ConsolePrint("Press any key to exit...", ConsoleColor.Yellow);
            Console.ReadKey();
#endif
        }

        private static bool HandlerArgs(string[] args)
        {
            string gameArgs = "";
            int crashDelay = 25;
            if (args.Length > 0)
            {
                if(args.Length > 1)
                    gameArgs = args[1]; // may gets overwritten somwhere else
                if (args[0].Equals("-FixLocalSave", StringComparison.InvariantCultureIgnoreCase))
                {
                    // hande FixLocalSave
                    SaveFilePatcher.PatchSaveFiles(args.ElementAtOrDefault(1));
                    return true;
                }
                else if (args[0].Equals("-UpdateKeyBinds", StringComparison.InvariantCultureIgnoreCase))
                {
                    // hande UpdateKeyBinds
                    KeyBindingSync.SyncKeyBindings(args.ElementAtOrDefault(1));
                    return false;
                }
                else if (args.Length > 1 && args[1].Equals("-Delay", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Handle Delay
                    if(!Int32.TryParse(args[2], out crashDelay))
                    {
                        Console.WriteLine("Bad argument for -Delay");
                        return false;
                    }
                    ConsolePrint($"Delay has been set to: {crashDelay}ms", ConsoleColor.DarkYellow);

                    // if user is setting a custom delay value and specifying game.exe path
                    if (args.Length > 3)
                        gameArgs = args[3];
                }
                // launch with extra CLI options
                return Patcher.Start(args[0], crashDelay, gameArgs);
            }
            // launch with default settings
            return Patcher.Start();
        }

        private static void PrintASCIIArt()
        {
            // print chars individual to set color
            // NOTE: thanks @pairofdocs for ASCII art (https://github.com/ferib/D2R-Offline/pull/4)
            string Diablo2ASCII = "                                        ..\n                                  .,.  .*,\n                             ,.  ./ (((#(*                                ,\n                            *(*, */#%#/*,                                */.        ..\n                           .* *##(#%((//.         ..   ,.               ./#*.  .,.   *((/.\n                            , *(%%#%(/*. .,.    .**,.**.        .        ,(%%%#(%(.  *%#*\n   ...                        ./ &%%%/*,./(* ,/(///((/*.       ,.         .*#%#%%%(#%&#*\n   ,,,.                        .*%.%%%%#%(/**#%##%#*    /.  ,/(. *,.,*****(#%%%%%&,%/.                   .,*,\n   ,/(*,.                        ,/&  /&%%%%%%%%%%#((((#/   ,%#((##%##%%%%%%%&,  #*                     ,*((*,.\n    **%%#/**,,.                    .*%             .%((*    ./(%              *%/                    ..,/#%#/,,.\n  ,(%%%%#((#%((/(*.           .,    */#&         .%(/          ,*(*          #,                    .,*(%&&&%%#(,,.\n ., (&           / (/*        .,/*.    ,*##      .,(#(*          .*/#         %*      ..,           ,*#(         ##/,\n  .*#,  &(**(%&   &/,      /(#(**,.   *(%   ,,**#(*/.       .,*(%*(,,,      /,    ,*/#,          ./#  .%(**/%    &(/\n  , ((, &/,,/##&   &*     .*&  .*(,.  ,/% .,///% .%//       ,(* ,,,**#//    (/  ,((###%,         *#  ,%#/,. ,(%   &(,\n  *((, &/,, *#(%   #(      ,/&  *(*,   /& ,./( /%&. &*      .,(.  %#  .(,   %*  .(#  &(/        *%   @#(#*.. ,#.   #,\n  ,/#,  &/,.,(#&   #(.    .,(%  */*.  .*& .*(  &##&  %/,     *(.  %%,  &,.  #   *(#  &%*(,,     /%   /#(#/,  ,#(   #,\n  , (%, &/, ..*#,   %*     .,(&  *#*,  ./& ,#.  &(/#  /(,    ./#.  @&   %*   (.  /(#  @&&#,#.    ,(,   &##*. ./%.  //.\n ./#&.  &%#%%@    &*.      ,(((((#/,  *(& */**/**/%  &*      ***,,/,,*.     /.   /((((((%(.      *(.   &%/*/#%,  *(.\n.*#&            @(,                   */&        #. %,           *%         /*                    */%.         ,#*\n, (* @%#####((/*,                  ., ,*#&       # &*..           ./         %,                       ,/(#&%%#/,\n,#(,                             .(///(%#     ((,/.*#.          ,,(         %*\n                                .((%#%%,          /%%(/*       .(&           &##/,\n                                ./#&&&&&&&&&&&&&&&&&&%%*,   *&%%%%%%%%%%%%%%%%%%%&(                ~ Ferib Hellscream";
            Dictionary<char, ConsoleColor> colorMapping = new Dictionary<char, ConsoleColor>()
            {
                { '.', ConsoleColor.Yellow },
                { ',', ConsoleColor.Yellow },
                { '*', ConsoleColor.Yellow },
                { '%', ConsoleColor.DarkYellow },
                { '#', ConsoleColor.DarkYellow },
                { '@', ConsoleColor.DarkYellow },
                { '/', ConsoleColor.DarkYellow },
                { '(', ConsoleColor.DarkYellow },
                { '&', ConsoleColor.DarkYellow },
            };

            ConsoleColor oldf = Console.ForegroundColor;
            ConsoleColor oldb = Console.BackgroundColor;
            ConsoleColor lastColor = oldf;
            string buffer = "";
            for (int i = 0; i < Diablo2ASCII.Length; i++)
            {

                ConsoleColor currentColor;
                if (colorMapping.TryGetValue(Diablo2ASCII[i], out currentColor))
                {
                    if (currentColor != lastColor)
                    {
                        lastColor = colorMapping[Diablo2ASCII[i]];
                        Console.Write(buffer);
                        buffer = "";
                        Console.ForegroundColor = lastColor;
                    }
                }
                buffer += Diablo2ASCII[i];
            }
            Console.WriteLine(buffer);

            // print footer
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"			                                       _____  _______ _______ ____" +
                $"___ _     _ _______  ______ \n {Program.version.PadRight(16)}			                " +
                $"      |_____] |_____|    |    |       |_____| |______ |_____/ \n___");
            Console.WriteLine($"_____________________" +
                $"_________________________" +
                $"_____________|       |     |    |   " +
                $" |_____  |     | |______ |    \\_ \n");
            Console.ForegroundColor = ConsoleColor.Gray;


            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = oldf;
            Console.BackgroundColor = oldb;
        }
        public static void ConsolePrint(string str, ConsoleColor color = ConsoleColor.Gray)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fffff")}]: {str}");
            Console.ForegroundColor = old;
        }

    }
}