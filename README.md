# SignalR Blazor Chat Application

## Overview

This repository contains a simple real-time chat application built with Blazor (WebAssembly) and ASP.NET Core SignalR. It's designed as a small demo to show how to build a browser-based chat UI that communicates with a SignalR hub on the server. The solution is split into three main projects:

- `BlazorChatApp` - The Blazor WebAssembly host project (client UI). It contains pages and components that render user lists, chat windows, and system messages.
- `BlazorChatApp.Client` - The Blazor client project (static assets, components, and client-side UI). It contains components such as `ConnectedUsersComponent.razor` and `ChatComponent.razor` and the client-side SignalR connection logic.
- `HubServer` - The ASP.NET Core SignalR hub server that manages connected clients and routes messages.
- `ChatContractsLibrary` - Shared models used by both client and server, e.g. `ConnectedUser` and `ChatMessage`.

The solution is intended as a demonstration and learning example; it uses an in-memory message store by default and does not include production-grade persistence or authentication.

## How it works

- When a user connects the browser opens a SignalR connection to the `ChatHub` endpoint with query parameters for `userId` and `userName`.
- The hub keeps an in-memory list of connected users and an in-memory message history for the demo.
- When a user sends a message from the client, the client calls the hub method `ForwardMessage`, which forwards the message to the target connection and saves it to the hub's in-memory message history.
- The hub pushes updated user lists (including conversation messages relevant to each client) to clients via the `UpdateUserList` client method. The client uses that payload to show chat history and unread counts.

## Projects and key files

- `ChatContractsLibrary`
  - `ConnectedUser.cs` - Model for a connected user with a `Messages` collection.
  - `ChatMessage.cs` - Model for a chat message including `FromUserId`, `ToUserId`, `Message`, and `Unread`.

- `HubServer`
  - `ChatHub.cs` - SignalR hub. Holds the in-memory lists (`_connectedUsers`, `_message_history`) and implements `OnConnectedAsync`, `OnDisconnectedAsync`, and `ForwardMessage`.
  - `Program.cs` - ASP.NET Core startup code to host the hub.

- `BlazorChatApp.Client`
  - `Pages/Home.razor` - Connects to the hub and wires up event handlers.
  - `Components/ConnectedUsersComponent.razor` - Shows the list of connected users and unread counts.
  - `Components/ChatComponent.razor` - Shows messages for the selected conversation and composes new messages.

## Features

- Real-time messaging using SignalR.
- Per-client conversation history (maintained in memory on the server for the demo) so reconnecting users can see earlier messages exchanged with other users.
- Unread message counting and simple notification sound on incoming messages.
- Simple UI implemented with Blazor components.

## Running locally

Prerequisites:

- .NET SDK 9 (matches the solution target)
- A modern browser

Steps:

1. Open the solution in Visual Studio or run from command line.
2. Build the solution:

   `dotnet build`

3. Run the hub server (from the `HubServer` project directory):

   `dotnet run --project HubServer/HubServer.csproj`

   The server listens on the configured URL in `Program.cs` (for example `https://localhost:7070`).

4. Run the Blazor client (from the `BlazorChatApp`/`BlazorChatApp.Client` project depending on your setup). In development the client is typically hosted by the Blazor project and will connect to the hub server URL with the `userId` and `userName` query parameters.

5. Open two browser windows (or different browsers/incognito) and connect with different `userId` and `userName` values to test chat features.

Example client hub connection (found in `Home.razor`):

```
new HubConnectionBuilder().WithUrl($"https://localhost:7070/chathub?userid={userId}&username={userName}").Build();
```

## Message persistence and production notes

- Currently messages are stored in memory in `ChatHub` (`_messageHistory`) to make the demo self-contained. This means messages are lost when the server is restarted.
- For a production system, persist messages and user presence to a durable store (SQL, NoSQL, etc.) and avoid sending entire global histories to each client. Instead, query per-conversation messages from the database when a user opens a chat.
- Consider authentication (ASP.NET Core Identity, JWT) instead of relying on `userId` and `userName` passed in query strings.
- Locking is used around in-memory lists for thread-safety. When moving to a shared data store, use appropriate transactions or concurrency control.

## Extending the project

- Persist messages to a database, add message timestamps, and paginate history.
- Add user authentication and secure the SignalR hub.
- Add group rooms or multi-user chats.
- Improve unread message logic: track per-user per-conversation unread flags server-side and reset when a conversation is opened.

## Troubleshooting

- If clients do not see updates, confirm the hub URL and that the hub server is running and accessible from the client origin.
- If reconnecting clients do not receive history, verify that the hub's `_messageHistory` list is being populated (messages are saved in `ForwardMessage`) and that `UpdateUserList` is invoked.
- For Cross-Origin issues, ensure CORS is configured in the hub server `Program.cs` to allow the client origin.

## Contribution and License

This repo is a sample/demo. Contributions are welcome as pull requests. For production usage, consult proper security and scaling guidance and add a license if sharing or publishing the code more broadly.

---

If you want, I can add a quick guide showing how to replace the in-memory storage with a simple database-backed implementation (EF Core + SQLite example).