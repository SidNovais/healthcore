# PRD: PatientManagement

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-05-23

---

## 1. Executive Summary

The PatientManagement module introduces a proper patient registry to the HealthCore LIS platform. Today, receptionists have no way to create or look up a real patient record — orders are placed against randomly generated GUIDs, which is operationally unacceptable. This module allows receptionists to register new patients or search for existing ones before placing an order, ensuring every order is tied to a verified patient identity. Patient data is managed with full event sourcing for auditability and propagated to the TestOrders module via integration events so order workflows never depend on a synchronous cross-module call.

---

## 2. Problem Statement

There is no patient registry in the system. When a patient arrives and requests lab exams, the receptionist has no structured way to record or retrieve patient identity — today a random GUID is generated and used as the patient identifier on the order. This means patient data is lost, unsearchable, and not reusable across visits.

**Current workaround:** Receptionist manually generates a random GUID to stand in for a real patient identifier when creating an order.

**Why now:** A functioning patient registry is a prerequisite for a production-grade LIS workflow. Without it, orders cannot be reliably linked to patients, undermining clinical traceability and compliance.

---

## 3. Goals & Success Metrics

| Goal | Success Metric |
|---|---|
| Receptionists can register a patient before placing an order | 100% of new orders are linked to a real patient record (zero GUID-only orders) |
| Receptionists can find existing patients quickly | Patient search returns results within 100–200 ms under normal load |
| Patient data changes are fully traceable | Every mutation to a patient record is captured as a domain event and queryable in the event store |
| TestOrders can create orders without calling PatientManagement synchronously | TestOrders stores a patient snapshot received via integration event; zero synchronous cross-module reads at order creation time |

---

## 4. Users & Personas

| User Role | Frequency | Relationship |
|---|---|---|
| Receptionist | ~400 registrations/day, ~1,000 searches/day | Primary actor |
| TestOrders module | Every order creation | Downstream consumer (integration event) |
| Future modules (billing, QA, etc.) | TBD | Potential future consumers — out of scope this release |

**Workflow context:** A patient arrives and requests lab exams. The receptionist searches for the patient by name or document ID. If found, the existing patient ID is used to start the order. If not found, the receptionist creates a new patient record inline, then uses the newly created patient ID to start the order. The PatientManagement module fires an integration event carrying a patient snapshot; TestOrders consumes this event to maintain its own denormalized patient data.

---

## 5. Functional Requirements

1. The system shall allow a receptionist to **register a new patient** with the following data: full name *(required)*, date of birth *(required)*, gender *(optional)*, mother's full name *(optional)*, national document ID *(optional)*, phone *(optional)*, email *(optional)*.
2. The system shall allow a receptionist to **search for an existing patient** by full name or document ID, returning results within 100–200 ms.
3. The system shall manage patient records using **event sourcing**, recording every data change as a domain event so the full history of mutations is preserved and queryable.
4. The system shall **return a patient ID** upon registration or selection, for use by the receptionist when creating an order in TestOrders.
5. The system shall **publish an integration event** containing a patient data snapshot whenever a patient record is created or updated, enabling TestOrders and future modules to maintain denormalized copies without synchronous cross-module calls.
6. The system shall support **patient data anonymization** on deletion request, replacing personally identifiable fields with anonymized values while retaining the record structure for audit purposes (HIPAA right-of-access / right-to-be-forgotten compliance).

---

## 6. Data & Integrations

**Inputs:**

| Data / Signal | Source | Format |
|---|---|---|
| Full name | Receptionist entry | String (required) |
| Date of birth | Receptionist entry | Date (required) |
| Gender | Receptionist entry | Enum — optional |
| Mother's full name | Receptionist entry | String — optional |
| National document ID | Receptionist entry | String — optional |
| Phone | Receptionist entry | String — optional |
| Email | Receptionist entry | String — optional |
| Search query | Receptionist entry | Name string or document ID string |

**Outputs:**

| Output | Destination | Format |
|---|---|---|
| Patient ID | Receptionist / TestOrders (via UI) | GUID |
| PatientRegisteredIntegrationEvent (snapshot) | Event bus → TestOrders (and future modules) | JSON integration event |
| PatientUpdatedIntegrationEvent (snapshot) | Event bus → TestOrders (and future modules) | JSON integration event |

**Module integrations:** TestOrders — consumes PatientRegistered/PatientUpdated integration events to store a local patient snapshot; no synchronous calls to PatientManagement at order creation time.

**External integrations:** None in this release.

---

## 7. Out of Scope (This Release)

- Patient medical history
- Previous exam or report results linked to the patient
- Billing or insurance information
- Integration with external patient identity systems (e.g., HL7 ADT feeds, national health registries)
- Role-based visibility restrictions on patient data beyond basic authentication

---

## 8. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | Patient search by name or document ID must respond within 100–200 ms under normal operating load (~1,000 searches/day) |
| Availability | Module is critical path — unavailability blocks receptionist from creating orders and halts lab operations |
| Audit / Traceability | All patient record mutations must be stored as domain events in the event store, traceable to the acting user and timestamp |
| Data Retention / Anonymization | Patient records must support anonymization on request (HIPAA); PII fields are replaced with anonymized values; record structure is retained |
| Regulatory Compliance | HIPAA |

---

## 9. Constraints

- **Technical:** Patient data must reside in its own isolated DB schema (consistent with all other HC.LIS modules). No synchronous cross-module reads — TestOrders must use a local snapshot.
- **Regulatory:** HIPAA compliance required for all patient PII at rest and in transit.
- **Timeline:** No hard deadline.

---

## 10. Open Questions

| # | Question | Owner | Due |
|---|---|---|---|
| 1 | Should the anonymization operation be triggered by the receptionist, a lab manager, or a dedicated admin role? | TBD | TBD |
| 2 | Is document ID format validation required (e.g., CPF for Brazil, SSN for US), or is free-text input acceptable? | TBD | TBD |
| 3 | Should duplicate detection be enforced (e.g., same name + DOB + document ID), and what is the resolution flow? | TBD | TBD |

---

## 11. Revision History

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-05-23 | IT / LIS Administrator | Initial draft via /create-prd |
