using System;
using System.Text.RegularExpressions;

namespace MzLite.Wiff
{
    /// <summary>
    /// The wiff native id format as defined in PSI-MS, term id 'MS:1000770'.
    /// </summary>
    public struct WiffNativeID
    {

        public static readonly string TERM_ACCESSION = "MS:1000770";

        public WiffNativeID(int sample, int period, int cycle, int experiment)
            : this()
        {

            if (sample < 0)
                throw new ArgumentOutOfRangeException("sample");
            if (period < 0)
                throw new ArgumentOutOfRangeException("period");
            if (cycle < 0)
                throw new ArgumentOutOfRangeException("cycle");
            if (experiment < 0)
                throw new ArgumentOutOfRangeException("experiment");

            Sample = sample;
            Period = period;
            Cycle = cycle;
            Experiment = experiment;

        }

        public int Sample { get; private set; }
        public int Period { get; private set; }
        public int Cycle { get; private set; }
        public int Experiment { get; private set; }

        public static string ToString(int sample, int period, int cycle, int experiment)
        {
            if (sample < 0)
                throw new ArgumentOutOfRangeException("sample");
            if (period < 0)
                throw new ArgumentOutOfRangeException("period");
            if (cycle < 0)
                throw new ArgumentOutOfRangeException("cycle");
            if (experiment < 0)
                throw new ArgumentOutOfRangeException("experiment");

            return string.Format("sample={0} period={1} cycle={2} experiment={3}", sample, period, cycle, experiment);
        }

        static readonly Regex regexID = new Regex(@"sample=(\d+) period=(\d+) cycle=(\d+) experiment=(\d+)", RegexOptions.Compiled | RegexOptions.ECMAScript);

        public static WiffNativeID Parse(string nativeID)
        {

            if (string.IsNullOrWhiteSpace(nativeID))
                throw new ArgumentNullException("nativeID");

            Match match = regexID.Match(nativeID);

            if (match.Success)
            {
                try
                {
                    GroupCollection groups = match.Groups;
                    int sample = int.Parse(groups[1].Value);
                    int period = int.Parse(groups[2].Value);
                    int cycle = int.Parse(groups[3].Value);
                    int experiment = int.Parse(groups[4].Value);
                    return new WiffNativeID(sample, period, cycle, experiment);
                }
                catch (Exception ex)
                {
                    throw new FormatException("Error parsing wiff native id format: " + nativeID, ex);
                }
            }
            else
            {
                throw new FormatException("Not a valid wiff native id format: " + nativeID);
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

        public override string ToString()
        {
            return WiffNativeID.ToString(Sample, Period, Cycle, Experiment);
        }

        public override bool Equals(object obj)
        {

            if (Object.ReferenceEquals(this, obj))
                return true;

            WiffNativeID other = (WiffNativeID)obj;

            return Sample.Equals(other.Sample) &&
                Period.Equals(other.Period) &&
                Cycle.Equals(other.Cycle) &&
                Experiment.Equals(other.Experiment);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Sample, Period, Cycle, Experiment).GetHashCode();
        }

    }
}
