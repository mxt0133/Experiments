using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LabSimulator
{
    interface DataExtractorInfrastuctureProcess
    {
        /// <summary>
        /// Method will initialize and start the Data Extractor Infrastucture Process
        /// </summary>
        /// <param name="ConfigurationJSON">JSON configuraiton that the process will use to perform it's task</param>
        /// <param name="Logger">Logger instance that the Data Extract Infrastructure Process will use to log to</param>
        /// <param name="TaskCancellatoinToken">A cancellation token that can be used to signal the Task to terminate</param>
        void Start(string ConfigurationJSON, ILog Logger, CancellationToken TaskCancellatoinToken);            
    }

    public interface IDynamicUI
    {
        void Refresh();
    }

    public class StatusUpdateEventArgs : EventArgs
    {
        public string BatchID { get; set; }
        public string Message { get; set; }
        public DateTime UpdateTimeStamp { get; set; }
    }

    interface DataExtractionProcess
    {
        /// <summary>
        /// Event that will be fired when the status of the current data extract changes
        /// </summary>
        event EventHandler<StatusUpdateEventArgs> OnStatusUpdate;
        
        /// <summary>
        /// Method will initialize and start the Data Extraction Process
        /// </summary>
        /// <param name="ConfigurationJSON">JSON configuraiton that the process will use to perform it's task</param>
        /// <param name="Logger">Logger instance that the Data Extract Infrastructure Process will use to log to</param>
        void Start(string ConfigurationJSON, ILog Logger);   
    }
}
