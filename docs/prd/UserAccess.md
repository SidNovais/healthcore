# PRD: UserAccess

**Status:** Draft
**Author:** IT / LIS Administrator
**Date:** 2026-04-15

---

## 1. Executive Summary

The UserAccess module introduces authentication and role-based authorization to the HC.LIS platform. Today, the system has no login mechanism and no way to restrict access by user role, leaving patient data and clinical workflows unprotected. This feature will allow all platform users — Lab Technicians, Receptionists, Doctors, Lab Managers, and IT Administrators — to authenticate via email and password and access only the modules permitted for their role. The expected outcome is a secure, HIPAA-compliant platform where every action is traceable to an authenticated identity.

---

## 2. Problem Statement

The HC.LIS platform has no authentication or authorization layer. Any user can access all modules and endpoints without identifying themselves, creating significant security, regulatory, and patient-safety risks.

**Current workaround:** None — no workaround exists.

**Why now:** Regulatory compliance (HIPAA), patient safety requirements, and general system security demand that access control is established before the platform is used in any clinical or production environment.

---

## 3. Goals & Success Metrics

| Goal | Success Metric |
|---|---|
| All users can authenticate via email and password | 100% of active users have credentials and can log in successfully |
| Each role can access only its permitted modules | Zero unauthorized module access attempts succeed post-launch |
| Every login attempt and role change is audited | Audit log coverage = 100% of authentication and provisioning events |
| New users receive a welcome email to set their first password | 100% of provisioned users receive the invitation email within 60 seconds |

---

## 4. Users & Personas

| User Role | Frequency | Relationship |
|---|---|---|
| Lab Technician | Daily | Primary actor — SampleCollection access |
| Receptionist | Daily | Primary actor — TestOrders access |
| Doctor | Daily | Primary actor — LabAnalysis access |
| Lab Manager | Daily | Primary actor — all modules |
| IT Admin | Daily | Primary actor — all modules + user provisioning |

**Workflow context:** Users will authenticate at the start of each working session via an email/password login form. The system issues a JWT token which is validated by the API on every subsequent request. User provisioning is an administrative task performed by the IT Admin or root user when onboarding new staff.

---

## 5. Functional Requirements

1. The system shall authenticate users via email and password and issue a signed JWT token upon successful login.
2. The system shall seed a root/superuser account at first startup that can create and manage all other user accounts.
3. The system shall allow an authorized admin to create a new user by providing: email, full name, birthdate, and gender.
4. The system shall send a confirmation email to the new user's email address upon account creation, containing a link or token to set their initial password.
5. The system shall enforce role-based access control on all API endpoints according to the following permission matrix:

   | Role | TestOrders | SampleCollection | LabAnalysis |
   |---|---|---|---|
   | Lab Technician | No access | Full access | No access |
   | Receptionist | Full access | No access | No access |
   | Doctor | No access | No access | Full access |
   | Lab Manager | Full access | Full access | Full access |
   | IT Admin | Full access | Full access | Full access |

6. The system shall record an audit log entry for every login attempt (success or failure) and every role or permission change, including timestamp and acting user identity.

---

## 6. Data & Integrations

**Inputs:**

| Data / Signal | Source | Format |
|---|---|---|
| Email + Password | User (login form) | Plain text over HTTPS |
| Email, Name, Birthdate, Gender | IT Admin (user creation form) | JSON request body |

**Outputs:**

| Output | Destination | Format |
|---|---|---|
| JWT access token | Authenticated client | Signed JWT (Bearer) |
| First-password invitation email | New user's email address | Email (HTML/plain text) |
| Audit log entry | Internal audit store | Structured log record |

**Module integrations:** HC.LIS.API — JWT validation middleware and endpoint authorization attributes must be wired into the existing API host. All three protected modules (TestOrders, SampleCollection, LabAnalysis) will have their endpoints decorated with role requirements sourced from UserAccess.

**External integrations:** SMTP / email service for outbound invitation emails.

---

## 7. Out of Scope (This Release)

- Forgot password / password reset flow
- Social / SSO login (OAuth2, SAML)
- Multi-factor authentication (MFA)
- Fine-grained per-endpoint permission overrides (beyond module-level role matrix)
- Cross-module audit trail (auditing read/write activity within TestOrders, SampleCollection, LabAnalysis is deferred)

---

## 8. Non-Functional Requirements

| Category | Requirement |
|---|---|
| Performance | Login endpoint must respond within 200 ms under normal load |
| Availability | If UserAccess is unavailable, existing valid JWT tokens remain accepted by the API; only sign-in and sign-up are blocked |
| Audit / Traceability | All login attempts and role/permission changes must be logged with timestamp, actor identity, and outcome |
| Data Retention | Audit log entries must be retained in accordance with HIPAA requirements (minimum 6 years) |
| Regulatory Compliance | HIPAA — access control, audit controls, and person authentication safeguards (45 CFR §164.312) |

---

## 9. Constraints

- **Technical:** Must integrate with the existing `HC.LIS.API` project and its JWT configuration. Role-based authorization must be applied via standard ASP.NET Core authorization attributes/policies. UserAccess module must follow the HC.LIS modular monolith architecture (Domain / Application / Infrastructure / IntegrationEvents layer split).
- **Regulatory:** HIPAA Security Rule compliance is mandatory. Audit trail is non-negotiable.
- **Timeline:** No hard deadline.

---

## 10. Open Questions

| # | Question | Owner | Due |
|---|---|---|---|
| 1 | Should cross-module audit logging (tracking reads/writes within TestOrders, SampleCollection, LabAnalysis) be added to the next release scope? | IT Admin / Lab Manager | TBD |
| 2 | What SMTP provider or email service will be used for invitation emails? | IT Admin | TBD |
| 3 | What is the JWT token expiry duration and refresh token strategy? | IT Admin | TBD |
| 4 | Does the root/seed user need a separate onboarding mechanism (env var, migration, CLI)? | IT Admin | TBD |

---

## 11. Revision History

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-04-15 | IT / LIS Administrator | Initial draft via /create-prd |
