using System;
using System.Diagnostics;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class AggregateExceptionExtention
    {
        public static void HandleExceptions(this AggregateException ae, params Type[] exceptionsToDismiss)
        {
            ae.Flatten().Handle(e =>
            {
                foreach (Type ex in exceptionsToDismiss)
                {
                    if (e.GetType() == ex)
                    {
                        Debug.WriteLine(e.Message);
                        return true;
                    }
                }
                return false;
            });
        }
    }
}
