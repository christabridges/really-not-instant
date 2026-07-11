using System.Text.Json.Serialization;

public record MatrixResponse {
    [JsonPropertyName("durations")]
    public required double[][] Durations { get; set; }
}
