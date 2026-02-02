using System.Diagnostics.CodeAnalysis;
using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Routes.Headless;

namespace FikaServer.Models.Fika.Headless;

public record StartHeadlessRaid : IHeadlessWSMessage
{
    [SetsRequiredMembers]
    public StartHeadlessRaid(StartHeadlessRequest request)
    {
        StartHeadlessRequest = request;
    }

    public EFikaHeadlessWSMessageType Type { get; set; } = EFikaHeadlessWSMessageType.HeadlessStartRaid;
    public required StartHeadlessRequest? StartHeadlessRequest { get; set; }
}
