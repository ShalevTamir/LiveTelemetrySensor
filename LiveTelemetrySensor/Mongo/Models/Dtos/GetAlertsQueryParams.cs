using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LiveTelemetrySensor.Mongo.Models.Dtos
{
    public class GetAlertsQueryParams
    {
        [BindRequired]
        public long MinTimeStamp { get; set; }
        [BindRequired]
        public long MaxTimeStamp { get; set; }
        [BindRequired]
        public int MaxSamplesInPage { get; set; }
        [BindRequired]
        public int PageNumber { get; set; }
    }
}
