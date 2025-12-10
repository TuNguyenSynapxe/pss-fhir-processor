using System.Linq;

namespace MOH.HealthierSG.PSS.FhirProcessor.Models.Flattened
{
    /// <summary>
    /// Complete flattened result containing event, participant, and all screening data
    /// </summary>
    public class FlattenResult
    {
        public EventData Event { get; set; }
        public ParticipantData Participant { get; set; }

        public ScreeningSet HearingRaw { get; set; }
        public ScreeningSet OralRaw { get; set; }
        public ScreeningSet VisionRaw { get; set; }

        /// <summary>
        /// Helper to retrieve a specific hearing observation by question code
        /// </summary>
        public ObservationItem GetHearing(string code)
        {
            return HearingRaw?.Items?.FirstOrDefault(i => i.Question?.Code == code);
        }

        /// <summary>
        /// Helper to retrieve a specific oral observation by question code
        /// </summary>
        public ObservationItem GetOral(string code)
        {
            return OralRaw?.Items?.FirstOrDefault(i => i.Question?.Code == code);
        }

        /// <summary>
        /// Helper to retrieve a specific vision observation by question code
        /// </summary>
        public ObservationItem GetVision(string code)
        {
            return VisionRaw?.Items?.FirstOrDefault(i => i.Question?.Code == code);
        }
    }
}
