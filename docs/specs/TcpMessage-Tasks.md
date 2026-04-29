# Implementation Tasks: TcpMessage

**Tech Spec:** [docs/specs/TcpMessage-TechSpec.md](./TcpMessage-TechSpec.md)
**Date:** 2026-04-25

> **Workflow rule:** After implementing each task, mark it as done (`- [x]`) in this file before moving on.

---

## Prerequisites

1. **Analyzer module must be fully implemented** (all Analyzer-Tasks.md phases complete) — TcpMessage calls `IAnalyzerModule` and Phases 6–9 below add new commands to the Analyzer Application layer.
2. **No new DB tables needed** — TcpMessage has no schema of its own.

---

## Project Layout

TcpMessage is a **host process**, not a module. It uses a single `Microsoft.NET.Sdk.Worker` project, the same way `HC.LIS.API` uses a single `Microsoft.NET.Sdk.Web` project. There is no Application/Infrastructure/Domain split.

```
src/HC.LIS/
├── HC.LIS.TcpMessage/                      (Worker project — all TCP infrastructure)
├── HC.LIS.TcpMessage.Tests/                (unit tests — MllpFramer, ConnectionHandler)
└── HC.LIS.TcpMessage.IntegrationTests/     (integration tests — live TCP exchanges)
```

---

## Task List

### Phase 1: Project Scaffold

- [x] **Task 1.1** — Create `HC.LIS.TcpMessage` Worker project
  - **Manual**
  - **Creates:**
    - `src/HC.LIS/HC.LIS.TcpMessage/HC.LIS.TcpMessage.csproj` — `Sdk="Microsoft.NET.Sdk.Worker"`, `net10.0`
    - `src/HC.LIS/HC.LIS.TcpMessage/Program.cs` — stub (`Host.CreateDefaultBuilder().Build().RunAsync()`)
  - **Project references:** `HC.LIS.Modules.Analyzer.Infrastructure` (for `IAnalyzerModule`, `AnalyzerStartup`, `AnalyzerModule`), `HC.Core.Application` (for `IExecutionContextAccessor`)
  - **Package references:** `Autofac.Extensions.DependencyInjection`, `Serilog.Extensions.Hosting`, `Serilog.Sinks.Console`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 1.2** — Create `HC.LIS.TcpMessage.Tests` unit test project
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage.Tests/HC.LIS.TcpMessage.Tests.csproj`
  - **Project references:** `HC.LIS.TcpMessage`
  - **Package references:** `xunit`, `FluentAssertions`, `NSubstitute`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 1.3** — Create `HC.LIS.TcpMessage.IntegrationTests` project
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage.IntegrationTests/HC.LIS.TcpMessage.IntegrationTests.csproj`
  - **Project references:** `HC.LIS.TcpMessage`, `HC.LIS.Modules.Analyzer.Infrastructure`
  - **Package references:** `xunit`, `FluentAssertions`
  - **Note:** Requires `ASPNETCORE_HCLIS_IntegrationTests_ConnectionString` env var (same pattern as other integration test projects)
  - **Verify:** `dotnet build` succeeds

---

### Phase 2: MLLP Framer (TDD)

- [x] **Task 2.1** — Write failing unit tests for `MllpFramer`
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage.Tests/Mllp/MllpFramerTests.cs`
  - **Tests:**
    - `WrapPrependsSobAndAppendsEob`
    - `WrapAppendsBccChecksumByteWhenEnabled` — byte before `0x1C` equals sum-of-all-payload-bytes mod 256
    - `UnwrapAsyncReturnsInnerPayload` — strips SOB and EOB, returns payload only
    - `UnwrapAsyncStripsBccChecksumByteWhenEnabled` — checksum byte excluded from returned payload
    - `UnwrapAsyncThrowsOnChecksumMismatch`
    - `UnwrapAsyncThrowsOnTruncatedFrame`
  - **Expected:** Tests fail — `MllpFramer` does not exist yet

- [x] **Task 2.2** — Implement `MllpFramer`
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage/Mllp/MllpFramer.cs`
  - **API:**
    ```csharp
    // Read from stream until 0x1C 0x0D; validates BCC checksum if enabled.
    // Returns inner payload bytes (checksum byte excluded).
    internal static async Task<byte[]> UnwrapAsync(
        Stream stream, bool validateChecksum, CancellationToken ct);

    // Prepend 0x0B, optionally append BCC checksum byte, append 0x1C 0x0D.
    internal static byte[] Wrap(byte[] payload, bool includeChecksum);

    // Sum of all bytes in payload, modulo 256 (BCC — Block Character Check).
    internal static byte ComputeChecksum(byte[] payload);
    ```
  - **Note:** Checksum algorithm is BCC (Block Character Check = sum mod 256), not XOR. This matches HLLP specification.
  - **Verify:** Unit tests from Task 2.1 pass; `dotnet test` — all 6 MllpFramer tests green

