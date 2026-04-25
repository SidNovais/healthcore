# Technical Spec: TcpMessage

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-04-24
**PRD Reference:** [docs/prd/TcpMessage.md](../prd/TcpMessage.md)

---

## 1. Overview

TcpMessage is a headless .NET Generic Host (ConsoleApp) that establishes bidirectional TCP communication between HC.LIS and clinical analyzer equipment. It owns the MLLP transport layer only — all HL7 parsing and formatting remains inside the Analyzer module. TcpMessage's sole responsibilities are:

1. Accept TLS-secured MLLP connections from analyzer clients
2. Route payloads through a state machine (barcode-query phase → result phase)
3. Delegate each payload to the Analyzer module facade (`IAnalyzerModule`)
4. Return the Analyzer module's formatted response over TLS/MLLP
5. Emit PHI-safe audit log entries for every inbound and outbound message

**Relationship to Analyzer TechSpec:** The Analyzer TechSpec (Section 3.4) anticipated a ConsoleApp that called `ISampleInfoPresenter` and `IHL7ResultParser` directly. This TechSpec supersedes that pattern. Two new compound commands on `IAnalyzerModule` encapsulate all HL7 logic — TcpMessage passes raw payloads and receives formatted responses without interpreting content.

---

## 2. Solution Layout

TcpMessage is a **host process**, not a module. It follows the same single-project pattern as `HC.LIS.API` — no Application/Infrastructure layer split, no module facade, no DDD artefacts.

```
src/HC.LIS/
├── HC.LIS.TcpMessage/                      ← single Worker project
│   ├── HC.LIS.TcpMessage.csproj            (Microsoft.NET.Sdk.Worker)
│   ├── Program.cs
│   ├── SystemExecutionContextAccessor.cs
│   ├── Mllp/
│   │   └── MllpFramer.cs
│   ├── Tcp/
│   │   ├── TcpListenerService.cs           (IHostedService)
│   │   ├── ConnectionHandler.cs
│   │   └── ConnectionState.cs
│   ├── AuditLog/
│   │   └── TcpAuditLogger.cs
│   └── Configuration/
│       ├── TcpOptions.cs
│       └── AnalyzerAutofacModule.cs
├── HC.LIS.TcpMessage.Tests/                ← unit tests (MllpFramer, ConnectionHandler)
│   └── HC.LIS.TcpMessage.Tests.csproj
└── HC.LIS.TcpMessage.IntegrationTests/     ← integration tests (live TCP exchange)
    └── HC.LIS.TcpMessage.IntegrationTests.csproj
```

**Project reference:** `HC.LIS.TcpMessage.csproj` references `HC.LIS.Modules.Analyzer.Infrastructure` directly, the same way `HC.LIS.API.csproj` does.

**No ITcpMessageModule facade.** Nothing in the system calls TcpMessage as a module — it is an entry point, not a library.

---

## 3. Checksum Responsibility — Two Layers

There are two independent checksum concerns. Their ownership follows directly from the format-agnostic contract:

| Layer | What is validated | Who validates |
|---|---|---|
| **MLLP transport** | XOR byte appended to the MLLP frame (before `0x1C 0x0D`) | **TcpMessage** — `MllpFramer` validates/generates the envelope before any content is read |
| **HL7 message content** | Checksum embedded inside the HL7 payload (e.g., vendor `Z`-segment such as `ZCS`, or a field in `MSH`) | **Analyzer module** — validated inside `IHL7QueryParser.ParseBarcode()` and `IHL7ResultParser.Parse()`, which own all HL7 interpretation |

TcpMessage never reads inside the MLLP envelope. If an HL7-level checksum is invalid, the Analyzer module's parser throws, the command fails, and `ConnectionHandler` closes the connection without writing an ACK. This keeps the boundary clean regardless of which analyzer vendor's checksum scheme is in use.

---

## 4. MLLP Framing

MLLP (Minimum Lower Layer Protocol) is the sole transport framing. TcpMessage recognises the envelope bytes only — it never reads the HL7 content inside.

### 4.1 Frame Format

