using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace subtitles
{
    class Program
    {
        public static readonly string minTime= "00:00:00,000";
        public static string currentTime = "00:00:00,000";
        static DateTime time= DateTime.ParseExact(currentTime, "HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture);

        public static string AddTime(string t)
        {
            DateTime tt = DateTime.ParseExact(t, "HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            tt=tt.AddMinutes(time.Minute);
            tt=tt.AddSeconds(time.Second);
            tt=tt.AddMilliseconds(time.Millisecond);
            return tt.ToString("HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture); ;
        }

        public static void IncrementTime()
        {
            time = time.AddSeconds(4);
            currentTime = time.ToString("HH:mm:ss,fff", System.Globalization.CultureInfo.InvariantCulture);
        }

        static void Main(string[] args)
        {
            string filename = "";
            string filepath = args[0];
            string root = Path.GetDirectoryName(filepath);
            
            string[] files = Directory.GetFiles(root, "*.mp4");
            if (files.Any())
            {
                filename=Path.GetFileNameWithoutExtension(files[0]);
            }
            string line;
            int cnt = 1;
            Regex regex = new Regex(@"(\d+:\d+:\d+.\d*) --> (\d+:\d+:\d+.\d*)");

            var tmpPath = root + @"\" + "tmp";
            

            StreamReader input = new StreamReader(filepath, Encoding.UTF8);
            //StreamWriter output = new StreamWriter(root + @"\" + filename + ".srt");
            StreamWriter output = new StreamWriter(
                                    new FileStream(root + @"\" + filename + ".srt", FileMode.Open, FileAccess.Write),
                                    Encoding.UTF8 );
            //StreamWriter tmp = new StreamWriter(tmpPath);
            StreamWriter tmp = new StreamWriter(
                                    new FileStream(tmpPath, FileMode.Open, FileAccess.Write),
                                    Encoding.UTF8);

            bool isPrevWhiteline = false;

            while ((line = input.ReadLine()) != null)
            {
                if (line == "WEBVTT")
                {
                    continue;
                }
                if (line.StartsWith("X-TIMESTAMP-MAP=MPEGTS"))
                {
                    IncrementTime();
                    continue;
                }

                Match match = regex.Match(line);
                if (match.Success)
                {
                    var ma1 = match.Groups[1].Value;
                    var ma2 = match.Groups[2].Value;

                    line = line.Replace(ma1, AddTime(ma1));
                    line = line.Replace(ma2, AddTime(ma2));

                }
                tmp.WriteLine(line);
                cnt++;
            }
            tmp.Close();
            cnt = 1;
            StreamReader tmp1 = new StreamReader(tmpPath, Encoding.UTF8);
            while ((line = tmp1.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || string.IsNullOrEmpty(line))
                {
                    if (!isPrevWhiteline)
                    {
                        output.WriteLine(line);
                        output.WriteLine(cnt);
                        cnt++;
                    }
                    isPrevWhiteline = true;
                }
                else
                {
                    output.WriteLine(line);
                    isPrevWhiteline = false; ;
                }

            }
            tmp1.Close();
            output.Close();
            tmp.Close();
            File.Delete(tmpPath);

            Console.WriteLine($"wrote : {cnt} lines");
            Console.ReadLine();

        }
    }
}