---

### Phase 3: Configuration

- [x] **Task 3.1** — Implement `TcpOptions`
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage/Configuration/TcpOptions.cs`
  - **Properties:**
    - `Port (int = 8890)`
    - `UseTls (bool = false)` — most analyzers do not support TLS; defaults off
    - `TlsCertificatePath (string = "")`
    - `TlsCertificatePassword (string = "")`
    - `ConnectionTimeoutMs (int = 5000)`
    - `EnableMllpChecksum (bool = false)`
    - `EnableHl7Checksum (bool = false)`

- [x] **Task 3.2** — Implement `AnalyzerAutofacModule`
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage/Configuration/AnalyzerAutofacModule.cs`
  - **Registers:** `AnalyzerModule` as `IAnalyzerModule` (instance-per-lifetime-scope)
  - **Pattern:** Identical to `HC.LIS.API/Modules/Analyzer/AnalyzerAutofacModule.cs`

---

### Phase 4: TCP Listener

- [x] **Task 4.1** — Implement `TcpListenerService`
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage/Tcp/TcpListenerService.cs`
  - **Responsibilities:**
    - `StartAsync`: bind `TcpListener` to `Port`; start accept loop
    - Per connection: if `UseTls = true` → wrap with `SslStream` (TLS 1.3); else use `NetworkStream` directly
    - Pass stream + remote IP to `ConnectionHandler.HandleAsync`
    - `StopAsync`: stop listener, cancel accept loop, dispose
  - **TLS cert loading** (when `UseTls = true`):
    - `X509Certificate2(TlsCertificatePath, TlsCertificatePassword)` — loaded once at startup
    - If cert file does not exist → throw `InvalidOperationException` before accepting any connection
  - **Note:** `SemaphoreSlim(1,1)` lives in `TcpListenerService`, acquired at the start of each `HandleAsync` call
  - **Verify:** `dotnet build` succeeds

---

### Phase 5: Connection Handler & State Machine (TDD)

- [x] **Task 5.1** — Write failing unit tests for `ConnectionHandler`
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage.Tests/Tcp/ConnectionHandlerTests.cs`
  - **Tests:**
    - `FullExchangeQueryPhaseCallsHandleBarcodeQueryCommand`
    - `FullExchangeResultPhaseSendsImmediateAckBeforeDomainProcessing`
    - `FullExchangeResultPhaseCallsForwardRawResultCommandAfterAck`
    - `ReadTimeoutSemaphoreIsReleased`
  - **Pattern:** Mock `IAnalyzerModule` (NSubstitute), `DuplexStream` helper backed by two `MemoryStream`s
  - **Note:** Semaphore passed into `HandleAsync`; `TcpListenerService` updated accordingly (no longer releases semaphore itself)
  - **Note:** Command stub classes created in `Analyzer.Application` as prerequisite (no handlers yet)
  - **Note:** `NullLogger<ConnectionHandler>.Instance` used instead of `Substitute.For<ILogger<>>()` to avoid Castle DynamicProxy visibility issues with `internal` types

- [x] **Task 5.2** — Implement `ConnectionState` enum
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage/Tcp/ConnectionState.cs`
  - **Values:** `ReceivingQuery`, `QueryAnswered`, `ReceivingResult`, `Done`

- [x] **Task 5.3** — Implement `ConnectionHandler`
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage/Tcp/ConnectionHandler.cs`
  - **State machine:**
    1. `ReceivingQuery` → `MllpFramer.UnwrapAsync` → `HandleBarcodeQueryCommand` → `MllpFramer.Wrap` → write → `QueryAnswered`
    2. `ReceivingResult` → `MllpFramer.UnwrapAsync` → `BuildMessageAckCommand` → write ACK immediately → `ForwardRawResultCommand` → `Done`
  - **Error handling:** Catches non-cancellation exceptions → logs metadata, closes stream, releases semaphore in `finally`
  - **Verify:** All 10 unit tests pass (6 MllpFramer + 4 ConnectionHandler)

---

### Phase 6: Audit Logger