**Without checksum (default):**

```
[0x0B] <payload bytes> [0x1C] [0x0D]
```

**With checksum (opt-in via `TcpOptions.EnableMllpChecksum`):**

```
[0x0B] <payload bytes> [checksum byte] [0x1C] [0x0D]
```

The checksum byte is the XOR of all payload bytes. This is the form used by several clinical analyzer vendors that extend standard MLLP with a single-byte vertical count check (VCC).

| Byte(s) | Role |
|---|---|
| `0x0B` | Start-block — first byte of every MLLP frame |
| `<payload>` | Raw HL7 message bytes (opaque to TcpMessage) |
| `checksum` | XOR of all payload bytes — only present when `EnableMllpChecksum = true` |
| `0x1C 0x0D` | End-block — two-byte sequence that terminates the frame |

### 4.2 `MllpFramer` (static helpers)

```csharp
// Read from SslStream until end-block; validates checksum if enabled.
// Returns inner payload bytes (checksum byte excluded).
internal static async Task<byte[]> UnwrapAsync(
    Stream stream, bool validateChecksum, CancellationToken ct);

// Prepend start-block, optionally append checksum byte, append end-block.
internal static byte[] Wrap(byte[] payload, bool includeChecksum);

// XOR of all bytes in payload.
internal static byte ComputeChecksum(byte[] payload);
```

`UnwrapAsync` reads byte-by-byte until it detects `0x1C 0x0D`. It strips the leading `0x0B` and the trailing end-block before returning. When `validateChecksum` is `true`, it reads the byte immediately before `0x1C` as the checksum, verifies it against `ComputeChecksum(payload)`, and throws `InvalidOperationException("MLLP checksum mismatch")` on failure. If the stream closes before end-block is seen, it throws `InvalidOperationException("MLLP frame truncated")`.

---

## 5. Connection State Machine

One global `SemaphoreSlim(1, 1)` lives in `TcpListenerService`. Every accepted connection must acquire it before processing begins; it is released when the connection closes. This enforces at-most-one-active-exchange per instance as required by the PRD.

### 5.1 State Diagram

```
[TcpClient accepted]
    │
    ├─ await semaphore.WaitAsync(ct)
    │
    ▼
ConnectionState.ReceivingQuery
    │ MllpFramer.UnwrapAsync → rawQueryPayload
    │ IAnalyzerModule.ExecuteCommandAsync(HandleBarcodeQueryCommand(rawQueryPayload))
    │     → rawResponsePayload (RSP includes implicit MSA acknowledgment)
    │ MllpFramer.Wrap(rawResponsePayload) → TLS write
    ▼
ConnectionState.QueryAnswered
    │
    ▼
ConnectionState.ReceivingResult
    │ MllpFramer.UnwrapAsync → rawResultPayload
    │
    │ ── Immediate ACK (before domain processing) ──────────────────────────────
    │ IAnalyzerModule.ExecuteCommandAsync(BuildMessageAckCommand(rawResultPayload))
    │     → rawAckPayload  (ACK^R01 AA — MSH-10 of original message echoed in MSA-2)
    │ MllpFramer.Wrap(rawAckPayload) → TLS write
    │ ── Domain processing ─────────────────────────────────────────────────────
    │ IAnalyzerModule.ExecuteCommandAsync(ForwardRawResultCommand(rawResultPayload))
    ▼
ConnectionState.Done
    │
    └─ semaphore.Release()  ·  connection closed
```

### 5.2 ACK Strategy Per Message Type

| Message received | ACK sent | Timing |
|---|---|---|
| Barcode query (QBP^Q11) | RSP response (includes `MSA` segment with `AA`) | Combined — RSP is the acknowledgment |
| Result message (ORU^R01) | Explicit `ACK^R01` with `MSA\|AA` | **Immediate** — before domain processing begins |

Sending the ORU acknowledgment before domain processing ensures the analyzer is not blocked waiting for DB operations to complete. The ACK bytes are generated by `BuildMessageAckCommand`, which reads only `MSH-10` (message control ID) from the raw payload and requires no DB access.

