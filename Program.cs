// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

var ipTrackerConfig = new IpTrackerConfig();
configuration.GetSection("IpTracker").Bind(ipTrackerConfig);

ConsoleWriteConfiguration(ipTrackerConfig);
var currentIp = GetIpAddress();

var lastCheck = DateTime.Now;
var lastUpdate = DateTime.Now;

ConsoleWriteAlert("==> Application startup <=============");
ConsoleWriteAlert($"Initial Ip Address was: {currentIp}");
ConsoleWriteAlert($"Check interval in minutes: {ipTrackerConfig.CheckIntervalMinutes}");
ConsoleWriteAlert($"Update interval in minutes: {ipTrackerConfig.UpdateIntervalMinutes}");

do
{
    var newIp = GetIpAddress();
    if (DateTime.Now >= lastCheck + TimeSpan.FromMinutes(ipTrackerConfig.CheckIntervalMinutes))
    {
        if (newIp != null && !Equals(newIp, currentIp))
        {
            SendEMail(ipTrackerConfig, ipTrackerConfig.IpChangedSubject, ipTrackerConfig.IpChangedBody,
                newIp.ToString());
            currentIp = newIp;
            lastUpdate = DateTime.Now;
        }
        else
        {
            ConsoleWrite($"Ip not changed: {currentIp}");
        }

        lastCheck = DateTime.Now;
    }
    else if (DateTime.Now >= (lastUpdate + TimeSpan.FromMinutes(ipTrackerConfig.UpdateIntervalMinutes)))
    {
        SendEMail(ipTrackerConfig, ipTrackerConfig.IpNotChangedSubject, ipTrackerConfig.IpNotChangedBody, currentIp?.ToString());
        lastUpdate = DateTime.Now;
    }

    Thread.Sleep(5000);
} while (true);

IPAddress? GetIpAddress()
{
    try
    {
        string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "")
            .Replace("\\n", "").Trim();
        var externalIp = IPAddress.Parse(externalIpString);

        return externalIp;
    }
    catch (Exception e)
    {
        ConsoleWriteError($"Get IP address failed: {e}");
        return null;
    }
}

void SendEMail(IpTrackerConfig config, string subject, string bodyTemplate, string? ipAddress)
{
    try
    {
        var fromAddress = new MailAddress(config.FromAddress, config.FromDisplayName);
        string fromPassword = config.FromPassword;
        var toAddress = new MailAddress(config.ToAddress, config.ToDisplayName);
        var body = bodyTemplate
            .Replace("{newLine}", Environment.NewLine)
            .Replace("{ipAddress}", ipAddress);

        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
        };
        using (var message = new MailMessage(fromAddress, toAddress)
               {
                   IsBodyHtml = false,
                   Subject = subject,
                   Body = body
               })
        {
            smtp.Send(message);
        }
        ConsoleWriteAlert($"Sent email: {subject}");
    }
    catch (Exception e)
    {
        ConsoleWriteError($"Send e-mail failed: {e}");
    }
}

void ConsoleWriteConfiguration(IpTrackerConfig ipTrackerConfig1)
{
    ConsoleWrite($"FromAddress: {ipTrackerConfig1.FromAddress}");
    ConsoleWrite($"FromDisplayName: {ipTrackerConfig1.FromDisplayName}");
    // ConsoleWrite($"FromPassword: {ipTrackerConfig1.FromPassword}");
    ConsoleWrite($"ToAddress: {ipTrackerConfig1.ToAddress}");
    ConsoleWrite($"ToDisplayName: {ipTrackerConfig1.ToDisplayName}");
    ConsoleWrite($"IpChangedSubject: {ipTrackerConfig1.IpChangedSubject}");
    ConsoleWrite($"IpChangedBody: {ipTrackerConfig1.IpChangedBody}");
}

void ConsoleWrite(string message)
{
    Console.WriteLine($"[{DateTime.Now}] {message}");
}

void ConsoleWriteAlert(string message)
{
    var oldCol = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[{DateTime.Now}] {message}");
    Console.ForegroundColor = oldCol;
}

void ConsoleWriteError(string error)
{
    var oldCol = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[{DateTime.Now}] {error}");
    Console.ForegroundColor = oldCol;
}