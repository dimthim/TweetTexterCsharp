using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Runtime.CompilerServices;

namespace TwitterSharp
{
    public class GlobalVars
    {
        
        public static string PhoneNumber = "";
        public static string TwitterHandle = "";
        public static string RandoDingen = "Your Email password here";
        public static int Provider = 0;
    }


    static class Program
    {
        static int StringToDigit(string StrDigit)
        {
            int IntDigit = 0;
            switch (StrDigit)
            {
                case "0": IntDigit = 0; break;
                case "1": IntDigit = 1; break;
                case "2": IntDigit = 2; break;
                case "3": IntDigit = 3; break;
            };

            return (IntDigit);
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //User chooses cellular provider from a dropdown menu.
            //Have app run as server so that user can simply text the running app twitter handle parameters to change who they want to get text alerts about
            //Save multiple user data, have them select their name from a dropdown menu, and autofill the text boxes accordingly

            string PresetsPath = "UserAddresses.txt";
            string UserPresets = "";
            

            if (File.Exists(PresetsPath))
            {
                UserPresets = File.ReadAllText(PresetsPath);

                if (UserPresets.Length > 5)
                {
                    string[] UserDataChunks = UserPresets.Split(' ');
                    GlobalVars.TwitterHandle = UserDataChunks[0];
                    GlobalVars.PhoneNumber = UserDataChunks[1];
                    GlobalVars.Provider = StringToDigit(UserDataChunks[2]);
                }
            }
            else
            {
                File.Create(PresetsPath);
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new OpeningMenu());
            UserPresets = GlobalVars.TwitterHandle + ' ' + GlobalVars.PhoneNumber + ' ' + GlobalVars.Provider;
            File.WriteAllText(PresetsPath, UserPresets);

        }
    }
}