### 5.3 `ConnectionState` enum

```csharp
internal enum ConnectionState { ReceivingQuery, QueryAnswered, ReceivingResult, Done }
```

`ConnectionHandler` is instantiated per accepted connection and runs the state machine. It holds no static state.

---

## 6. TLS Configuration

```csharp
// TcpListenerService — per accepted connection
TcpClient client = await _listener.AcceptTcpClientAsync(ct);
var sslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);
await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
{
    ServerCertificate = _certificate,   // X509Certificate2 loaded at startup
    EnabledSslProtocols = SslProtocols.Tls13,
    ClientCertificateRequired = false,
}).ConfigureAwait(false);
```

Certificate loaded once at `TcpMessageStartup.Initialize()`:

```csharp
X509Certificate2 cert = new(options.TlsCertificatePath, options.TlsCertificatePassword);
```

If either env var is absent or the file does not exist, startup throws `InvalidOperationException` and the host does not start.

---

## 7. Analyzer Module — New Application Layer Additions

Two new commands are added to `HC.LIS.Modules.Analyzer.Application`. They replace the direct `ISampleInfoPresenter` / `IHL7ResultParser` calls that the Analyzer TechSpec (Section 3.4) had initially assigned to the ConsoleApp. Location for both: `Application/AnalyzerSamples/{CommandName}/`.

### 7.1 `HandleBarcodeQueryCommand`

```
extends CommandBase<byte[]>
Properties: RawQueryPayload (byte[])
```

**Handler — `HandleBarcodeQueryCommandHandler`:**

1. `IHL7QueryParser.ParseBarcode(RawQueryPayload)` → `string sampleBarcode`
2. `GetSampleInfoByBarcodeQuery(sampleBarcode)` (existing query) → `SampleInfoDto`
3. `IAggregateStore.Load<AnalyzerSample>(dto.AnalyzerSampleId)` → call `AnalyzerSample.DispatchInfo(SystemClock.Now)` → `IAggregateStore.AppendChanges()`
4. `ISampleInfoPresenter.Format(dto)` → `string hl7Response`
5. Return `Encoding.UTF8.GetBytes(hl7Response)`

**Notification emitted:** `SampleInfoDispatchedNotification` (already defined in Analyzer TechSpec).

### 7.2 `ForwardRawResultCommand`

```
extends CommandBase<byte[]>
Properties: RawResultPayload (byte[])
```

**Handler — `ForwardRawResultCommandHandler`:**

1. `string rawHl7 = Encoding.UTF8.GetString(RawResultPayload)`
2. `IHL7ResultParser.Parse(rawHl7)` (existing) → `AnalyzerResultDto`
3. `IAnalyzerSampleByBarcodeProvider.GetIdByBarcode(dto.SampleBarcode)` → `AnalyzerSampleId`
4. `IAggregateStore.Load<AnalyzerSample>(analyzerSampleId)` → call `AnalyzerSample.ReceiveResult(...)` → `AppendChanges()`
5. Build standard HL7 AA acknowledgment string → `Encoding.UTF8.GetBytes(ack)`
6. Return ACK bytes

**Notification emitted:** `ExamResultReceivedNotification` → `ExamResultReceivedIntegrationEvent` (already defined).

### 7.3 `BuildMessageAckCommand`

```
extends CommandBase<byte[]>
Properties: RawPayload (byte[])
```

**Handler — `BuildMessageAckCommandHandler`:**

1. `string rawHl7 = Encoding.UTF8.GetString(RawPayload)`
2. Extract `MSH-10` (message control ID) — minimal field-delimiter parse, no full HL7 interpretation
3. Extract `MSH-12` (version ID) for the ACK `MSH` header
4. Build HL7 ACK message:
   ```
   MSH|^~\&|HC.LIS||ANALYZER||<timestamp>||ACK^R01|<new_control_id>|P|<version>
   MSA|AA|<original_MSH-10>
   ```
5. Return `Encoding.UTF8.GetBytes(ack)`

This command requires **no aggregate access and no DB calls** — it is pure HL7 string construction. It is always fast enough to send before domain processing begins.

