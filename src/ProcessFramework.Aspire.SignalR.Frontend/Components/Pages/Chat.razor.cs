using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.SemanticKernel;
using ProcessFramework.Aspire.SignalR.Frontend.Models;

namespace ProcessFramework.Aspire.SignalR.Frontend.Components.Pages;

public class ChatBase : ComponentBase
{
    private HubConnection? hubConnection;
    protected List<string> messages = [];
    protected string? userInput;
    protected string? messageInput;

    protected override async Task OnInitializedAsync()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7207/pfevents")
            .Build();

        hubConnection.On<KernelProcessProxyMessage>("ReceivePFEvents", (eventData) =>
        {
            var encodedMsg = $"{eventData.ProcessId}: {eventData.EventData.ToString()}";
            messages.Add(encodedMsg);
            InvokeAsync(StateHasChanged);
        });

        await hubConnection.StartAsync();

        await base.OnInitializedAsync();
    }

    protected async Task Send()
    {
        using var httpClient = new HttpClient();
        var url = "https://localhost:7207/api/generate-doc";
        var payload = new DocumentGenerationRequest()
        {
            Title = "Document Title",
            Content = "Document Content",
            UserDescription = "A document about something"
        };

        var response = await httpClient.PostAsJsonAsync(url, payload);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            messages.Add($"Document generated: {result}");
        }
        else
        {
            messages.Add("Failed to generate document.");
        }
        await InvokeAsync(StateHasChanged);
    }

    protected bool IsConnected =>
        hubConnection?.State == HubConnectionState.Connected;

    protected async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }

}
