using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiddleFiddle.Logon
{
    public class JwtLogonTask : IFiddleTask
    {
        event EventHandler IFiddleTask.CompleteFiddleTasks
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// 작업 테스트 : 로그온 
        /// </summary>
        void IFiddleTask.DoTask()
        {
            
        }

        bool IFiddleTask.IsCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