### 7.4 `IHL7QueryParser` (new interface)

Location: `Application/AnalyzerSamples/HandleBarcodeQuery/IHL7QueryParser.cs`

```csharp
internal interface IHL7QueryParser
{
    string ParseBarcode(byte[] rawQueryPayload);
}
```

Implementation `HL7QueryParser` lives in `Infrastructure/HL7/` alongside the existing `HL7ResultParser`. Registered in `HL7Module` (already exists in `AnalyzerStartup`).

### 7.5 HL7 Message-Level Checksum — Analyzer Module Responsibility

If the analyzer vendor embeds a checksum **inside the HL7 message content** (e.g., a `ZCS` Z-segment, a vendor-specific field in `MSH`, or a trailing checksum record), validation is the responsibility of the Analyzer module — not TcpMessage.

**Where it is enforced:**
- `IHL7QueryParser.ParseBarcode()` — validates QBP^Q11 content checksum before returning the barcode
- `IHL7ResultParser.Parse()` — validates ORU^R01 content checksum before returning `AnalyzerResultDto`

**Failure behaviour:**
Both parsers throw `HL7ChecksumException : InvalidOperationException` on mismatch. The exception propagates through `HandleBarcodeQueryCommand` / `ForwardRawResultCommand` → MediatR pipeline → `IAnalyzerModule.ExecuteCommandAsync`. `ConnectionHandler` catches the exception, logs it (metadata only — no payload), and closes the connection without writing an ACK. The semaphore is released.

