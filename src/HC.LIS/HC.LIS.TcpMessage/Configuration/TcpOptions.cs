namespace HC.LIS.TcpMessage.Configuration;

internal sealed class TcpOptions
{
    public int Port { get; set; } = 8890;
    public bool UseTls { get; set; }
    public string TlsCertificatePath { get; set; } = "";
    public string TlsCertificatePassword { get; set; } = "";
    public int ConnectionTimeoutMs { get; set; } = 5000;
    public bool EnableMllpChecksum { get; set; }
    public bool EnableHl7Checksum { get; set; }
}
