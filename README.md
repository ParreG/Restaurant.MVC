# restaurant-mvc

An ASP.NET Core MVC frontend for a restaurant management system — server-side rendered views that communicate with the [restaurant-api](https://github.com/ParreG/RestrurantPG) backend over HTTP.

The split between MVC and API was a deliberate architectural choice. MVC handles server-rendered pages where SEO and fast initial load matters, while keeping the API clean and reusable for other clients like the Angular frontend.

---

## Tech Stack

- **ASP.NET Core MVC** — Server-side rendering with Razor views
- **Cookie Authentication** — Session-based login with JWT parsing and role extraction
- **HttpClient** — Communicates with the REST API for all data
- **Bootstrap** — Responsive UI with modals and cards
- **Vanilla JS + SVG** — Interactive restaurant floor planner

---

## Features

### Admin Panel
- Full CRUD for bookings, dishes and tables behind `[Authorize]`
- **Interactive SVG floor planner** — drag and drop tables around the restaurant floor, save positions back to the API
- SuperAdmin-only invite code generation for onboarding new admins
- FK conflict handling on table deletion — returns a clean `409 TABLE_HAS_BOOKINGS` instead of crashing

### Auth
- Login flow that parses the JWT, extracts role claims and signs in via cookie authentication
- 8-hour persistent session with clean logout
- Registration via invite code

### Error Handling
- Custom views for 401, 403, 404 and 500 via `UseStatusCodePagesWithReExecute`
- Admin pages set `noindex, nofollow` so they never show up in search results

### Public Pages
- Menu page pulling live dish data from the API
- Homepage displaying popular dishes

---

## Project Structure

```
├── Controllers
│   ├── AdminBookingsController.cs   # Booking CRUD
│   ├── AdminDishesController.cs     # Dish management
│   ├── AdminTablesController.cs     # Table CRUD + SVG layout save
│   ├── AdminInviteController.cs     # SuperAdmin invite codes
│   ├── AdminController.cs           # Login, register, logout
│   ├── DishController.cs            # Public menu
│   ├── ErrorController.cs           # Custom error pages
│   └── HomeController.cs            # Homepage with popular dishes
├── DTOs                             # API request/response models
├── Extensions                       # HttpClient auth header helper
├── Models                           # Domain models
├── ViewModel                        # ViewModels for Razor views
├── Views                            # Razor templates
└── wwwroot                          # CSS, JS (incl. SVG floor planner)
```

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- [restaurant-api](https://github.com/ParreG/RestrurantPG) running on `https://localhost:7270`

### Setup

1. Clone the repo

```bash
git clone https://github.com/ParreG/ResturantPG_MVC.git
cd ResturantPG_MVC
```

2. Start the API first, then run the MVC app

```bash
dotnet run
```

The app expects the API at `https://localhost:7270/api/` — update the base address in `Program.cs` if your setup differs.

---

## Related Repos

| Repo | Role |
|------|------|
| [Restaurant.API](https://github.com/ParreG/Restaurant.API) | REST API — the backend |
| [Restaurant.MVC](https://github.com/ParreG/Restaurant.MVC) | MVC frontend — server-rendered views |
| [Restaurant.Angular](https://github.com/ParreG/Restaurant.Angular) | Angular frontend — SPA client |