**Implementation note:** Whether the target analyzer actually uses an HL7-level checksum is TBD (PRD Open Question #1 — message structure). `HL7QueryParser` and `HL7ResultParser` should be written with a configurable flag (`TcpOptions.EnableHl7Checksum`) so the validation can be toggled without code changes when the analyzer protocol is confirmed.

---

## 8. TcpMessage Module Infrastructure

### 8.1 `TcpListenerService` (`IHostedService`)

- Holds `TcpListener`, `X509Certificate2`, `SemaphoreSlim(1,1)`, `IAnalyzerModule`, `TcpAuditLogger`, `TcpOptions`
- `StartAsync`: starts `TcpListener.Start()` and loops `AcceptTcpClientAsync`
- `StopAsync`: calls `TcpListener.Stop()`, cancels the loop, disposes resources
- Per accepted connection: authenticate TLS → instantiate `ConnectionHandler` → `await handler.HandleAsync(sslStream, ct)`
- New connections block on `semaphore.WaitAsync(ct)` inside `ConnectionHandler.HandleAsync()` before the state machine begins

### 8.2 `ConnectionHandler`

Constructor dependencies: `IAnalyzerModule`, `TcpAuditLogger`, `TcpOptions`.

```csharp
internal sealed class ConnectionHandler(
    IAnalyzerModule analyzerModule,
    TcpAuditLogger auditLogger,
    TcpOptions options)
{
    internal async Task HandleAsync(SslStream stream, string connectionIp, CancellationToken ct);
}
```

`HandleAsync` runs the state machine (Section 5). Each `MllpFramer.UnwrapAsync` call is wrapped in a `CancellationTokenSource` with `options.ConnectionTimeoutMs` deadline. If the deadline expires, the connection is closed and the semaphore released.

### 8.3 `TcpAuditLogger`

Wraps Serilog. Called after every MLLP read and every MLLP write. Logs structured properties only — never raw payload bytes.

```csharp
internal sealed class TcpAuditLogger(ILogger logger)
{
    internal void LogInbound(string connectionIp, int messageSizeBytes, ConnectionState state);
    internal void LogOutbound(string connectionIp, int messageSizeBytes, ConnectionState state);
}
```

Serilog template (added to structured log context):

```
[{Timestamp}] [{Direction}] IP={ConnectionIp} Size={MessageSizeBytes}B State={State}
```

**PHI contract:** `messageSizeBytes` is the only payload-derived value logged. No barcode, patient name, exam mnemonic, or result value is ever written to the log.

### 8.4 `TcpOptions`

```csharp
internal sealed class TcpOptions
{
    public int Port { get; init; } = 8890;
    public string TlsCertificatePath { get; init; } = string.Empty;
    public string TlsCertificatePassword { get; init; } = string.Empty;
    public int ConnectionTimeoutMs { get; init; } = 5000;
    public bool EnableMllpChecksum { get; init; } = false;
    public bool EnableHl7Checksum { get; init; } = false;
}
```

Bound from `IConfiguration` at startup.

---

## 9. `Program.cs`

Follows the same bootstrap pattern as `HC.LIS.API/Program.cs`. All TCP and Autofac wiring lives here — no separate Startup class.

```csharp
// ─── Logger ────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateDefaultBuilder(args)
        .UseSerilog((ctx, _, config) => config
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module}] {Message:lj}{NewLine}{Exception}"))
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureContainer<ContainerBuilder>(cb =>
        {
            cb.RegisterModule(new AnalyzerAutofacModule()); // registers IAnalyzerModule
        })
        .ConfigureServices((ctx, services) =>
        {
            services.AddHostedService<TcpListenerService>();
        });

    builder.Configuration.AddEnvironmentVariables("ASPNETCORE_HCLIS_");

    var host = builder.Build();

    // ─── Module initialization ─────────────────────────────────────────────
    var connectionString = builder.Configuration["DATABASE_CONNECTION_STRING"]
        ?? throw new InvalidOperationException("Missing ASPNETCORE_HCLIS_DATABASE_CONNECTION_STRING");

    var executionContext = new SystemExecutionContextAccessor();

    AnalyzerStartup.Initialize(connectionString, executionContext, Log.Logger, eventBus: null);

    await host.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "HC.LIS.TcpMessage terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

**`SystemExecutionContextAccessor`** — lightweight `IExecutionContextAccessor` returning a fixed system identity (`UserId = Guid.Empty`, `UserName = "tcpmessage-system"`, `IsAvailable = true`). Defined in `HC.LIS.TcpMessage/SystemExecutionContextAccessor.cs`.

---

## 10. Configuration

All env vars use the `ASPNETCORE_HCLIS_` prefix (stripped by `AddEnvironmentVariables`).

| Env var (without prefix) | `TcpOptions` property | Default | Purpose |
|---|---|---|---|
| `DATABASE_CONNECTION_STRING` | — | required | Passed to `AnalyzerStartup.Initialize` |
| `TCPMESSAGE_PORT` | `Port` | `8890` | TCP listen port |
| `TCPMESSAGE_TLS_CERT_PATH` | `TlsCertificatePath` | required | Path to PFX certificate file |
| `TCPMESSAGE_TLS_CERT_PASSWORD` | `TlsCertificatePassword` | required | PFX password |
| `TCPMESSAGE_CONNECTION_TIMEOUT_MS` | `ConnectionTimeoutMs` | `5000` | Per-read deadline (ms) |
| `TCPMESSAGE_ENABLE_MLLP_CHECKSUM` | `EnableMllpChecksum` | `false` | Enable XOR checksum on MLLP frames |
| `TCPMESSAGE_ENABLE_HL7_CHECKSUM` | `EnableHl7Checksum` | `false` | Enable HL7 message content checksum validation (Analyzer module) |

`TcpOptions` is bound via:

```csharp
builder.Configuration.GetSection("TCPMESSAGE").Get<TcpOptions>()
```

(The nested env var key format `TCPMESSAGE_PORT` maps to `TCPMESSAGE:PORT` after the prefix is stripped, which binds to `TcpOptions.Port`.)

---

## 11. Infrastructure Wiring

### `AnalyzerAutofacModule`

Defined in `Configuration/AnalyzerAutofacModule.cs`. Same pattern as `HC.LIS.API/Modules/Analyzer/AnalyzerAutofacModule.cs`:

```csharp
internal sealed class AnalyzerAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AnalyzerModule>()
               .As<IAnalyzerModule>()
               .InstancePerLifetimeScope();
    }
}
```

### `TcpListenerService` DI registration

`TcpListenerService` is registered as `IHostedService` via `services.AddHostedService<TcpListenerService>()` in `Program.cs` (standard .NET Worker pattern — no Autofac module needed for the hosted service itself).

`TcpOptions` is resolved from `IConfiguration` inside `TcpListenerService`'s constructor via the standard `IOptions<TcpOptions>` pattern or `IConfiguration.GetSection`. `TcpAuditLogger` and `ConnectionHandler` are plain constructor-injected dependencies.

### TLS validation — fail fast in `Program.cs`

```csharp
// After builder.Build(), before host.RunAsync()
var options = host.Services.GetRequiredService<IOptions<TcpOptions>>().Value;
if (options.UseTls && !File.Exists(options.TlsCertificatePath))
    throw new InvalidOperationException($"TLS certificate not found: {options.TlsCertificatePath}");
