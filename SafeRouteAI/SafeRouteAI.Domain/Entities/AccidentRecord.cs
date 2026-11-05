namespace SafeRouteAI.Domain.Entities;

public class AccidentRecord
{
    public int AccidentID { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public DateTime DateTime { get; set; }

    // Proposed fields representing accident-related attributes
    public string? AccidentType { get; set; }

    public string? Severity { get; set; }
}

/*Important: Phase 3 gives severity and accident type as examples of accident-related attributes; 
it doesn't prescribe those exact property names. These are our implementation choices, 
so confirm them with the database team before finalising the schema. -Kaelin Kesavjee*/