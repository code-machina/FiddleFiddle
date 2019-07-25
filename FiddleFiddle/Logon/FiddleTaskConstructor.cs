using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiddleFiddle.Logon
{
    class NotImplementedFiddleTask : Exception { }
    interface ILogonTask
    {
        void DoTask();
    }
    /// <summary>
    /// Logon 관련 Task 의 Factory 를 제공한다. 
    /// </summary>
    public class FiddleTaskContrcutor
    {

        FiddleTaskContrcutor(bool predefined=true)
        {
            if (predefined)
            {
                
            }
        }

        IEnumerable<IFiddleTask> GetFiddleTasks()
        {
            List<IFiddleTask> tasks = new List<IFiddleTask>();

            // 작업 추가
            // tasks.Add(new JwtLogonTask())
            // tasks.Add(new CacheTask())
            // tasks.Add(new .... )

            return tasks;
        }

        IEnumerable<IFiddleTask> GetFiddleTasks(IEnumerable<PreTaskItem> preTaskItems)
        {
            List<IFiddleTask> tasks = new List<IFiddleTask>();

            // 작업 추가

            foreach(var item in preTaskItems)
            {
                try
                {
                    var task = GetNewTaskWithEnum(item);
                    tasks.Add(task);
                }
                catch (NotImplementedFiddleTask)
                {

                }
            }

            return tasks;
        }

        IFiddleTask GetNewTaskWithEnum(PreTaskItem item)
        {
            switch (item)
            {
                case PreTaskItem.Logon:
                    throw new NotImplementedFiddleTask();
                default:
                    throw new NotImplementedFiddleTask();
            }
        }
    }

    public enum PreTaskItem
    {
        Logon,
        Cache
    }

    public interface IFiddleTask
    {
        void DoTask(); // Task 작업 시작
        bool IsCompleted(); // Task 종료 체크
        event EventHandler CompleteFiddleTasks; 
    }
}
