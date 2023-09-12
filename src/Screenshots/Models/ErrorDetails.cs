using Newtonsoft.Json;

namespace Screenshots.Models;

public sealed record ErrorDetails(int StatusCode, string Message)
{
    public override string ToString() => JsonConvert.SerializeObject(this);
}
