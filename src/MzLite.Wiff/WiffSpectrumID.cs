using System;
using System.Text.RegularExpressions;

namespace MzLite.Wiff
{
    internal struct WiffSpectrumID
    {

        public WiffSpectrumID(int experiment, int scan)
            : this()
        {

            if (experiment < 0)
                throw new ArgumentOutOfRangeException("experiment");
            if (scan < 0)
                throw new ArgumentOutOfRangeException("scan");

            Experiment = experiment;
            Scan = scan;
        }

        public int Experiment { get; private set; }
        public int Scan { get; private set; }

        public static string ToSpectrumID(int experiment, int scan)
        {
            if (experiment < 0)
                throw new ArgumentOutOfRangeException("experiment");
            if (scan < 0)
                throw new ArgumentOutOfRangeException("scan");

            return string.Format("experiment={0} scan={1}", experiment, scan);
        }

        static readonly Regex regexID = new Regex(@"experiment=(\d+) scan=(\d+)", RegexOptions.Compiled | RegexOptions.ECMAScript);

        public static WiffSpectrumID Parse(string spectrumID)
        {

            if (string.IsNullOrWhiteSpace(spectrumID))
                throw new ArgumentNullException("nativeID");

            Match match = regexID.Match(spectrumID);

            if (match.Success)
            {
                try
                {
                    GroupCollection groups = match.Groups;
                    int experiment = int.Parse(groups[1].Value);
                    int scan = int.Parse(groups[2].Value);

                    return new WiffSpectrumID(experiment, scan);
                }
                catch (Exception ex)
                {
                    throw new FormatException("Error parsing wiff spectrum id format: " + spectrumID, ex);
                }
            }
            else
            {
                throw new FormatException("Not a valid wiff spectrum id format: " + spectrumID);
            }

        }

        static readonly Regex regexSampleIndex = new Regex(@"sample=(\d+)", RegexOptions.Compiled | RegexOptions.ECMAScript);

        public static int ParseWiffSampleIndex(string runID)
        {

            if (string.IsNullOrWhiteSpace(runID))
                throw new ArgumentNullException("runID");

            Match match = regexSampleIndex.Match(runID);

            if (match.Success)
            {
                try
                {
                    GroupCollection groups = match.Groups;
                    return int.Parse(groups[1].Value);
                }
                catch (Exception ex)
                {
                    throw new FormatException("Error parsing wiff sample index: " + runID, ex);
                }
            }
            else
            {
                throw new FormatException("Not a valid wiff sample index format: " + runID);
            }

        }

        public static string ToRunID(int sample)
        {
            return string.Format("sample={0}", sample);
        }
    }
}
