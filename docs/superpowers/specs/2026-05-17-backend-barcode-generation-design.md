# Backend Barcode Generation Design

**Date:** 2026-05-17  
**Status:** Implemented

## Problem

Barcode values were provided by the frontend (scanned or typed by the technician). This coupled sample identification to user input when it should be a system concern — a barcode is a permanent unique identifier for a specimen, not something that should depend on human input.

## Decision

Generate barcodes automatically in the backend when the patient moves to `Waiting` status. The frontend removes the manual input step and instead renders the generated barcodes for printing.

## Architecture

### Trigger

`PatientWaitingDomainEvent` → `PatientWaitingNotificationHandler` → schedules `GenerateSampleBarcodesForCollectionRequestCommand` (internal command via `ICommandsScheduler`).

A second handler — `PatientWaitingNotificationProjection` — continues to handle read model projection unchanged.

### Barcode Generation

- **Format:** 8-character uppercase alphanumeric `[A-Z0-9]` (~2.8 trillion combinations)
- **Interface:** `IBarcodeValueGenerator.Generate()` in Application layer
- **Implementation:** `RandomAlphanumericBarcodeValueGenerator` using `Random.Shared` in Infrastructure
- **Uniqueness:** DB unique constraint is the safety net; Quartz auto-retries on the practically impossible collision

### Domain Change

`technicianId` removed from `CollectionRequest.CreateBarcode()` and `BarcodeCreatedDomainEvent` — generation is system-driven, not by a specific technician.

### Frontend

- `barcode-form.component.ts` deleted
- `print-labels-card.component.ts` added: fetches samples, renders barcode SVGs via JsBarcode (CODE128), print button calls `window.print()`
- `createBarcode` removed from port, adapter, and service

## Files Changed

### Added
- `Application/IBarcodeValueGenerator.cs`
- `Application/Collections/GenerateSampleBarcodes/GenerateSampleBarcodesForCollectionRequestCommand.cs`
- `Application/Collections/GenerateSampleBarcodes/GenerateSampleBarcodesForCollectionRequestCommandHandler.cs`
- `Application/Collections/MovePatientToWaiting/PatientWaitingNotificationHandler.cs`
- `Infrastructure/Barcodes/RandomAlphanumericBarcodeValueGenerator.cs`
- `src/app/features/triage/print-labels-card.component.ts`

### Removed
- `API/Modules/SampleCollection/CollectionRequests/CreateBarcode/` (endpoint + request DTO)
- `Application/Collections/CreateBarcode/CreateBarcodeCommand.cs`
- `Application/Collections/CreateBarcode/CreateBarcodeCommandHandler.cs`
- `src/app/features/triage/barcode-form.component.ts`

### Modified
- `Domain/Collections/CollectionRequest.cs` — removed `technicianId` from `CreateBarcode()`, added `GetPendingSampleTubeTypes()`
- `Domain/Collections/Events/BarcodeCreatedDomainEvent.cs` — removed `TechnicianId`
- `Infrastructure/Configurations/SampleCollectionStartup.cs` — added `GenerateSampleBarcodesForCollectionRequestCommand` to BiMap
- `Infrastructure/Configurations/ApplicationModule.cs` — registered `IBarcodeValueGenerator`
- `src/app/features/triage/preparing-patient-card.component.ts` — uses `PrintLabelsCardComponent`

## Label Content

Currently: barcode value only. Future iterations will add patient name, gender, and birthdate to the label.
