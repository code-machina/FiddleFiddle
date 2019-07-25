using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiddleFiddle
{
    /// <summary>
    /// FiddleFiddle 에게 일관된 시간을 제공
    /// 시간 포맷을 변경하고자 할 때 ??
    /// </summary>
    public static class FiddleFiddleTime
    {
        // 참고
        // https://docs.microsoft.com/ko-kr/dotnet/standard/base-types/custom-date-and-time-format-strings
        public const string FiddleTimeStampFormat = "{0:yyyyddMMTH:mm:sszzz}";
        public static Now Time = new Now();

        public class Now
        {
            public override String ToString()
            {
                return String.Format(FiddleTimeStampFormat, DateTime.Now);
            }
        }
    }
}
