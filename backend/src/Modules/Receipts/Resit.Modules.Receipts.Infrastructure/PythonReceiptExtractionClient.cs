using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Resit.Modules.Receipts.Application.Abstractions;
using Resit.Modules.Receipts.Domain;

namespace Resit.Modules.Receipts.Infrastructure;

public sealed class PythonReceiptExtractionClient(HttpClient httpClient) : IReceiptExtractionClient
{
    public async Task<ExtractedReceiptData> ExtractAsync(Stream fileContent, string fileName, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileContent);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(GuessContentType(fileName));
        content.Add(streamContent, "file", fileName);

        using var response = await httpClient.PostAsync("/extract", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ExtractionResponse>(SnakeCaseOptions, cancellationToken)
            ?? throw new InvalidOperationException("Extraction service returned an empty response.");

        return new ExtractedReceiptData(
            payload.Merchant,
            payload.Total,
            DateOnly.Parse(payload.PurchasedOn),
            payload.SuggestedCategory,
            payload.MerchantConfidence,
            payload.TotalConfidence,
            payload.DateConfidence,
            payload.RawText);
    }

    private static readonly JsonSerializerOptions SnakeCaseOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private static string GuessContentType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".pdf" => "application/pdf",
        _ => "image/jpeg"
    };

    private sealed record ExtractionResponse(
        string Merchant,
        decimal Total,
        string PurchasedOn,
        string SuggestedCategory,
        double MerchantConfidence,
        double TotalConfidence,
        double DateConfidence,
        string RawText);
}
