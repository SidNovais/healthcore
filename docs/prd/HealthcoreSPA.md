# PRD: HealthcoreSPA

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-05-03

---

## 1. Executive Summary

HealthcoreSPA is a browser-based Single Page Application built with Angular and TypeScript that provides a complete user interface for the HC.LIS laboratory information system. Today, all interactions require direct API calls, making the system inaccessible to non-technical lab staff. The SPA exposes role-specific workflows — login, test order requests, waiting room and sample collection, and a doctor worklist — so that Lab Technicians, Lab Managers, Doctors, Receptionists, and IT Admins can complete their daily tasks without any API knowledge. Success means every core backend feature is reachable through a HIPAA-compliant, sub-200ms web interface used dozens of times per day across all roles.

---

## 2. Problem Statement

There is no user interface for the HC.LIS system. Every user action — creating orders, collecting samples, reviewing exam results — requires calling the REST API directly. This is technically inaccessible to clinical and administrative staff and introduces operational risk, as staff cannot perform their roles without developer assistance.

**Current workaround:** Direct REST API calls (requires technical knowledge).

**Why now:** The HC.LIS backend is functionally complete. A UI is now the critical missing layer to make the system usable by its intended end-users and to deliver value to the lab.

---

## 3. Goals & Success Metrics

| Goal | Success Metric |
|---|---|
| All lab staff can perform daily workflows without API access | 100% of core workflows (login, orders, sample collection, worklist) operable via UI |
| Role-appropriate access for all user types | Each of the 5 roles (Receptionist, Lab Technician, Bio-doctor, IT Admin, Lab Manager) can log in and reach their relevant screens |
| Responsive interface meeting lab throughput demands | Common user actions complete in ≤ 200 ms |
| HIPAA-compliant data handling | No PHI exposed in URLs, logs, or local storage; authenticated access enforced on all routes |

---

## 4. Users & Personas

| User Role | Frequency | Relationship |
|---|---|---|
| Receptionist | Dozens of times/day | Primary — registers patients, requests test orders |
| Lab Technician | Dozens of times/day | Primary — manages waiting room, collects samples |
| Bio-doctor / Pathologist | Dozens of times/day | Primary — reviews worklist, views exam states and results |
| Lab Manager | Multiple times/day | Primary — oversees workflow and queue |
| IT / LIS Administrator | As needed | Primary — system configuration and user management |

**Workflow context:** A patient arrives → Receptionist creates a test order request → Lab Technician calls the patient from the waiting room and collects the sample → Pathologist reviews the worklist to see exam states and results. This cycle repeats dozens of times per shift.

---

## 5. Functional Requirements

1. The system shall provide a Login screen that authenticates users against the HC.LIS API and enforces role-based access control.
2. The system shall provide a Test Order Request UI that allows authorised users to create and submit new test orders via the HC.LIS API.
3. The system shall provide a Waiting Room / Sample Collection UI that displays queued patients, allows staff to call the next patient, and registers sample collection events.
4. The system shall provide a Worklist UI that displays the current state of each exam (pending, in progress, completed) along with results, scoped to the authenticated doctor's context.
5. The system shall enforce HIPAA-compliant data handling across all screens (authenticated routes, no PHI in URLs or browser storage).

---

## 6. Data & Integrations

**Inputs:**

| Data / Signal | Source | Format |
|---|---|---|
| User credentials | End-user input | Form fields (username/password) |
| Test order data | End-user input + HC.LIS API | JSON via REST |
| Patient queue / waiting room state | HC.LIS API | JSON via REST |
| Exam worklist and results | HC.LIS API | JSON via REST |

**Outputs:**

| Output | Destination | Format |
|---|---|---|
| New test order | HC.LIS API | HTTP POST (JSON) |
| Sample collection event | HC.LIS API | HTTP POST/PUT (JSON) |
| Patient call action | HC.LIS API | HTTP POST (JSON) |
| Authenticated session token | Browser memory | JWT (in-memory, not localStorage) |

**Module integrations:** HC.LIS.Modules.TestOrders, HC.LIS.Modules.UserAccess (authentication)

**External integrations:** None in this release.

---

## 7. Out of Scope (This Release)

- Reports and dashboards
- Billing or insurance workflows
- HL7 / FHIR external system integration
- Mobile or native application
- Offline / progressive web app (PWA) capabilities
- Patient-facing portal

---

## 8. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | Common user actions (page transitions, form submissions) must complete in ≤ 200 ms |
| Availability | SPA unavailability degrades service quality (staff serve patients with less accuracy) but does not fully block operations |
| Audit / Traceability | No specific UI-layer audit trail required; backend handles event sourcing |
| Data Retention | No UI-side retention requirements |
| Regulatory Compliance | HIPAA — PHI must not appear in URLs, browser console logs, or client-side storage; all routes must require authentication |

---

## 9. Constraints

- **Technical:** Angular + TypeScript; browser-only (no SSR, no mobile wrapper); integrates exclusively with the HC.LIS REST API
- **Regulatory:** HIPAA compliance required across all screens and data handling
- **Timeline:** No hard deadline

---

## 10. Open Questions

| # | Question | Owner | Due |
|---|---|---|---|
| 1 | What authentication flow does HC.LIS API expose? (JWT bearer, cookie session, OAuth?) | IT Admin / Backend team | TBD |
| 2 | Are there role-permission mappings already defined in the UserAccess module that the SPA should mirror? | IT Admin | TBD |
| 3 | Should the worklist be real-time (polling or WebSocket) or refreshed on demand? | Lab Manager / Pathologist | TBD |
| 4 | Which exam states are exposed by the TestOrders module API today? | Backend team | TBD |

---

## 11. Revision History

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-05-03 | IT / LIS Administrator | Initial draft via /create-prd |
