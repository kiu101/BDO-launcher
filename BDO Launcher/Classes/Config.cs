using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using System.IO;

namespace BDO_Launcher
{

    
    public class Config
    {


        FileIniDataParser parser = new FileIniDataParser();
        public IniData data = new IniData();


        //hardcoded stuff
        public string apiURL = "http://localhost";
        public string authURL = "api/authenticate";


        //loaded from ini
        public string gameDirectory = @"\";
        public string gameBin64Directory = System.IO.Directory.GetCurrentDirectory(); 
        public string gameFilename = "BlackDesert64.exe";
        public string updateUrl = "/update";

        public string version = "0";
        public string index = "0";


        public Config()
        {
            //checkif exists 

            if ( !File.Exists("bdo.ini"))
            {
                MakeIniFile();
            }

            data = parser.ReadFile("bdo.ini");

            this.gameDirectory = data["game"]["gamedirectory"];
            this.gameBin64Directory = data["game"]["bin64directory"];
            this.gameFilename = data["game"]["gamefilename"];


            this.apiURL = data["api"]["apiurl"];
            this.authURL = data["api"]["authurl"];
            this.updateUrl = data["api"]["updateurl"];

            this.version = data["updater"]["version"];
            this.index = data["updater"]["index"];

        }

        private void MakeIniFile()
        {
            var parser = new FileIniDataParser();
            IniData data = new IniData();

            data["game"]["gamedirectory"] = this.gameDirectory;
            data["game"]["bin64directory"] = this.gameBin64Directory;
            data["game"]["gamefilename"] = this.gameFilename;


            data["api"]["apiurl"] = this.apiURL;
            data["api"]["authurl"] = this.authURL;
            data["api"]["updateurl"] = this.updateUrl;

            data["updater"]["version"] = this.version;
            data["updater"]["index"] = this.index;

            parser.WriteFile("bdo.ini", data);

        }

        public void SaveIniFile()
        {
            parser.WriteFile("bdo.ini", data);
        }

    }
}
