using System;
using System.Collections.Generic;
using System.Text;

namespace getCshCode.Utility
{
    public class ioControl
    {
        public static string GetSpan(string line, char span, int spanCnt)
        {
            var spanLine = line.AsSpan();
            for (int i = 0; i < spanLine.Length; i++)
            {
                if (spanLine[i] == span)
                {
                    int start = i + 1;
                    int end = spanLine.Slice(start).IndexOf(span) + start;
                    if (end > start)
                    {
                        if (--spanCnt == 0) return spanLine.Slice(start, end - start).ToString();
                    }
                }
            }
            return spanLine.ToString();

        }
    }
}
