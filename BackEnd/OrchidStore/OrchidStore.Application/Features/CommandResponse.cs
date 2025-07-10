namespace OrchidStore.Application.Features;

public class CommandResponse : AbstractApiResponse<string>
{
    public override string Response { get; set; }
}