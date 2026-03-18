# PRD: Sample Collection Workflow

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-03-17

---

## 1. Executive Summary

The Sample Collection Workflow feature manages the end-to-end process from patient arrival at the laboratory facility through to specimen collection. Today, this process relies entirely on paper forms and manual coordination between Receptionists and Lab Technicians, introducing risk of patient mix-ups and operational delays. This feature replaces the manual process with a digitised, event-driven workflow that automates communication between roles, generates GS1-compliant barcode labels for sample traceability, and records each collection with a tamper-evident audit trail. Success is defined as the system fully managing the sample collection lifecycle, eliminating paper-based tracking.

---

## 2. Problem Statement

Receptionists and Lab Technicians have no digital workflow to coordinate patient check-in, exam preparation verification, and specimen collection. Steps are tracked manually using paper forms, creating risk of lost information, patient mix-ups, and no traceability of who performed which action or when.

**Current workaround:** Manual tracking with paper forms.

**Why now:** Sample collection is a core LIS workflow. The module is required to complete the end-to-end system workflow and unblock downstream processing.

---

## 3. Goals & Success Metrics

| Goal | Success Metric |
|---|---|
| Digitise and automate the sample collection workflow | Zero paper forms required for the patient arrival → specimen collection journey |
| Ensure unique, traceable sample identification | Every tube has a GS1-compliant barcode; no patient mix-up incidents |
| Provide real-time coordination between Receptionist and Lab Technician | Worklist populated at check-in; no manual handoff required |
| Meet HIPAA audit trail requirements | All workflow actions logged with actor, timestamp (UTC), and location |

---

## 4. Users & Personas

| User Role | Frequency | Relationship |
|---|---|---|
| Receptionist | Every patient visit (core daily workflow) | Primary actor — checks in patients, verifies exam preparation |
| Lab Technician / MLT | Every patient visit (core daily workflow) | Primary actor — manages worklist, generates barcodes, collects specimens |

**Workflow context:** A patient arrives at the facility and announces themselves at reception. The Receptionist verifies the patient's exam preparation status. The patient is directed to the waiting room. The Lab Technician prepares barcode labels for the required tubes and readies equipment. The patient is called, and the Lab Technician collects the specimen, registering the collection time in UTC.

---

## 5. Functional Requirements

1. The system shall verify the patient's exam preparation status before allowing the workflow to advance.
2. The system shall generate a worklist of all required test exams for the patient upon successful check-in, visible to the assigned Lab Technician.
3. The system shall generate a GS1-compliant barcode label for each sample tube, uniquely identifying the sample and linking it to the patient and order.
4. The system shall record the specimen collection timestamp in UTC and associate it with the collecting Lab Technician.
5. The system shall emit domain events at each workflow transition: `PatientArrived`, `PatientWaiting`, `PatientCalled`, `BarcodeCreated`, and `SampleCollected`.

---

## 6. Data & Integrations

**Inputs:**

| Data / Signal | Source | Format |
|---|---|---|
| Patient information (identity, relevant clinical info) | TestOrders module / Patient registry | Internal domain model |
| Test exam orders | TestOrders module | Integration event / order record |
| Exam catalog (collection requirements, tube types, volumes) | Exam catalog service / reference data | Internal reference data |

**Outputs:**

| Output | Destination | Format |
|---|---|---|
| `PatientArrived` event | Internal event bus | Domain event |
| `PatientWaiting` event | Internal event bus | Domain event |
| `PatientCalled` event | Internal event bus | Domain event |
| `BarcodeCreated` event | Internal event bus | Domain event |
| `SampleCollected` event | Internal event bus | Domain event |
| Barcode label | Barcode printer / label output | GS1 standard |
| Patient worklist | Lab Technician UI | Internal read model |

**Module integrations:** TestOrders — after a test order is created, this module receives the order so the Receptionist can verify exam preparation and the Lab Technician worklist can be populated.

**External integrations:** GS1 barcode standard for sample tube labelling (barcode printer integration TBD).

---

## 7. Out of Scope (This Release)

- Test result handling and reporting
- Integration with billing or external physician portals
- Automated specimen routing after collection

---

## 8. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | Common actions (patient check-in, barcode generation) must respond within 100 ms |
| Availability | High availability desired; unavailability does not halt operations but causes significant workflow harm |
| Audit / Traceability | All workflow actions must be logged with actor identity, UTC timestamp, and location; required for HIPAA compliance |
| Data Retention | Retain all audit records per HIPAA minimum retention requirements (6 years minimum) |
| Regulatory Compliance | HIPAA — patient data protection, audit trail, access controls |

---

## 9. Constraints

- **Technical:** Each sample must have a unique identifier; GS1 barcode standard strongly preferred for tube labelling; full traceability (who, when, where) required at every step; zero tolerance for patient mix-ups.
- **Regulatory:** HIPAA compliance mandatory — covers PHI handling, audit logging, and access control.
- **Timeline:** No hard deadline defined.

---

## 10. Open Questions

| # | Question | Owner | Due |
|---|---|---|---|
| 1 | Should GS1 be formally adopted as the barcode standard, or are other standards acceptable? | IT / LIS Administrator | TBD |
| 2 | What is the exact scope of "patient information" needed from the patient registry for this module? | IT / LIS Administrator | TBD |
| 3 | Does barcode label printing require direct integration with a printer driver, or is label export sufficient? | IT / LIS Administrator | TBD |
| 4 | What constitutes "location" in the audit trail — facility, room, workstation? | IT / LIS Administrator | TBD |

---

## 11. Revision History

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-03-17 | IT / LIS Administrator | Initial draft via /create-prd |
