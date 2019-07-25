using Fiddler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiddleFiddle
{
    /// <summary>
    /// 모든 클래스에 걸쳐서 일관된 로깅 인터페이스를 제공하기 위한 방안
    /// </summary>
    public static class FiddleFiddleLogger
    {
        public static readonly Logger logger = FiddlerApplication.Log;
        public static readonly string Name = "FiddleFiddleExt-gbkim-180423";
        public static void FiddleLog(string msg)
        {
            logger.LogString(
                String.Format(" #<{0}> {1} : {2}", Name, FiddleFiddleTime.Time, msg)
            );
        }

        /// <summary>
        /// 디버깅을 위해서 작성한 로깅 함수이다. params 키워드를 이용하여 가변인자 파라미터를 던진다.
        /// </summary>
        /// <param name="v"></param>
        public static void FiddleDebugLog(params object[] v)
        {
           
            logger.LogString(
                String.Format("{0} : [CERT][DEBUG][FiddleDebugLog]", FiddleFiddleTime.Time) + 
                String.Join("\n", v)
            );
            
        }

        public static void FiddleDebugLogWithPairs(params KeyValuePair<string, object>[] v)
        {
            // 참고 1. Sort Dictionary: https://www.dotnetperls.com/sort-dictionary
            // 참고 2. StringBuilder: https://docs.microsoft.com/ko-kr/dotnet/standard/base-types/stringbuilder
            StringBuilder sbDebug = new StringBuilder();
            var dict = v.ToList();
            // Use Linq To sorting
            var items = from pair in dict
                        orderby pair.Key ascending
                        select pair;

            foreach (KeyValuePair<string, object> pair in items)
            {
                sbDebug.AppendLine(String.Format("{0}: {1}", pair.Key, pair.Value));
            }

            logger.LogString(
                String.Format("{0}\n{1}",
                    String.Format(
                        "{0} : [CERT][DEBUG][FiddleDebugLogWithPairs]", 
                        FiddleFiddleTime.Time
                    ),
                    sbDebug.ToString())
            );
            
        }
    }
}