- [x] **Task 6.1** — Implement `TcpAuditLogger`
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage/AuditLog/TcpAuditLogger.cs`
  - **Methods:** `LogInbound(string connectionIp, int messageSizeBytes, ConnectionState state)`, `LogOutbound(string connectionIp, int messageSizeBytes, ConnectionState state)`
  - **Constraint:** No raw payload bytes, barcodes, or patient data in any log entry — metadata only
  - **Uses:** `SystemClock.Now` for timestamps; Serilog structured logging
  - **Verify:** `dotnet build` succeeds

---

### Phase 7: Analyzer Module Additions

These tasks extend the Analyzer module Application layer with three new commands. They live in `HC.LIS.Modules.Analyzer`, not in TcpMessage.

- [x] **Task 7.1** — Implement `BuildMessageAckCommand` and handler
  - **Manual**
  - **Creates:**
    - `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/BuildMessageAck/BuildMessageAckCommand.cs` — `CommandBase<byte[]>`, property: `RawPayload (byte[])`
    - `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/BuildMessageAck/BuildMessageAckCommandHandler.cs`
  - **Handler logic:**
    1. `string rawHl7 = Encoding.UTF8.GetString(RawPayload)`
    2. Extract `MSH-10` (message control ID) by splitting on `|`
    3. Extract `MSH-12` (version ID)
    4. Build: `MSH|^~\&|HC.LIS||ANALYZER||<SystemClock.Now>||ACK^R01|<Guid>|P|<version>\rMSA|AA|<MSH-10>`
    5. Return `Encoding.UTF8.GetBytes(ack)`
  - **Note:** No aggregate access, no DB calls
  - **Verify:** `dotnet build` succeeds

- [x] **Task 7.2** — Define `IHL7QueryParser` interface
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/HandleBarcodeQuery/IHL7QueryParser.cs`
  - **Interface:** `string ParseBarcode(byte[] rawQueryPayload)`

- [x] **Task 7.3** — Implement `HandleBarcodeQueryCommand` and handler
  - **Manual**
  - **Creates:**
    - `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/HandleBarcodeQuery/HandleBarcodeQueryCommand.cs` — `CommandBase<byte[]>`, property: `RawQueryPayload (byte[])`
    - `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/HandleBarcodeQuery/HandleBarcodeQueryCommandHandler.cs`
  - **Handler logic:**
    1. `IHL7QueryParser.ParseBarcode(RawQueryPayload)` → `string barcode`
    2. `IQueryHandler<GetSampleInfoByBarcodeQuery, SampleInfoDto?>.HandleAsync(...)` → `SampleInfoDto` (null → throw `SampleNotFoundException`)
    3. Load aggregate → `AnalyzerSample.DispatchInfo(SystemClock.Now)` → `AppendChanges`
    4. `ISampleInfoPresenter.Format(dto)` → response bytes
  - **Verify:** `dotnet build` succeeds

- [x] **Task 7.4** — Implement `ForwardRawResultCommand` and handler
  - **Manual**
  - **Creates:**
    - `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/ForwardRawResult/ForwardRawResultCommand.cs` — `CommandBase`, property: `RawResultPayload (byte[])`
    - `src/HC.LIS/HC.LIS.Modules/Analyzer/Application/AnalyzerSamples/ForwardRawResult/ForwardRawResultCommandHandler.cs`
  - **Handler logic:**
    1. `IHL7ResultParser.Parse(Encoding.UTF8.GetString(RawResultPayload))` → `AnalyzerResultDto`
    2. Resolve `AnalyzerSampleId` via `GetSampleInfoByBarcodeQuery`
    3. Load aggregate → `AnalyzerSample.ReceiveResult(...)` → `AppendChanges`
  - **Verify:** `dotnet build` succeeds

- [x] **Task 7.5** — Implement `HL7QueryParser` + register in `HL7Module`
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.Modules/Analyzer/Infrastructure/HL7/HL7QueryParser.cs`
  - **Implements:** `IHL7QueryParser.ParseBarcode` — minimal QBP^Q11 field parser; validates HL7 content checksum when `EnableHl7Checksum = true`; throws `HL7ChecksumException` on mismatch
  - **Also update:** `HL7ResultParser` — add same checksum gate for ORU^R01
  - **Modifies:** `Infrastructure/Configurations/HL7/HL7Module.cs` — register `HL7QueryParser` as `IHL7QueryParser`
  - **Verify:** `dotnet build` succeeds

---

### Phase 8: `Program.cs` + `SystemExecutionContextAccessor`

- [x] **Task 8.1** — Implement `SystemExecutionContextAccessor`
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage/SystemExecutionContextAccessor.cs`
  - **Implements:** `IExecutionContextAccessor` with `UserId = Guid.Empty`, `UserName = "tcpmessage-system"`, `IsAvailable = true`, `CorrelationId = Guid.Empty`