```

No separate `TcpMessageStartup` class — validation is inline.

---

## 12. Unit Tests

Location: `Tests/UnitTests/` within the TcpMessage module.

| Test | Asserts |
|---|---|
| `MllpFramer_Wrap_PrependsSobAppendsEob` | Output starts with `0x0B`, ends with `0x1C 0x0D`, inner bytes match input |
| `MllpFramer_Wrap_AppendsChecksumWhenEnabled` | Byte before `0x1C` equals XOR of all payload bytes |
| `MllpFramer_UnwrapAsync_ReturnsInnerPayload` | Strips SOB, checksum (if present), and EOB; returns only payload bytes |
| `MllpFramer_UnwrapAsync_ThrowsOnChecksumMismatch` | `InvalidOperationException("MLLP checksum mismatch")` when checksum validation is enabled and byte is wrong |
| `MllpFramer_UnwrapAsync_ThrowsOnTruncatedFrame` | `InvalidOperationException("MLLP frame truncated")` when stream closes before EOB |
| `ConnectionHandler_FullExchange_SendsAckBeforeDomainProcessing` | Mock `IAnalyzerModule` receives `BuildMessageAckCommand` and the TLS write completes **before** `ForwardRawResultCommand` is called |
| `ConnectionHandler_FullExchange_QueryPhaseCallsBarcodeCommand` | `HandleBarcodeQueryCommand` called with correct raw bytes; response written to stream |
| `ConnectionHandler_TimeoutOnRead_ReleasedSemaphore` | When read exceeds `ConnectionTimeoutMs`, semaphore count returns to 1 |

### Analyzer module unit tests — new commands

Location: existing `Tests/UnitTests/AnalyzerSamples/AnalyzerSampleTests.cs`

No new aggregate unit tests are required. The new command handlers coordinate existing domain methods (`DispatchInfo`, `ReceiveResult`) which are already unit-tested. Handler-level tests belong to integration tests.

---

## 13. Integration Tests

Location: `Tests/IntegrationTests/` within the TcpMessage module.

Tests spin up a real `TcpListenerService` (on a random port) against a real PostgreSQL database and a real `AnalyzerStartup`-initialized Analyzer module.

| Test | Scenario | Assertion |
|---|---|---|
| `BarcodeQueryExchange_ReturnsRspWithImplicitAck` | Pre-seed `AnalyzerSample` (AwaitingQuery); connect TcpClient → send MLLP-wrapped QBP barcode query | MLLP RSP response received; response contains `MSA\|AA`; `AnalyzerSampleDetails.Status == "InfoDispatched"` |
| `ResultForwardExchange_SendsImmediateAckThenProcessesDomain` | After dispatch, send MLLP-wrapped ORU result | MLLP `ACK^R01 AA` received with original `MSH-10` echoed in `MSA-2`; `AnalyzerSampleExamDetails.ResultValue` set; `AnalyzerSampleDetails.Status == "ResultReceived"` |
| `ResultForwardExchange_ChecksumValidation_AcceptsCorrectChecksum` | Send MLLP-wrapped ORU with correct XOR checksum (EnableChecksum = true) | ACK received; no error |
| `ResultForwardExchange_ChecksumValidation_RejectsWrongChecksum` | Send MLLP-wrapped ORU with tampered checksum (EnableChecksum = true) | Connection closed before ACK; no DB state change |
| `SecondConnection_WaitsForFirstExchangeToComplete` | Two concurrent TcpClients attempt connection | Second client does not progress until first exchange is `Done` |
| `TlsHandshakeFailure_ConnectionRejected` | TcpClient without valid TLS presents plain-text | Connection closes; no state change in DB |

---

## 14. Open Design Decisions

| # | Decision | Recommendation |
|---|---|---|
| 1 | HL7 query message type for barcode queries | QBP^Q11 assumed; validate with Lab Manager when target analyzer is identified |
| 2 | MLLP transport checksum | `EnableMllpChecksum = false` by default (standard MLLP v1); enable when vendor requires VCC byte in framing |
| 3 | HL7 message content checksum | `EnableHl7Checksum = false` by default; Analyzer module parsers validate when enabled; confirm checksum scheme (Z-segment, MSH field, other) with Lab Manager once target analyzer is identified |
| 4 | Max simultaneous connections per instance | Currently 1 (one-at-a-time per PRD); increase by changing `SemaphoreSlim` initial count when Lab Manager defines multi-connection requirements |
| 5 | `SystemExecutionContextAccessor` identity | Using `UserId = Guid.Empty` and `UserName = "tcpmessage-system"`; replace with a proper service account once UserAccess module is integrated |
| 6 | PHI masking beyond size-only | HIPAA Officer to specify additional fields; current design is safe-by-default (no payload bytes logged) |
| 7 | ACK MSH sender/receiver fields | `MSH-3` = `HC.LIS`, `MSH-5` = `ANALYZER` hardcoded for now; move to `TcpOptions` when multi-analyzer identity is required |

---

## 15. Implementation Task Breakdown

| Phase | Task | Key Files |
|---|---|---|
| 1 | Scaffold `HC.LIS.TcpMessage` Worker project + test projects | `HC.LIS.TcpMessage.csproj`, `HC.LIS.TcpMessage.Tests.csproj`, `HC.LIS.TcpMessage.IntegrationTests.csproj` |
| 2 | `MllpFramer` + unit tests | `Mllp/MllpFramer.cs`, `HC.LIS.TcpMessage.Tests/Mllp/MllpFramerTests.cs` |
| 3 | `TcpOptions`, `AnalyzerAutofacModule` | `Configuration/TcpOptions.cs`, `Configuration/AnalyzerAutofacModule.cs` |
| 4 | `TcpListenerService` (TLS/plain, semaphore, accept loop) | `Tcp/TcpListenerService.cs` |
| 5 | `ConnectionHandler` + `ConnectionState` + unit tests | `Tcp/ConnectionHandler.cs`, `Tcp/ConnectionState.cs`, `HC.LIS.TcpMessage.Tests/Tcp/ConnectionHandlerTests.cs` |
| 6 | `TcpAuditLogger` | `AuditLog/TcpAuditLogger.cs` |
| 7 | Analyzer module — `BuildMessageAckCommand` (immediate ACK, no DB) | `Analyzer/Application/AnalyzerSamples/BuildMessageAck/` |
| 8 | Analyzer module — `HandleBarcodeQueryCommand`, `ForwardRawResultCommand`, `IHL7QueryParser` | `Analyzer/Application/AnalyzerSamples/HandleBarcodeQuery/`, `ForwardRawResult/` |
| 9 | `HL7QueryParser` implementation + registration in `HL7Module` | `Analyzer/Infrastructure/HL7/HL7QueryParser.cs` |
| 10 | `Program.cs` + `SystemExecutionContextAccessor` | `HC.LIS.TcpMessage/Program.cs`, `SystemExecutionContextAccessor.cs` |
| 11 | Integration tests | `HC.LIS.TcpMessage.IntegrationTests/TcpExchangeTests.cs` |
| 12 | `Dockerfile` + `docker-compose` entry | `Dockerfile.tcpmessage`, `development-compose.yaml` |
