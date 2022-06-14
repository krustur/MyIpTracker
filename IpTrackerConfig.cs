// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
public class IpTrackerConfig
{
    public int CheckIntervalMinutes { get; set; } = 15;
    public int UpdateIntervalMinutes { get; set; } = 15;
    public string FromAddress { get; set; } = String.Empty;
    public string FromDisplayName { get; set; } = String.Empty;
    public string FromPassword { get; set; } = String.Empty;
    public string ToAddress { get; set; } = String.Empty;
    public string ToDisplayName { get; set; } = String.Empty;
    public string IpChangedSubject { get; set; } = String.Empty;
    public string IpChangedBody { get; set; } = String.Empty;
    public string IpNotChangedSubject { get; set; } = String.Empty;
    public string IpNotChangedBody { get; set; } = String.Empty;
}