- [x] **Task 8.2** — Implement `Program.cs`
  - **Manual**
  - **Modifies:** `src/HC.LIS/HC.LIS.TcpMessage/Program.cs`
  - **Bootstrap sequence:**
    1. Bootstrap Serilog (same template as HC.LIS.API)
    2. `Host.CreateDefaultBuilder` + `UseServiceProviderFactory(new AutofacServiceProviderFactory())`
    3. `AddEnvironmentVariables("ASPNETCORE_HCLIS_")`
    4. `ConfigureContainer<ContainerBuilder>` — register `AnalyzerAutofacModule`
    5. `ConfigureServices` — `AddHostedService<TcpListenerService>()`, bind `TcpOptions` via `services.Configure<TcpOptions>(config.GetSection("TCPMESSAGE"))`
    6. After `Build()`: validate TLS cert if `UseTls = true`; call `AnalyzerStartup.Initialize(...)`
    7. `host.RunAsync()`
  - **Verify:** `dotnet build` succeeds; `dotnet run` starts and logs "Listening on port 8890"

---

### Phase 9: Integration Tests

- [x] **Task 9.1** — Write integration test: full barcode-query exchange
  - **Manual**
  - **Creates:** `src/HC.LIS/HC.LIS.TcpMessage.IntegrationTests/TcpExchangeTests.cs`
  - **Setup:** `AnalyzerStartup.Initialize(connectionString, ...)` in test constructor; start `TcpListenerService` on random port; connect `TcpClient` (plain TCP — no TLS in tests)
  - **Test:** `BarcodeQueryExchange_ReturnsRspWithImplicitAck`
    - Seed `AnalyzerSample` (AwaitingQuery); send MLLP-wrapped QBP^Q11 bytes
    - Assert: MLLP RSP received; contains `MSA|AA`; `AnalyzerSampleDetails.Status == "InfoDispatched"`

- [x] **Task 9.2** — Write integration test: result forward with immediate ACK
  - **Manual**
  - **Modifies:** `TcpExchangeTests.cs`
  - **Test:** `ResultForwardExchange_SendsImmediateAckThenProcessesDomain`
    - Pre-condition: sample in `InfoDispatched` state
    - Send MLLP-wrapped ORU^R01 bytes
    - Assert: `ACK^R01 AA` received; `MSA-2` echoes original `MSH-10`; `AnalyzerSampleExamDetails.ResultValue` set

- [x] **Task 9.3** — Write integration tests: MLLP BCC checksum
  - **Manual**
  - **Modifies:** `TcpExchangeTests.cs`
  - **Tests:**
    - `MllpChecksum_CorrectBccByte_AckReceived`
    - `MllpChecksum_WrongBccByte_ConnectionClosed`

- [x] **Task 9.4** — Write integration tests: HL7 message content checksum
  - **Manual**
  - **Modifies:** `TcpExchangeTests.cs`
  - **Tests:**
    - `Hl7Checksum_ValidContentChecksum_AckReceived`
    - `Hl7Checksum_InvalidContentChecksum_ConnectionClosedWithoutAck`

- [x] **Task 9.5** — Write integration test: semaphore enforcement
  - **Manual**
  - **Modifies:** `TcpExchangeTests.cs`
  - **Test:** `SecondConnection_WaitsForFirstExchangeToComplete`

- [ ] **Task 9.6** — Verify all integration tests pass
  - **Manual**
  - **Verify:** `dotnet test src/HC.LIS/HC.LIS.TcpMessage.IntegrationTests/HC.LIS.TcpMessage.IntegrationTests.csproj` — all tests green

---

### Phase 10: Docker

- [ ] **Task 10.1** — Add `Dockerfile.tcpmessage`
  - **Manual**
  - **Creates:** `Dockerfile.tcpmessage`
  - **Pattern:** Multi-stage — `sdk` build stage, `aspnet` runtime stage
  - **Key lines:** `EXPOSE 8890`, `ENTRYPOINT ["dotnet", "HC.LIS.TcpMessage.dll"]`

- [ ] **Task 10.2** — Add TcpMessage service to `development-compose.yaml`
  - **Manual**
  - **Modifies:** `development-compose.yaml`
  - **Adds:** service `tcpmessage`, `Dockerfile.tcpmessage`, port `8890:8890`, env vars for DB connection string and optional TLS cert
  - **Verify:** `docker-compose -f development-compose.yaml up tcpmessage` — logs show "Listening on port 8890"

---

## Summary

| Phase | Task Count | Complexity |
|---|---|---|
| Project Scaffold | 3 | Low |
| MLLP Framer (TDD) | 2 | Medium |
| Configuration | 2 | Low |
| TCP Listener | 1 | High |
| Connection Handler & State Machine (TDD) | 3 | High |
| Audit Logger | 1 | Low |
| Analyzer Module Additions | 5 | Medium-High |
| Program.cs + SystemExecutionContextAccessor | 2 | Low |
| Integration Tests | 6 | High |
| Docker | 2 | Low |
| **Total** | **27** | |
