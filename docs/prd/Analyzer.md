# PRD: Analyzer

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-04-08

---

## 1. Executive Summary

The Analyzer module automates bidirectional communication between the LIS and clinical analyzers using the HL7 protocol. Today, laboratory technicians manually enter patient and sample data into analyzers and then manually transcribe results back into the LIS — a repetitive, error-prone process across approximately 500 samples per day. This module eliminates both manual steps by responding to analyzer barcode queries with patient/sample information and by receiving and persisting analyzer results automatically, improving patient safety and operational throughput.

---

## 2. Problem Statement

Laboratory technicians must manually input patient demographics and sample data into clinical analyzers, then manually transcribe results back into the LIS. This dual manual-entry workflow is slow, error-prone, and does not scale with growing sample volumes.

**Current workaround:** Manual data entry on both sides — technicians type patient information into the analyzer and then re-enter results into the LIS.

**Why now:** Patient safety risk from transcription errors and the need to accelerate sample turnaround time as volumes grow.

---

## 3. Goals & Success Metrics

| Goal | Success Metric |
|---|---|
| Eliminate manual sample-info entry into analyzers | 100% of analyzer sample queries answered automatically via HL7 |
| Eliminate manual result transcription into LIS | 100% of analyzer results received and persisted automatically via HL7 |
| Reduce sample turnaround time | Measurable reduction in time from sample placement to result availability |
| Improve data accuracy | Zero transcription errors for analyzer-communicated results |

---

## 4. Users & Personas

| User Role | Frequency | Relationship |
|---|---|---|
| Laboratory Technician | ~500 samples/day | Primary actor — places samples and monitors workflow |
| Lab Manager / Supervisor | Daily | Oversees operations, monitors analyzer connectivity and throughput |
| LabAnalysis Module | Event-driven | Downstream consumer — receives result notifications |

**Workflow context:** When a technician places a sample in the analyzer, the analyzer reads the barcode and requests sample information via TCP/IP. The Analyzer module responds with an HL7 message containing patient/sample data. Once analysis completes, the analyzer sends results via TCP/IP as an HL7 message. The module parses, persists, and forwards results to downstream modules.

---

## 5. Functional Requirements

1. The system shall construct and send HL7 messages containing sample and patient information in response to analyzer queries by barcode.
2. The system shall receive and parse HL7 result messages from clinical analyzers.
3. The system shall persist received results and update internal sample status (e.g., processing, result received).
4. The system shall store sample information (patient name, birthdate, gender, barcode, exam mnemonic, urgency) sourced from TestOrders and SampleCollection integration events, ready for analyzer queries.
5. The system shall notify the LabAnalysis module when results are received from an analyzer.

---

## 6. Data & Integrations

**Inputs:**

| Data / Signal | Source | Format |
|---|---|---|
| Patient name, birthdate, gender | TestOrders / SampleCollection integration events | Integration event |
| Sample barcode | TestOrders / SampleCollection integration events | Integration event |
| Exam mnemonic, urgency flag | TestOrders / SampleCollection integration events | Integration event |
| Analyzer result data | Clinical analyzer | HL7 v2.x message via TCP/IP |
| Analyzer sample query | Clinical analyzer | HL7 v2.x message via TCP/IP |

**Outputs:**

| Output | Destination | Format |
|---|---|---|
| Sample/patient information | Clinical analyzer | HL7 v2.x message via TCP/IP |
| Result-received notification | LabAnalysis module | Integration event |
| Status changes (processing, result received) | Internal (event-sourced) | Domain events |

**Module integrations:** Listens to integration events from TestOrders and SampleCollection modules. Publishes integration events to LabAnalysis module. Transport mechanism TBD (in-memory event bus vs RabbitMQ).

**External integrations:** Clinical analyzers via HL7 v2.x over TCP/IP. The TCP/IP transport layer is handled by an external ConsoleApp/WebApi that calls the Analyzer module facade.

---

## 7. Out of Scope (This Release)

- ASTM protocol or any protocol other than HL7
- Quality Control (QC) workflows
- TCP/IP transport implementation — the module exposes a facade; a separate ConsoleApp/WebApi handles the network layer
- Patient management aggregate creation (may be needed but is a separate module concern)

---

## 8. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | Response to analyzer barcode query must complete within 300ms |
| Availability | Not strictly blocking — manual fallback exists — but degradation significantly delays lab operations |
| Audit / Traceability | Full event sourcing via Marten; all state transitions recorded as domain events |
| Data Retention | Per event store retention policy (Marten) |
| Regulatory Compliance | Possibly HIPAA — requires further investigation for LIS-analyzer integration requirements |

---

## 9. Constraints

- **Technical:** HL7 v2.x standard version commonly used by clinical analyzers; module must be usable from a separate process (ConsoleApp) via its facade; event sourcing with Marten
- **Regulatory:** HIPAA applicability to be confirmed; accreditation requirements for LIS-analyzer integration unknown — needs research
- **Timeline:** No hard deadline

---

## 10. Open Questions

| # | Question | Owner | Due |
|---|---|---|---|
| 1 | Which specific HL7 v2.x version and message types to support first? (e.g., ORM, ORU, QRY) | IT / LIS Admin | TBD |
| 2 | Will the module run in-process or as a separate process? This determines event bus transport (in-memory vs RabbitMQ). | IT / LIS Admin | TBD |
| 3 | Are there specific HIPAA or accreditation requirements (CAP, CLIA, ISO 15189) for LIS-analyzer communication? | Lab Director | TBD |
| 4 | Do we need a PatientManagement module/aggregate to properly store patient demographics, or will caching from TestOrders/SampleCollection events suffice? | IT / LIS Admin | TBD |
| 5 | Which analyzer brands/models will be targeted first for validation? | Lab Manager | TBD |

---

## 11. Revision History

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-04-08 | IT / LIS Administrator | Initial draft via /create-prd |
