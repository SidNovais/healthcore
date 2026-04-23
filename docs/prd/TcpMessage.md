# PRD: TcpMessage

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-04-23

---

## 1. Executive Summary

TcpMessage is a headless ConsoleApp that establishes bidirectional TCP communication between HC.LIS and clinical analyzer equipment. Today, laboratory technicians must manually enter patient and sample information into analyzer UIs and transcribe results back into the LIS — a slow, error-prone process that cannot scale with volume growth. TcpMessage automates this loop: when an analyzer reads a sample barcode, the app responds with the corresponding order information over TCP; when the analyzer completes its analysis, the app receives the result and forwards it to the HC.LIS Analyzer Module. The expected outcome is zero manual order or result entry for connected analyzers.

---

## 2. Problem Statement

Clinical analyzers have no automated channel to exchange data with HC.LIS. When a technician loads a sample, they must manually key patient and order details into the analyzer UI. When results are ready, they must manually transcribe them into the LIS. Both touchpoints introduce transcription errors and are unsustainable at scale.

**Current workaround:** Manual entry of patient/sample information in the analyzer UI and manual result entry in the LIS.

**Why now:** Increasing test volume makes manual throughput a bottleneck. Day-0 automation is a strategic goal — the integration must be in place before volume surpasses manual capacity.

---

## 3. Goals & Success Metrics

| Goal | Success Metric |
|---|---|
| Eliminate manual order entry for analyzers | Zero manual order inputs per analyzer per day post-launch |
| Eliminate manual result entry for analyzers | Zero manual result transcriptions per analyzer per day post-launch |
| Scalable multi-analyzer support | Additional analyzers onboarded via horizontal scaling with no code changes |

---

## 4. Users & Personas

| User Role | Frequency | Relationship |
|---|---|---|
| Automated system (no human actor) | Continuous / per sample | Primary actor |
| Analyzer Module (HC.LIS) | Per result received | Downstream consumer |

**Workflow context:** Order is created in HC.LIS → patient arrives and sample is collected → technician places sample in analyzer → analyzer reads barcode → sends TCP request to TcpMessage asking for sample/order info → TcpMessage responds with the order data → analyzer performs analysis → sends result via TCP to TcpMessage → TcpMessage forwards result to the Analyzer Module.

---

## 5. Functional Requirements

1. The system shall accept inbound TCP connections from clinical analyzer clients over a configurable port.
2. The system shall send TCP response messages back to connected analyzer clients within the request/response cycle.
3. The system shall implement a state machine that enforces processing of one message at a time, holding new connections idle or queued until the current exchange completes.
4. The system shall transport raw TCP message payloads without parsing or interpreting message content (format-agnostic transport layer).
5. The system shall forward received result messages to the HC.LIS Analyzer Module upon successful receipt.
6. The system shall enforce TLS/SSL on all TCP connections.
7. The system shall emit an audit log entry for every inbound and outbound TCP message, masking sensitive and PHI fields before logging.

---

## 6. Data & Integrations

**Inputs:**

| Data / Signal | Source | Format |
|---|---|---|
| Sample barcode query | Clinical analyzer (TCP client) | Raw TCP bytes (format-agnostic) |
| Analysis result | Clinical analyzer (TCP client) | Raw TCP bytes (format-agnostic) |

**Outputs:**

| Output | Destination | Format |
|---|---|---|
| Sample / order information response | Clinical analyzer (TCP client) | Raw TCP bytes (format-agnostic) |
| Result payload | HC.LIS Analyzer Module | Internal module call / integration event |

**Module integrations:** HC.LIS Analyzer Module (result forwarding).

**External integrations:** Clinical analyzer equipment (TCP/IP over TLS).

---

## 7. Out of Scope (This Release)

- ASTM protocol support
- Any other non-TCP messaging protocol
- HL7 message parsing or interpretation
- Analyzer device management or configuration
- UI or dashboard for monitoring connections

---

## 8. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | Request/response cycle must complete within 200ms–1s under normal load |
| Availability | Non-hard-blocking — lab falls back to manual workflow if the app is unavailable |
| Audit / Traceability | Every TCP message (inbound and outbound) must be logged; PHI/sensitive fields must be masked in logs |
| Data Retention | Audit log retention policy to be defined per HIPAA guidelines |
| Regulatory Compliance | HIPAA compliant; TLS/SSL mandatory on all connections |

---

## 9. Constraints

- **Technical:** Deployed as a Docker container; must support horizontal scaling (one instance per analyzer or group of analyzers).
- **Regulatory:** HIPAA — no PHI exposed in logs or unencrypted in transit.
- **Timeline:** No hard deadline.

---

## 10. Open Questions

| # | Question | Owner | Due |
|---|---|---|---|
| 1 | What is the exact TCP message structure used by the target analyzers (byte framing, delimiters, encoding)? | IT / LIS Administrator | TBD |
| 2 | Should TcpMessage query the HC.LIS order data directly or delegate to a specific module (e.g., TestOrders)? | Architecture team | TBD |
| 3 | What is the maximum number of simultaneous analyzer connections expected per instance? | Lab Manager | TBD |
| 4 | What PHI fields must be masked in audit logs (e.g., patient name, DOB, MRN)? | HIPAA Officer | TBD |

---

## 11. Revision History

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-04-23 | IT / LIS Administrator | Initial draft via /create-prd |
