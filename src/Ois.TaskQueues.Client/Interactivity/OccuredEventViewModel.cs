using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ois.TaskQueues.Client.Interactivity
{
    internal class OccuredEventViewModel
    {
        #region Properties

        public DateTime OccurenceDateTime { get; private set; }

        public TaskQueueServiceEvents EventType { get; private set; }

        public string EventData { get; private set; }
        #endregion

        #region Constructors

        public OccuredEventViewModel(TaskQueueEventOccuredEventArgs dataSource)
        {
            OccurenceDateTime = DateTime.Now;

            JObject jsonEventData = JObject.Parse(dataSource.EventData);

            EventData = jsonEventData.ToString(Formatting.Indented, new JsonConverter[0]);
            EventType = dataSource.EventType;
        }
        #endregion
    }
}