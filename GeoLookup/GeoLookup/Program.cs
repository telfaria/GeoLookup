using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MaxMind.GeoIP2.Responses;
using Newtonsoft.Json.Linq;

namespace GeoLookup
{
    class Program
    {
        private const string GEOLOCATION_COUNTRY_DB_FILE_GZip = "GeoLite2-Country.mmdb.gz";
        private const string GEOLOCATION_COUNTRY_DB_FILE = "GeoLite2-Country.mmdb";

        private const string GEOLOCATION_CITY_DB_FILE_GZip = "GeoLite2-City.mmdb.gz";
        private const string GEOLOCATION_CITY_DB_FILE = "GeoLite2-City.mmdb";


        private const string GEOLOCATION_COUNTRY_DB_DOWNLOAD_SITE =
            "http://geolite.maxmind.com/download/geoip/database/GeoLite2-Country.mmdb.gz";

        private const string GEOLOCATION_CITY_DB_DOWNLOAD_SITE =
            "http://geolite.maxmind.com/download/geoip/database/GeoLite2-City.mmdb.gz";



        static void Main(string[] args)
        {

            List<string> ipList = new List<string>();

            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }
            
            //入力をファイル名として
            string infile = args[0];

            //入力がIPアドレスかどうかの判定
            //IPアドレスだったら判定まで行く

            IPAddress address;
            if (System.Net.IPAddress.TryParse(infile, out address))
            {
                ipList.Add(infile);
            }
            else
                {
                    if (System.IO.File.Exists(infile) == false)
                    {
                        Console.WriteLine("infile: {0} is not found.", infile);
                        return;
                    }

                    //read IP list

                    string[] l = System.IO.File.ReadAllLines(infile);

                    foreach (var s in l)
                    {
                        ipList.Add(s);
                    }
            }


            //show notice.
            ShowUsage();
            //ShowNotice();

            DownloadGeoLocaionDb();


            //geolocate 

            foreach (string ipaddr in ipList)
            {
                System.Net.IPAddress ip = System.Net.IPAddress.Parse(ipaddr);

                //var countryResponse = GeoLocate_GetCountry(ip);

                var cityResponce = GeoLocate_GetCity((ip));

                Console.WriteLine("{0} -> {1}, {2}",ip.ToString(), cityResponce.City.Name,  cityResponce.Country.Name);

                
            }


            //Console.ReadLine();
        }

        private static void ShowNotice()
        {
            Console.WriteLine("------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("この製品には MaxMind が作成した GeoLite2 データが含まれており、http://www.maxmind.com から入手いただけます。");
            Console.WriteLine("------------------------------------------------------------------------------------------------------------");
        }

        static void ShowUsage()
        {
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine(" IP Address lookup Geolocation");
            Console.WriteLine("  Usage: GetLookup [IP ADDRESS | Lookup List file]");
            Console.WriteLine("-------------------------------------------------------------");
            ShowNotice();

        }

        static void DownloadGeoLocaionDb()
        {
            
            //Country DB  Exist Check, Download,Decompless.
            if (System.IO.File.Exists(GEOLOCATION_COUNTRY_DB_FILE) == false)
            {
                Console.WriteLine("Geolocation Country DB file not found.");
                Console.WriteLine("Geolocation Country DB Download...");

                var wc = new System.Net.WebClient();
                wc.DownloadFile(GEOLOCATION_COUNTRY_DB_DOWNLOAD_SITE,GEOLOCATION_COUNTRY_DB_FILE_GZip);
                Console.WriteLine("Geolocation Country DB Download Complete.");

                //and unzip
                GZdecompress(GEOLOCATION_COUNTRY_DB_FILE_GZip);
            }

            //City DB  Exist Check, Download,Decompless.
            if (System.IO.File.Exists(GEOLOCATION_CITY_DB_FILE) == false)
            {
                Console.WriteLine("Geolocation DB file not found.");
                Console.WriteLine("Geolocation DB Download...");

                var wc = new System.Net.WebClient();
                wc.DownloadFile(GEOLOCATION_CITY_DB_DOWNLOAD_SITE, GEOLOCATION_CITY_DB_FILE_GZip);
                Console.WriteLine("Download Complete.");

                //and unzip
                GZdecompress(GEOLOCATION_CITY_DB_FILE_GZip);
            }
        }



        static CountryResponse GeoLocate_GetCountry(System.Net.IPAddress ipaddress)
        {
            using (var mm = new MaxMind.GeoIP2.DatabaseReader(GEOLOCATION_COUNTRY_DB_FILE))
            {
                var responce = mm.Country(ipaddress);

                return responce;

            }
        }

        static CityResponse GeoLocate_GetCity(System.Net.IPAddress ipaddress)
        {
            using (var mm = new MaxMind.GeoIP2.DatabaseReader(GEOLOCATION_CITY_DB_FILE))
            {
                var responce = mm.City(ipaddress);

                return responce;
            }
        }


        //decompress method from http://www.atmarkit.co.jp/fdotnet/dotnettips/485gzipstream/gzipstream.html

        static void GZdecompress(string gzFile )
        {

            string inFile = gzFile;

            // 入力ファイルは.gzファイルのみ有効
            if (!inFile.ToLower().EndsWith(".gz"))
            {
                return;
            }

            // ファイル名末尾の「.gz」を削除
            string outFile = inFile.Substring(0, inFile.Length - 3);

            int num;
            byte[] buf = new byte[1024]; // 1Kbytesずつ処理する

            FileStream inStream // 入力ストリーム
                = new FileStream(inFile, FileMode.Open, FileAccess.Read);

            GZipStream decompStream // 解凍ストリーム
                = new GZipStream(
                    inStream, // 入力元となるストリームを指定
                    CompressionMode.Decompress); // 解凍（圧縮解除）を指定

            FileStream outStream // 出力ストリーム
                = new FileStream(outFile, FileMode.Create);

            using (inStream)
            using (outStream)
            using (decompStream)
            {
                while ((num = decompStream.Read(buf, 0, buf.Length)) > 0)
                {
                    outStream.Write(buf, 0, num);
                    Console.Write(".");
                }
            }
            Console.WriteLine("decompress complete.");

        }


    }
}
