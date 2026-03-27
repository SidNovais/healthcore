# PRD: LabAnalysis

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-03-26

---

## 1. Executive Summary

The LabAnalysis module is the central processing hub of the HealthCore LIS platform. It receives sample collection events, builds analysis worklists for biomedical scientists and pathologists, coordinates communication with clinical analyzer equipment, and returns structured results that feed into patient reports and order completion. Without this module, lab operations cannot progress past sample collection — it is the critical bridge between intake and result delivery. Success means zero paper-based result recording and full digital traceability from sample receipt to released report.

---

## 2. Problem Statement

Biomedical scientists and pathologists currently record test exam results manually on paper after samples are collected. This creates transcription errors, delays in result availability, and no digital audit trail. There is no mechanism to communicate sample or order information to clinical analyzers or to receive results back programmatically.

**Current workaround:** Paper-based result recording and manual data entry.

**Why now:** LabAnalysis is a critical gap in the current system. With sample collection already digitized, the absence of this module means results cannot flow digitally to TestOrders, physicians, or patients — the entire downstream pipeline is blocked.

---

## 3. Goals & Success Metrics

| Goal | Success Metric |
|---|---|
| Eliminate paper-based result recording | 0 paper result entries 6 months post-launch |
| Enable digital result ingestion from clinical analyzers | Results received via events, not manual input, for 100% of processed samples |
| Notify TestOrders on result completion | TestOrders receives PartialComplete / Complete notifications for every finished worklist item |
| Release reports digitally | Report PDFs generated and stored for every completed analysis |

---

## 4. Users & Personas

| User Role | Frequency | Relationship |
|---|---|---|
| Biomedical Scientist | Hundreds–thousands per hour | Primary actor — manages worklist, places samples on analyzers |
| Pathologist | As needed (complex cases) | Primary actor — reviews and validates results |
| TestOrders Module | Per result completion | Downstream consumer — receives PartialComplete / Complete status |
| Future physician/patient portal | Out of scope | Future downstream consumer |

**Workflow context:** A `SampleCollected` integration event is received from the SampleCollection module. LabAnalysis ingests this event to create a worklist item. An event is emitted to notify clinical analyzer equipment. The analyzer processes the sample and returns results via an inbound event. LabAnalysis records the result, generates a report PDF, and emits a notification to TestOrders.

---

## 5. Functional Requirements

1. The system shall ingest `SampleCollected` integration events from the SampleCollection module to create worklist items for biomedical scientists and pathologists.
2. The system shall emit an integration event when a worklist item is created, containing the patient info, sample barcode, and exam code, so that clinical analyzers can be notified.
3. The system shall receive result events from clinical analyzers and store them as result records against the corresponding worklist item.
4. The system shall generate a report PDF upon result completion for each worklist item.
5. The system shall emit a status notification to the TestOrders module (PartialComplete or Complete) when a worklist item result is finalized.

---

## 6. Data & Integrations

**Inputs:**

| Data / Signal | Source | Format |
|---|---|---|
| Sample collected notification | SampleCollection module | Integration event |
| Patient info | Upstream modules (PatientManagement) | Event payload |
| Sample barcode | SampleCollection module | Event payload field |
| Exam code | TestOrders / SampleCollection | Event payload field |
| Analyzer result | Clinical analyzer (via event) | Integration event |

**Outputs:**

| Output | Destination | Format |
|---|---|---|
| Worklist item created event | Clinical analyzer integration layer | Integration event |
| Result record | LabAnalysis internal store | Domain aggregate / event store |
| Report PDF | Internal document store | PDF file |
| Order completion notification | TestOrders module | Integration event (PartialComplete / Complete) |

**Module integrations:** SampleCollection (inbound), TestOrders (outbound), PatientManagement (data source).

**External integrations:** Clinical analyzer equipment — event-based only (no HL7 in this release). Future HL7/ASTM integration is explicitly deferred.

---

## 7. Out of Scope (This Release)

- Patient and physician result portal (viewing/downloading reports)
- HL7 integration with clinical analyzers (event-based communication only)
- Billing or charge capture from results
- QA auditor reporting dashboards

---

## 8. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | Key user actions (worklist load, result save) must respond within 200–300 ms |
| Availability | High — unavailability directly blocks lab operations and all downstream workflows |
| Audit / Traceability | Full event-sourced audit trail required: who entered/validated a result and when |
| Data Retention | Results and reports must be retained in compliance with HIPAA requirements |
| Regulatory Compliance | HIPAA — PHI must be protected at rest and in transit; access controls enforced |

---

## 9. Constraints

- **Technical:** No hard technical constraints identified at this stage; event-driven architecture (publish/subscribe) is the required integration pattern.
- **Regulatory:** HIPAA compliance mandatory — all data handling, storage, and transmission must meet PHI protection standards.
- **Timeline:** Deadline-driven; specific date TBD. This module is on the critical path for system completeness.

---

## 10. Open Questions

| # | Question | Owner | Due |
|---|---|---|---|
| 1 | What is the exact structure/schema of the result event emitted by clinical analyzers? | IT / LIS Admin + Lab Team | TBD |
| 2 | What is the specific deadline for delivery? | Project Stakeholder | TBD |
| 3 | Which report PDF template/format is required for released results? | Lab Director / Pathologist | TBD |
| 4 | How should partial results be handled — emit PartialComplete per item or batch? | IT / LIS Admin + TestOrders team | TBD |
| 5 | Are there specific HIPAA controls (encryption at rest, field-level masking) already standardized in HC.LIS? | IT / LIS Admin | TBD |

---

## 11. Revision History

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-03-26 | IT / LIS Administrator | Initial draft via /create-prd |
