# Campus Events API

A full-stack **ASP.NET Core Web API** for managing campus events, categories, and bookings with **JWT-based authentication**, **role-based authorization**, and a **client dashboard**.

This project was developed as part of a **Service-Oriented Architecture (SOA)** assignment and demonstrates modern backend design principles, RESTful APIs, DTO usage, authentication, and client–server interaction.

---

## 🚀 Features

### 🔐 Authentication & Authorization

- JWT-based authentication
- Role-based access control:
    - **Admin**
    - **Organiser**
    - **Student**
- Secure protected routes using `[Authorize]`
- Tokens used in:
    - Swagger
    - Client-side application

---

### 📅 Events

- Create, read, update, delete events (**Admin / Organiser only**)
- Events belong to a **Category**
- Event capacity enforced
- Sorting supported:
    - By start time
    - By title
- Public event browsing

---

### 🏷 Categories

- Create categories (**Admin / Organiser only**)
- Delete categories (**Admin only**)
- Public category listing

---

### 🎟 Bookings

- Students can:
    - Book events
    - View **their active bookings only**
    - Cancel bookings
- Business rules enforced:
    - No duplicate bookings
    - Capacity limits
- Organisers/Admins can:
    - View who booked each event
    - Remove attendees (cancel bookings)
- Cancelled bookings are **filtered from results**

---

### 🖥 Client Application

- HTML / CSS / JavaScript client
- Role-aware UI:
    - Buttons and actions appear based on role
- Features:
    - Login / Register
    - Browse & sort events
    - Book / cancel bookings
    - View event attendees (Organiser/Admin)
- JWT stored securely in `localStorage`
- API communication via `fetch`

---

## 🧱 Architecture & Design

- **ASP.NET Core Web API (.NET 8)**
- **Entity Framework Core**
- **SQLite (local)** / **Azure SQL (cloud-ready)**
- **DTO pattern** (data returned to client differs from DB models)
- **Repository pattern** for events
- Clear separation of concerns:
    - Controllers
    - Services
    - Repositories
    - DTOs
- One-to-many relationships:
    - Category → Events
    - Event → Bookings
    - User → Bookings
