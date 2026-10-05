# Code Review Notes — CustomerManagementPractiseCS

Written for intern level. Every item explains **what happens**, **why it happens**, and **how to fix it**
in the simplest way that works. Nothing here needs a production-grade rewrite — these are small,
learnable fixes you can make this week.

**Build status:** `dotnet build` → **Build succeeded, 0 errors.** Nothing is broken at compile time.
Everything below is a runtime / design / security issue.

**Last verified:** 2026-10-05, against commit `dd43aa1` plus the uncommitted working-tree changes.
Every path, line number and count in this document was re-checked against the source that day.

---

## Table of Contents

1. [Real Bugs (fix these first)](#1-real-bugs-fix-these-first)
2. [Security Issues](#2-security-issues)
3. [Dead Code to Clean Up](#3-dead-code-to-clean-up)
4. [Improvements (good learning exercises)](#4-improvements-good-learning-exercises)
5. [Things You Already Did Well](#5-things-you-already-did-well)
6. [Suggested Order of Work](#6-suggested-order-of-work)

### Status

**Done (6):** 1.3, 1.5, 4.1, 4.2, 4.6, 4.8
**Still open:** everything else, plus the new items 1.9 and 1.10 below.

Since the last update the **Experience and Social Link sections were converted to the same AJAX modal
pattern** as Address / Contact / Education (`refactor(profile): implement asynchronous partial view
loading for experience and social links`). That widened several findings rather than closing them:

| Item | What changed |
|---|---|
| 1.1 | Now **five** broken pages, not two — `Education/Create`, `Experience/Create` and `SocialLink/Create` have the same `[FromBody]` + HTML form mismatch. |
| 1.2 | One of the 15 catch blocks got an alert; the other 14 still only `console.error`. The one that did is also subtly broken. |
| 1.3 | Widened from 9 handlers to **15**, and it stayed fixed for the two new sections. |
| 3.1 | The dead-view list grew from 5 files to **11** — the new `_ExperienceEditForm` / `_SocialLinkDeleteForm` pattern orphaned another six. |
| 4.4 | The copy-pasted "find my profile" block grew from 31 to **36** occurrences. |
| 4.3 | The 5 child collections in `ProfileController.Index` are now **loaded and never rendered** — see the new note in 4.3. |

Work that was not part of this review: a Blood Group filter, AJAX loading for the admin profile table and
the user table, a delete confirmation modal for user accounts, a shared `showToast()` helper in
`wwwroot/js/site.js`, and the navbar alignment rules in `wwwroot/css/site.css`.

---

## 1. Real Bugs (fix these first)

### 1.1 Five `/…/Create` pages always fail with **415 Unsupported Media Type**

**Files:**
- `Controllers/AddressController.cs:61` — `public async Task<IActionResult> Create([FromBody] AddressCreateViewModel ViewModel, ...)`
- `Controllers/ContactController.cs:62` — same pattern
- `Controllers/EducationController.cs:65` — same pattern
- `Controllers/ExperienceController.cs:65` — same pattern
- `Controllers/SocialLinkController.cs:65` — same pattern
- `Views/Address/Create.cshtml:14`, `Views/Contact/Create.cshtml:14`,
  `Views/Education/Create.cshtml:14`, `Views/Experience/Create.cshtml:14`,
  `Views/SocialLink/Create.cshtml:14` — all `<form asp-action="Create" method="post">`

**What happens:** You sign in, go to `/Address/Create`, fill the form, press *Save Address*, and the
server answers **415 Unsupported Media Type**. Nothing is saved and the user sees a blank error page.
The same happens on `/Contact/Create`, `/Education/Create`, `/Experience/Create` and
`/SocialLink/Create`.

**Why:** These two POST actions read the body with `[FromBody]`, which means *"give me JSON"*.
But the plain HTML `<form method="post">` in the view sends the data as
`application/x-www-form-urlencoded` (normal web form data). ASP.NET Core has no JSON reader for that
content type, so it refuses the request with 415 before your code even runs.

The AJAX modals on `Views/Profile/Index.cshtml` send real JSON (`axios.post(url, data)`), so
**those work fine**. Only the five standalone `/…/Create` pages are broken.

**How to fix (pick one):**

Option A — delete the broken pages, because the modals already do the job:
```bash
# remove the unreachable/broken pieces
Views/Address/Create.cshtml      Views/Contact/Create.cshtml
Views/Education/Create.cshtml     Views/Experience/Create.cshtml
Views/SocialLink/Create.cshtml
GET + POST AddressController.Create
GET + POST ContactController.Create
GET + POST EducationController.Create
GET + POST ExperienceController.Create
GET + POST SocialLinkController.Create
```
The `+ Add` buttons on the profile page already open modals, so nothing is lost.

Option B — keep the pages and make them work, by removing `[FromBody]`:
```csharp
// Form data binds automatically, no [FromBody] needed
[HttpPost]
public IActionResult Create(AddressCreateViewModel ViewModel)
```
Then change the return values so the user gets a real page instead of JSON:
```csharp
if (!ModelState.IsValid)
{
    return View(ViewModel);   // instead of BadRequest(ModelState)
}

return RedirectToAction("Index", "Profile");   // instead of Ok()
```

> **Lesson:** `[FromBody]` = "the client sends JSON (fetch/axios)".
> No attribute = "the client sends a normal HTML form". Pick one style per action and stay consistent.

---

### 1.2 Failures in the AJAX modals are silent — the user sees nothing happen

**File:** `Views/Profile/Index.cshtml` — the `.catch(...)` block of every axios call.
There are now **15** of them (address, contact, education, experience and social link × create / edit /
delete), and only one of them shows the user anything.

**What happens:** You add an address with an empty city inside the modal. The server replies
`400 Bad Request` with the validation errors. The modal just stays open and **nothing is displayed**.
The only clue is in the browser console (F12). Same for every other modal except address-create.

**Why:** Almost every `axios` call ends with a `.catch()` that only does `console.error(...)`:
```javascript
.catch(function (error) {
    console.error('Error submitting address:', error);   // user never sees this
});
```

**The one exception** is `Views/Profile/Index.cshtml:499`, in the address-create handler, which does
prepend an alert. It is a good direction but it is not finished:
- The message is **hardcoded** — "Failed to create address. Please try again." — so the actual
  validation message the server sent ("City is required.") is still thrown away.
- The alert is never **removed**. Press *Save* twice with a bad value and you get two stacked alerts.
- It uses a **fixed `id="model-error-alert"`**, and `id`s must be unique in a document.

**How to fix (all 15 handlers):** show the problem inside the modal. Small change, big UX win:
```javascript
.catch(function (error) {
    let message = 'Something went wrong. Please try again.';

    // If the server sent validation errors, show them
    if (error.response && error.response.data && error.response.data.errors) {
        message = Object.values(error.response.data.errors).flat().join('<br>');
    }

    // Clear any previous alert first, then show the new one.
    // No id — the element does not need to be looked up later.
    $('#addressEditModalBody').find('.alert-danger').remove();
    $('#addressEditModalBody').prepend(
        $('<div class="alert alert-danger"></div>').text(message)
    );
});
```
> Using `.text()` instead of putting the string into `.prepend('…' + message + '…')` matters: server
> messages can contain characters that would otherwise be parsed as HTML.

---

### 1.3 "Success" messages pop up on the wrong page, later — **FIXED**

**Files:** `Controllers/AddressController.cs`, `Controllers/ContactController.cs`,
`Controllers/EducationController.cs`, `Controllers/ExperienceController.cs`,
`Controllers/SocialLinkController.cs` (Create / Edit / Delete POST actions)

**What happened:** You add an address in the modal → success. Then you click *View CV* → the banner
**"Address added successfully."** suddenly appears there. Confusing.

**Why:** Those actions used to set `TempData["Success"] = "..."` but then `return Ok()` — no redirect.
`TempData` survives until the **next full page render**, and the AJAX call is not a page render,
so the message waited and then popped up on whatever page you visited next.

**The fix:** the message now travels back with the JSON response, and JavaScript shows it.
```csharp
return Ok(new { message = "Address added successfully." });
```
```javascript
.then(function (response) {
    showToast(response.data.message);
    bootstrap.Modal.getInstance(document.getElementById('addressCreateModal')).hide();
    fetchAddresses();
});
```

`Views/Profile/Index.cshtml` grew a small `showToast(message, isError)` helper that appends a
Bootstrap alert to a fixed holder in the bottom-right corner and fades it out after 4 seconds. It
uses `.text()`, not `.html()`, so a message can never inject markup. **All 15** success handlers —
Address, Contact, Education, Experience and Social Link × create / edit / delete — now call it, and
`showToast(response.data.message)` appears 15 times in the file.

Verified: no `TempData` remains in any of the five section controllers. The only `TempData` left in
the project is in `AccountController` and `ProfileController`, and every one of those actions ends in a
redirect — which is the correct pairing.

**Rule to remember:** `TempData` is for actions that **redirect**. An endpoint that answers `200 OK`
with JSON has to send its message in the body.

---

### 1.4 `[Required]` does nothing on `DateOnly` and `int`

**Files:**
- `ViewModels/Profile/ProfileCreateViewModel.cs:16-19`
- `ViewModels/Profile/ProfileEditViewModel.cs:19-21`
- `ViewModels/EducationViewModels/EducationCreateViewModel.cs:22-23`

**What happens:** A user submits the profile form with the date of birth left blank. Instead of a
clean *"Date of birth is required"* message, you either get a confusing model-binding error, or
the date is saved as `01 Jan 0001` and the profile page renders **"01 Jan 0001"**.

**Why:** `[Required]` only works on types that can be `null` (reference types and nullable value
types). `DateOnly` and `int` are **non-nullable**, so they always have *a* value (`0001-01-01` and
`0`). The attribute is checked, always passes, and does nothing.

**How to fix:** Use the nullable version so "not filled in" is a real state:
```csharp
[Required]
[DataType(DataType.Date)]
public DateOnly? DateOfBirth { get; set; }     // was: DateOnly
```
```csharp
[Required]
[Range(1900, 2100, "Enter a realistic year.")]
public int? StartYear { get; set; }           // was: int
```
Then guard the places you print the date:
```csharp
// ProfileController.Index
DateOfBirth = profileData.DateOfBirth,
```
```cshtml
@* Views/Profile/Index.cshtml *@
@if (Model.DateOfBirth.HasValue)
{
    <text>@Model.Gender / @Model.DateOfBirth.Value.ToString("dd MMM yyyy")</text>
}
else
{
    <text>@Model.Gender</text>
}
```
The same `.ToString("dd MMM yyyy")` pattern exists in `Views/Admin/Index.cshtml:97`,
`Views/Admin/Details.cshtml:36`, `Views/Profile/Delete.cshtml:19` and `Views/Profile/Cv.cshtml:38` —
all five need the guard.

Note that `EducationCreateViewModel.EndYear` (line 25) is already `int?`, which is why a blank
end year works while a blank start year does not. That contrast is the whole point of this item.

> **Lesson:** `[Required]` on `int` / `bool` / `DateOnly` / `DateTime` = no-op.
> Use `int?` / `bool?` / `DateOnly?` when "the user may leave it blank".

---

### 1.5 The contact email/phone validation script never runs — **FIXED**

**Files:** `Views/Contact/_ContactCreateForm.cshtml`, `Views/Profile/Index.cshtml`

**What happens:** You type `abc` as an Email contact. The form accepts it.

**Why (two separate causes, both verified against the live HTML):**

1. The script was wrapped in `@section Scripts { ... }` inside a **partial view**. Razor only renders
   section blocks declared in the **main view** — a partial is an HTML fragment with no section slot.
   Verified: the partial's form HTML appeared on the page, but its `<script>` did not.
2. Even if it had rendered, the error would have gone nowhere. `<span asp-validation-for="Value">`
   renders as `<span class="text-danger" data-valmsg-for="Value">` with **no `id`**. Verified:
   `id="ValueError"` was absent from the output, so `$("#ValueError")` matched nothing.

**The fix (frontend only, no backend change):**
- Gave the form an id: `<form id="contactCreateForm" ...>`
- Gave the error span an id: `<span id="ValueError" asp-validation-for="Value" ...>`
- Moved the validation JavaScript into the main view's working `@section Scripts` in
  `Views/Profile/Index.cshtml`
- Scoped the handler to `#contactCreateForm` instead of `$("form")` (which bound every form on the page)
- Added `e.stopPropagation()` alongside `e.preventDefault()`, because the modal's own axios `submit`
  handler sits on an ancestor and would otherwise still fire

---

### 1.6 axios loads from a CDN — the whole profile page dies without internet

**File:** `Views/Profile/Index.cshtml:446`
```html
<script src="https://cdn.jsdelivr.net/npm/axios/dist/axios.min.js"></script>
```

**What happens:** You develop offline (or the CDN is blocked on your network). You open My Profile
and **every** card is empty / the browser console says `axios is not defined`. All add/edit/delete
in all five sections is dead — the page cannot render a single list without it.

**Why:** `Views/Profile/Index.cshtml` is the **only** page that loads axios, and it loads it from
the internet. Every other library (jQuery, Bootstrap, validation) is already sitting in
`wwwroot/lib/`, so this one page is the odd one out.

**How to fix:** Copy axios into `wwwroot/lib/axios/axios.min.js` and reference it locally:
```html
<script src="~/lib/axios/axios.min.js"></script>
```

---

### 1.7 Duplicate photos are possible (no database guarantee of "one profile per user")

**Files:** `Controllers/ProfileController.cs:93` and `:113`, `Migrations/20261001060835_InitialCreate.cs`

**What happens:** Your app only *hopes* each user has one profile. If a user double-clicks
*Create Profile* fast enough (or opens two tabs), both requests pass the `Any(...)` check before
either `SaveChanges()` runs, and you get two `Persons` rows for the same `UserId`. From then on
`FirstOrDefault(x => x.UserId == userId)` returns an arbitrary one of them.

**Why:** The check-then-insert happens in application code only. The database has **no unique
index** on `Persons.UserId` — the migration only creates `IX_Addresses_PersonId` and friends.

**How to fix:** Let the database enforce it.
1. In `Data/AppDbContext.cs`, add a fluent config:
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // One profile per user, enforced by the database
    builder.Entity<Person>()
        .HasOne<IdentityUser>()
        .WithOne()
        .HasForeignKey<Person>(p => p.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<Person>()
        .HasIndex(p => p.UserId)
        .IsUnique();
}
```
2. Create the migration:
```bash
dotnet ef migrations add AddUniqueUserIdIndexOnPerson
dotnet ef database update
```
3. (Optional) Wrap the insert so a duplicate gives a friendly message instead of a 500:
```csharp
_context.Persons.Add(profileData);

try
{
    _context.SaveChanges();
}
catch (DbUpdateException)
{
    ModelState.AddModelError(string.Empty, "You already have a profile.");
    return View(ViewModel);
}
```

---

### 1.8 No error handling around `SaveChanges()` — users can see raw exceptions

**Everywhere.** e.g. `ProfileController.Create`, `AddressController.Create`,
`ExperienceController.Create`, and so on. (They are `await _context.SaveChangesAsync()` now — same item.)

**What happens:** Any database hiccup (unique constraint, string too long, connection dropped)
throws `DbUpdateException`. With no `try/catch`, the user gets the developer exception page with
stack trace and table names.

**How to fix:** Two small pieces.

*Option A — per action (best for learning, shows you try/catch + ModelState):*
```csharp
_context.Addresses.Add(addressData);

try
{
    _context.SaveChanges();
}
catch (DbUpdateException ex)
{
    // Log it so you can debug, but show the user something friendly
    var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AddressController>>();
    logger.LogError(ex, "Could not save the address.");

    ModelState.AddModelError(string.Empty, "We could not save your address. Please try again.");
    return BadRequest(ModelState);
}
```

*Option B — once for the whole app.* `Views/Shared/Error.cshtml` already exists, so the remaining
work is the global filter in `Program.cs`. Do this later; per-action is fine for a practice project.

---

### 1.9 The admin profile pager links navigate away from the page

**File:** `Views/Admin/_ProfileList.cshtml:103`, `:114`, `:124`

**What happens:** You are on page 2 of the All Profiles list and click **3**. The browser navigates to
`/Admin/List?page=3` and shows a bare table with no navbar, no filter form and no styling.

**Why:** the pager links only have an `href`, and the AJAX handler is bound to `data-profile-page`:

```javascript
$(document).on('click', '[data-profile-page]', function (e) { ... });
```

`href` alone is a normal link, and `/Admin/List` returns a *partial*. A partial is not a page.
Re-checked on the current file: `data-profile-page` appears **nowhere** in `_ProfileList.cshtml` — only
in the handler at `Views/Admin/Index.cshtml:100`.

**How to fix:** put `data-profile-page` back on each link so the handler catches the click. All three
links need it, not just the numbered ones:

```html
@* Previous *@
<a class="page-link" href="..." data-profile-page="@(currentPage - 1)">Previous</a>

@* Numbers *@
<a class="page-link" href="..." data-profile-page="@i">@i</a>

@* Next *@
<a class="page-link" href="..." data-profile-page="@(currentPage + 1)">Next</a>
```

While you are there: the profile table has filters and pagination but **no sorting** yet. The list is
newest-first only (`AdminController.cs:74` hard-codes `OrderByDescending(x => x.CreatedAt)`). The
action already receives `search`, `gender`, `bloodGroup` and `page`, so a `sort` / `dir` pair plus an
`OrderBy` switch and clickable `<th>`s is a small next step.

---

### 1.10 The `+ Add` buttons on the profile page are commented out (new)

**File:** `Views/Profile/Index.cshtml` — lines 86, 103, 120, 131 and 146

```cshtml
@* <a asp-controller="Experience" asp-action="Create" class="btn btn-sm btn-primary">+ Add</a> *@
```

**What happens:** Nothing visibly breaks — the modals still work. But every "add" entry point on the
profile page is dead markup, so a new section has no visible way to start.

**Why:** When each section moved to AJAX, the standalone link was commented out because
`/Experience/Create` (and the other four) return 415 — see **1.1**. So the comment is hiding a real
bug rather than removing a redundant feature. Once 1.1 is fixed or the pages are deleted, the buttons
should come back as plain `data-*` triggers for the modals.

**How to fix:** once 1.1 is resolved, replace the commented anchor with the modal trigger the section
already uses:

```html
<button type="button" class="btn btn-sm btn-primary"
        data-bs-toggle="modal" data-bs-target="#experienceCreateModal">
    + Add
</button>
```

---

These are the ones to understand properly, because they are the difference between a practice app and
something you could not put online.

### 2.1 No anti-forgery (CSRF) validation on any POST

**Files:** every `[HttpPost]` action in the project, plus `Program.cs`.

**What happens:** ASP.NET Core ships an HTML hidden token in forms and checks it on POST. This
project generates nothing and validates nothing, and `Program.cs` never registers the middleware:
```csharp
// Program.cs — no app.UseAntiforgery() anywhere
app.UseAuthentication();
app.UseAuthorization();
```

**Why it matters:** Any other website the user is logged in to can silently submit a POST to your
site (a hidden form posting to `/Profile/DeleteConfirmed`, for example) and the browser will send
the auth cookie along with it. The server can't tell the request came from your site or from an
attacker's page.

**How to fix:**
1. `Program.cs` — add the middleware after routing:
```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();          // <-- add this
```
2. Add `[ValidateAntiForgeryToken]` to every POST action. Shortcut so you don't forget:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(...) { }
```
3. For the `axios` calls, send the token in a header. Put the token on the page once:
```html
@* in the form that renders the modals, or in _Layout *@
<form id="af-token-form">
    @Html.AntiForgeryToken()
</form>
```
```javascript
// one helper at the top of your script section
function getToken() {
    return $('#af-token-form input[name="__RequestVerificationToken"]').val();
}

axios.post(url, data, {
    headers: { 'RequestVerificationToken': getToken() }
});
```

> **Lesson:** `<form method="post">` + `[ValidateAntiForgeryToken]` is the default for MVC.
> The reason it is opt-in is API-style JSON endpoints, which use a different mechanism.

---

### 2.2 Anyone can read anyone's full CV by changing the URL

**File:** `Controllers/ProfileController.cs:297-300`
```csharp
[AllowAnonymous]
public async Task<IActionResult> Cv(int id)
{
    var profileData = await _context.Persons
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == id);
```

**What happens:** Not logged in, go to `/Profile/Cv/1`, `/Profile/Cv/2`, `/Profile/Cv/3` … You see
every user's full name, date of birth, gender, religion, blood group, phone numbers, email
addresses, home addresses and social links. The IDs are small sequential integers, so guessing is
trivial. This is called an **IDOR** (Insecure Direct Object Reference).

**Why:** `[AllowAnonymous]` was added on purpose (the README says the CV is meant to be
"shareable"). That is a fine product decision — the bug is that there is **no way to tell public
profiles from private ones**.

**How to fix:** Add a "published" switch so a user chooses.
1. `Models/Person.cs`:
```csharp
public bool IsPublic { get; set; } = false;   // default private
```
2. `dotnet ef migrations add AddIsPublicToPerson` then `dotnet ef database update`.
3. `ProfileController.Cv`:
```csharp
[AllowAnonymous]
public IActionResult Cv(int id)
{
    // Only serve the CV if the owner marked it public
    var profileData = _context.Persons
        .FirstOrDefault(x => x.Id == id && x.IsPublic);

    if (profileData == null)
    {
        return NotFound();
    }
    // ...
}
```
4. Add a checkbox on `Views/Profile/Edit.cshtml` and save it in `ProfileController.Edit`.

Extra credit: a **share token** is the nicer version — generate a random GUID per profile and make
the URL `/Profile/Cv/{token}`. Random 128-bit tokens cannot be guessed at all.

---

### 2.3 First registered user silently becomes Admin

**File:** `Controllers/AccountController.cs:44, 70-77`
```csharp
bool usersAlreadyExist = await _userManager.Users.AnyAsync();
...
if (usersAlreadyExist) { await _userManager.AddToRoleAsync(user, "User"); }
else                  { await _userManager.AddToRoleAsync(user, "Admin"); }
```

**What happens:** The rule is "no users yet → this one is Admin". Now imagine you delete every
account from the user-management screen (or you restore an empty database backup, or a teammate
shares a fresh copy of the DB). The **next** random person who registers becomes the Admin, and
from that moment they can read and delete every profile and every user.

**How to fix (intern-friendly):** never decide roles based on a count. Pick the admin explicitly.
```csharp
// Only this specific email is ever an admin
const string AdminEmail = "admin@yourcompany.com";

var isAdmin = string.Equals(email, AdminEmail, StringComparison.OrdinalIgnoreCase);

await _userManager.AddToRoleAsync(user, isAdmin ? "Admin" : "User");
```
You already reference the `"Admin"` role in six places, so this fits your existing style.

---

### 2.4 Login uses the email in the *username* parameter

**File:** `Controllers/AccountController.cs:106`
```csharp
var result = await _signInManager.PasswordSignInAsync(ViewModel.Email, ViewModel.Password, ...);
```

**What happens:** It works right now — but only by luck. The first parameter of
`PasswordSignInAsync` is **`userName`**, not email. Your `Register` action happens to set
`UserName = email`, so the two match. The moment you ever let a user change their username (and
look at `Views/Account/EditProfile.cshtml:22-26`, that is clearly on your roadmap) **nobody will be
able to log in**.

**How to fix:** be explicit about which one you mean.
```csharp
var user = await _userManager.FindByEmailAsync(email);

if (user == null)
{
    ModelState.AddModelError(string.Empty, "Invalid email or password.");
    return View();
}

var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);
```

While you are here, notice two things that were commented out in `Program.cs:20-25`:
- `lockoutOnFailure: false` means **account lockout is switched off entirely**. Nobody can brute-force
  your login, but nobody gets locked out either. Setting it to `true` turns on Identity's built-in
  protection, and you can tune the attempt count with `options.Lockout.MaxFailedAccessAttempts`.
- There is no `services.Configure<IdentityOptions>(...)`, so passwords only need 6 characters and
  **no digit and no uppercase letter**. That is Identity's default, not something you chose.

---

### 2.5 Uploads: no size limit, and file paths are not checked

**File:** `Services/ImageService.cs`

**a) No maximum file size.** `SaveImage` only checks the extension (line 27). A user can upload a
2 GB file and it will be written into `wwwroot`, filling the disk.

```csharp
private const long MaxBytes = 2 * 1024 * 1024;   // 2 MB

if (file.Length > MaxBytes)
{
    return null;   // or throw a friendly validation error
}
```
You can also cap it globally in `Program.cs`:
```csharp
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 2 * 1024 * 1024;
});
```

**b) Extension-only checking.** `IsAllowedImage` trusts `Path.GetExtension(file.FileName)` only.
The real fix is to look at the first bytes of the file (the "magic number"), but for a practice app
the extension check plus the size limit above is a reasonable stopping point.

**c) `DeleteImage` does not confirm the path is inside `wwwroot`.** Lines 59-60:
```csharp
string cleanedPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
string fullPath = Path.Combine(_env.WebRootPath, cleanedPath);
```
`TrimStart('/')` only removes forward slashes, not `..`. Today `PhotoUrl` can only be a value your
own code produced, so there is no live exploit — but the moment a URL is ever user-supplied, a value
like `../../appsettings.json` would delete a real project file.

```csharp
public void DeleteImage(string? relativePath)
{
    if (string.IsNullOrEmpty(relativePath)) return;

    var webRoot = Path.GetFullPath(_env.WebRootPath);
    var fullPath = Path.GetFullPath(Path.Combine(webRoot, relativePath.TrimStart('/', '\\')));

    // Only delete if the file really is inside wwwroot/uploads
    if (!fullPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase)) return;
    if (!File.Exists(fullPath)) return;

    File.Delete(fullPath);
}
```
Also only allow deleting from the `uploads` folder, not all of `wwwroot`.

---

## 3. Dead Code to Clean Up

Dead code is not a style opinion — it is code that future-you will read, try to fix, and get confused
by. Removing it is a real skill.

### 3.1 Eleven view files that can never be rendered

| File | Why it is dead |
|---|---|
| `Views/Address/Edit.cshtml` | `AddressController.Edit(int id)` returns `PartialView("_AddressEditForm", ...)` (`AddressController.cs:152`), never `View(...)` |
| `Views/Address/Delete.cshtml` | `AddressController.Delete(int id)` returns `PartialView("_AddressDeleteForm", ...)` (`:249`) |
| `Views/Contact/Edit.cshtml` | `ContactController.Edit(int id)` returns `PartialView("_ContactEditForm", ...)` (`ContactController.cs:144`) |
| `Views/Contact/Delete.cshtml` | `ContactController.Delete(int id)` returns `PartialView("_ContactDeleteForm", ...)` (`:237`) |
| `Views/Education/Edit.cshtml` | `EducationController.Edit(int id)` returns `PartialView("_EducationEditForm", ...)` (`EducationController.cs:144`) |
| `Views/Education/Delete.cshtml` | `EducationController.Delete(int id)` returns `PartialView("_EducationDeleteForm", ...)` (`:231`) |
| `Views/Experience/Edit.cshtml` | `ExperienceController.Edit(int id)` returns `PartialView("_ExperienceEditForm", ...)` (`ExperienceController.cs:146`) |
| `Views/Experience/Delete.cshtml` | `ExperienceController.Delete(int id)` returns `PartialView("_ExperienceDeleteForm", ...)` (`:235`) |
| `Views/SocialLink/Edit.cshtml` | `SocialLinkController.Edit(int id)` returns `PartialView("_SocialLinkEditForm", ...)` (`SocialLinkController.cs:132`) |
| `Views/SocialLink/Delete.cshtml` | `SocialLinkController.Delete(int id)` returns `PartialView("_SocialLinkDeleteForm", ...)` (`:207`) |
| `Views/UserManagement/Delete.cshtml` | The user delete confirmation moved into a modal, so `UserManagementController.Delete` now returns `PartialView("_UserDeleteForm", ...)` |

The last six arrived with the Experience / Social Link AJAX refactor. The pattern is now fully
consistent: **every section's `Edit` and `Delete` GET returns a partial**, so **all ten** of those
`Edit.cshtml` / `Delete.cshtml` files are dead. Only the five `Create.cshtml` pages are still
reachable — and they are broken (415, see **1.1**).

The dead views are also misleading: each one contains a real `<form method="post">` posting to an
action that has `[FromBody]` on it, so if someone ever did restore the controller's `View(...)` call
they would get an instant 415. Delete the views, not the controllers.

**How to check for dead views yourself:** put a breakpoint in the action, or search for the file name.
If nothing references it, delete it. Git keeps history if you change your mind.

### 3.2 Two unused partials

`Views/Profile/_Addresses.cshtml` and `Views/Profile/_Contacts.cshtml` are only referenced from
commented-out lines (`Views/Profile/Index.cshtml:80` and `:97`). The data is now loaded via AJAX.
Both files still exist on disk. Delete both.

### 3.3 ~330 lines of commented-out JavaScript

`Views/Profile/Index.cshtml` holds a full second copy of the Address + Contact scripts, wrapped
in `@* ... *@`, in the *vanilla JavaScript* style next to the live *jQuery* version. It now runs from
line 1192 to line 1521. Git already remembers it — delete it. Keeping two copies of the same logic
is how bugs get fixed in one place and not the other.

While you are there, consider moving the remaining JS **out of the view** into `wwwroot/js/site.js`
(or a new `wwwroot/js/profile.js`). The file is now **1523 lines** and hard to read. In the layout you
already have the hook for it: the `@await RenderSectionAsync("Scripts", ...)` line.

There is also commented-out Razor left in the same view — the `<partial name="_Addresses">` /
`_Contacts` tags (lines 80, 97, 114) and the five commented `+ Add` links (lines 86, 103, 120, 131,
146). Those last five are not just noise; they hide the bug in **1.10**.

### 3.4 Stray file

| Item | Note |
|---|---|
| `Controllers/AdminController.cs` | `using static Microsoft.EntityFrameworkCore.DbLoggerCategory;` was unused and has been removed. |
| `.gitignore:5` | Reads `*.suoEditProfileViewModel`. Looks like an accidental edit; `*.suo` on line 4 already covers it. Should just be `*.suo`. |

`wwwroot/js/site.js` used to be empty apart from the template comments. It now holds the shared
`showToast()` helper that the profile page, the admin list and the users page all call, so item 3.4 is
done for that file.

### 3.5 The `Deleted` soft-delete flag exists but nothing uses it

Every model has `public bool Deleted { get; set; } = false;`, and the soft-delete block in
`ProfileController.DeleteConfirmed` is commented out (`ProfileController.cs:268`). So deletes are
hard deletes and the flag is always `false`.

This is a trap: if you ever *do* set `Deleted = true`, **no query filters on it**, so the "deleted"
rows keep showing up everywhere. Either finish it properly (add a global query filter) or remove the
flag until you need it. Half-implemented soft delete is worse than none.

---

## 4. Improvements (good learning exercises)

### 4.1 Use async all the way down — **FIXED**

You already `await` Identity calls, then call the database **synchronously** right next to them:
```csharp
var result = await _userManager.CreateAsync(user, password);   // async  ✓
bool usersAlreadyExist = _userManager.Users.Any();             // blocking ✗
```

| Sync (before) | Async (now) |
|---|---|
| `_context.SaveChanges()` | `await _context.SaveChangesAsync()` |
| `.FirstOrDefault(x => ...)` | `await .FirstOrDefaultAsync(x => ...)` |
| `.Any(x => ...)` | `await .AnyAsync(x => ...)` |
| `.Count(x => ...)` | `await .CountAsync(x => ...)` |
| `.Where(...).ToList()` | `await .Where(...).ToListAsync()` |
| `_userManager.Users.ToList()` | `await _userManager.Users.ToListAsync()` |

Every action in `Profile`, `Address`, `Contact`, `Education`, `Experience`, `SocialLink`, `Admin`,
`UserManagement` and `Account` is now `async Task<IActionResult>` and awaits every database call.
`Microsoft.EntityFrameworkCore` was added to those `using` lists.

Two things that were deliberately **left sync** on purpose:

- **No `CancellationToken` parameters.** Real apps pass one so a query stops when the user navigates
  away, but that is extra plumbing with no visible benefit here. You can add it later — the pattern is
  `public async Task<IActionResult> Index(CancellationToken cancellationToken)` plus
  `ToListAsync(cancellationToken)`.
- **`ImageService.SaveImage` / `DeleteImage`** still use `File.Copy` / `File.Delete`. They hit the
  disk, not the database, and the `IImageService` interface would have to change to support async
  file APIs.

The remaining plain `IActionResult` actions (`Account.Register`, `Account.Login`,
`Account.AccessDenied`, `Account.ChangePassword` GET, and all of `HomeController`) touch no
database at all — they just `return View()` or redirect, so there is nothing to await.

### 4.2 Add `AsNoTracking()` to read-only queries — **FIXED**

EF Core tracks every entity it returns so it can write changes back. On pages that only *display*
data, that tracking is wasted memory and time.

```csharp
// ProfileController.Index — we only read
var profileData = await _context.Persons
    .AsNoTracking()
    .FirstOrDefaultAsync(x => x.UserId == userId);
```

Now applied in: `ProfileController.Index`, `ProfileController.Cv`, `AdminController.Index`,
`AdminController.Details`, `UserManagementController.Index` (each of those also loads its addresses,
contacts, education, experience and social-link rows with `AsNoTracking()`).
**Not** used anywhere that then calls `SaveChangesAsync` (the Create / Edit / Delete actions).

Two notes from doing it:

- `AccountController.Profile` was on the original list but needed no change. It reads through
  `_userManager.GetUserAsync(User)`, which is Identity's own store query, not one of your
  `AppDbContext` queries — there is nothing to put `AsNoTracking()` on.
- Other read-only queries could also use it later if you want: the `List` actions and the GET
  `Edit` / `Delete` actions in the five section controllers only fill a ViewModel and never save.

### 4.3 `ProfileController.Index` makes 6 database round-trips — and now loads 5 lists nobody uses

`ProfileController.cs:43-79` loads the person (1 query), then 5 more separate queries for addresses,
contacts, education, experience and social links. Each one is a network round-trip to SQL Server.

**This got worse with the AJAX refactor, and in a way worth understanding.** The five collections
are assigned to the ViewModel, but `Views/Profile/Index.cshtml` no longer renders any of them — the
five `@* <partial name="_Addresses"> *@`-style tags are commented out and each section is now filled
in by its own `List` partial over axios. Verified: the only `Model.*` references left in the view are
`Id`, `PhotoUrl`, `FullName`, `Gender`, `DateOfBirth`, `BloodGroup`, `Religion`, `Summary` and the
four completeness properties.

So those five `ToListAsync()` calls pull every child row of the profile into memory on **every page
load**, and the view throws four of the five away. The only consumer left is
`ProfileCompletenessService.Fill`, and it just asks `.Count > 0`.

Two ways to fix, in order of how much they teach:

**Option 1 — keep the shape, fetch only what is used.** Replace each `ToListAsync()` with a count
check. Five list loads become five `EXISTS` queries that return one row each:

```csharp
Addresses   = await _context.Addresses.AsNoTracking().AnyAsync(x => x.PersonId == profileData.Id),
Contacts    = await _context.Contacts.AsNoTracking().AnyAsync(x => x.PersonId == profileData.Id),
Educations  = await _context.Educations.AsNoTracking().AnyAsync(x => x.PersonId == profileData.Id),
Experiences = await _context.Experiences.AsNoTracking().AnyAsync(x => x.PersonId == profileData.Id),
SocialLinks = await _context.SocialLinks.AsNoTracking().AnyAsync(x => x.PersonId == profileData.Id)
```
Then change the five properties on `ProfileIndexViewModel` from `List<Address>` to `bool`, and
`ProfileCompletenessService` from `.Count > 0` to just `if (viewModel.Addresses)`. That also lets you
delete `using CustomerManagementPractiseCS.Models;` from the ViewModel — the view model no longer
exposes EF entities at all, which is the point of having a view model.

**Option 2 — cut the round-trips with `Include`.** If you keep the collections, at least fetch them
in one go:
```csharp
var profileData = await _context.Persons
    .AsNoTracking()
    .Include(p => p.Addresses)
    .Include(p => p.Contacts)
    .Include(p => p.Educations)
    .Include(p => p.Experiences)
    .Include(p => p.SocialLinks)
    .FirstOrDefaultAsync(x => x.UserId == userId);
```
The `OrderByDescending` calls stay as they are — sorting is still done in SQL.

> Take **Option 1**. The AJAX refactor already moved the rendering out of the view, so the server
> loading the data to hand to the view is simply the last piece of the old design left behind.
> Option 2 optimises code that should not be running at all.

### 4.4 Stop repeating the same "find my profile" block

This exact block appears **36 times** across the five section controllers
(6 in `AddressController`, 7 in `ContactController`, 7 each in `EducationController`,
`ExperienceController` and `SocialLinkController`) — it went up by 5 when the `List` actions were
added:

```csharp
var userId = _userManager.GetUserId(User);
var profileData = _context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);
if (profileData == null) { return NotFound(); }
```

Fix it once with a base controller:
```csharp
// Controllers/ProfileOwnerController.cs
[Authorize]
public abstract class ProfileOwnerController : Controller
{
    protected readonly AppDbContext Context;
    protected readonly UserManager<IdentityUser> UserManager;

    protected ProfileOwnerController(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        Context = context;
        UserManager = userManager;
    }

    // Returns the signed-in user's profile, or null if they have not created one yet
    protected async Task<Person?> GetMyProfileAsync()
    {
        var userId = UserManager.GetUserId(User);
        return await Context.Persons.FirstOrDefaultAsync(x => x.UserId == userId);
    }
}
```
Then each action becomes one line:
```csharp
var profile = await GetMyProfileAsync();
if (profile == null) return NotFound();
```
Learn the name for this: **DRY** (Don't Repeat Yourself).

### 4.5 You already delete children manually — the database does it for free

`ProfileController.DeleteConfirmed:268-282` and `UserManagementController.DeleteConfirmed:90-107`
both do this:

```csharp
var addresses = _context.Addresses.Where(x => x.PersonId == profileData.Id).ToList();
var contacts  = _context.Contacts.Where(x => x.PersonId == profileData.Id).ToList();
// ... 3 more queries ...
_context.Contacts.RemoveRange(contacts);
// ... 5 more RemoveRange calls ...
_context.SaveChanges();
```

That is 10 queries plus 10 `RemoveRange` calls. But your migration already created every child
foreign key with `onDelete: ReferentialAction.Cascade`
(`Migrations/20261001060835_InitialCreate.cs:212, 240, 272, 305, 331`). So deleting the `Person` row
deletes all children automatically:

```csharp
_context.Persons.Remove(profileData);
await _context.SaveChangesAsync();   // children go with it
```

5 queries and 1 delete becomes 1 query and 1 delete. You only still need `_imageService.DeleteImage(...)`
because files on disk are not in the database.

### 4.6 Use a ViewModel for Register and Login — **FIXED**

`AccountController.Register(string email, string password)` and `Login(string email, string password)`
took loose strings, so there was **no** `[Required]`, `[EmailAddress]`, `[StringLength]` or
`[DataType]` validation. If the field was missing, `password` was `null` and Identity returned a
confusing error instead of "Password is required".

Added `ViewModels/RegisterViewModel.cs` and `ViewModels/LoginViewModel.cs`:
```csharp
public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```
Both actions now take the ViewModel, check `ModelState` **before** touching the database, and
`return View(ViewModel)` so a rejected form comes back with the fields still filled in.
`Views/Account/Register.cshtml` gained the `Confirm Password` field with
`<span asp-validation-for="ConfirmPassword">`, and both views now use `asp-for` tag helpers plus
`<partial name="_ValidationScriptsPartial" />` so the checks also run in the browser.
`asp-validation-summary` was switched from `All` to `ModelOnly` — otherwise every field message
would show twice (once in the summary, once under the input).

> Note: the login `Password` has `[Required]` but no `[StringLength]`, deliberately. The length rule
> is a *registration* rule; on login it would only reject passwords that Identity already accepted.

### 4.7 Move the connection string out of `appsettings.json`

`.gitignore:29-31` says *"never commit real connection strings"* and ignores
`appsettings.Development.json` — but the **live** connection string is committed in
`appsettings.json:11`. Move it to user secrets (never committed, per-machine):

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=CustomerManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
```
Then in `appsettings.json` leave an empty placeholder:
```json
"ConnectionStrings": { "DefaultConnection": "" }
```
Note that user secrets do not work in production — they are a **development** tool. That is exactly
right for where your project is now.

### 4.8 Add pagination to the Admin profile list — **FIXED**

`AdminController.Index` used to load **every** profile into memory (`.ToList()`). With 10 profiles
that is fine; with 10,000 it is a slow page and a lot of RAM.

```csharp
public async Task<IActionResult> Index(string search, string gender, int page = 1)
{
    const int pageSize = 20;

    var query = _context.Persons.AsNoTracking().AsQueryable();

    if (!string.IsNullOrEmpty(search)) query = query.Where(x => x.FullName.Contains(search));
    if (!string.IsNullOrEmpty(gender)) query = query.Where(x => x.Gender == gender);

    var total = await query.CountAsync();
    var profiles = await query
        .OrderByDescending(x => x.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(x => new AdminProfileListViewModel { ... })
        .ToListAsync();

    ViewBag.Total = total;
    ViewBag.Page = page;
    return View(profiles);
}
```

`Skip`/`Take` become `OFFSET`/`FETCH` in SQL — that is how SQL Server pages. Three details that
matter:

1. **Count before paging.** `CountAsync()` runs on the filtered query *before* `Skip`/`Take`, so
   the view knows the real number of pages.
2. **Clamp the page number.** It comes straight from the URL, so `/Admin?page=999` would make
   `Skip` walk past the end of the list. `page` is forced into `1..totalPages`.
3. **Keep the filters on the links.** Every page link re-sends `search` and `gender`, otherwise
   page 2 of a filtered list silently drops the filter.

`Views/Admin/_ProfileList.cshtml` now draws the Bootstrap pager, and the filters were widened to
**name**, **gender** and **blood group**. Since then the whole table was also moved behind AJAX: `Index`
only draws the filter form, and `List` returns the table as a partial that the page drops into
`#profiles-container` (5 profiles per page). Paging through 10,000 rows still draws 7 buttons, not 500.

> Two follow-ups came out of that work: the pager links lost their `data-profile-page` attribute (see
> **1.9** above), and sorting is not in yet.

### 4.9 Add a test project

There are zero tests, and this project is mostly *business rules* that are perfect to test:
ownership checks, the primary-address rule, completeness percentage, file extension validation.

Start with the cheapest, highest-value one — `ImageService`:
```csharp
[Fact]
public void IsAllowedImage_ReturnsTrue_ForJpg()
{
    var service = new ImageService(fakeEnv);
    Assert.True(service.IsAllowedImage("photo.jpg"));
}

[Fact]
public void IsAllowedImage_ReturnsFalse_ForExe()
{
    var service = new ImageService(fakeEnv);
    Assert.False(service.IsAllowedImage("virus.exe"));   // must be rejected
}
```
```bash
dotnet new xunit -n CustomerManagementPractiseCS.Tests
dotnet add CustomerManagementPractiseCS.Tests reference CustomerManagementPractiseCS.csproj
```
For controller tests, `WebApplicationFactory<Program>` lets you spin the whole app up in memory.

### 4.10 Smaller things worth doing

| Item | Where |
|---|---|
| Prevent an admin from deleting **another admin** | `UserManagementController.DeleteConfirmed` — the guard only blocks deleting *yourself* |
| Add `app.UseStatusCodePages()` | `Program.cs` — turns bare 404/403 into a friendly page |
| Add `[ResponseCache(NoStore = true)]` to pages showing private data | `ProfileController.Index`, `AdminController.Details` — stops the browser caching personal data |
| Use `DateTimeOffset` instead of `DateTime` for `CreatedAt`/`UpdatedAt` | all models — `DateTime` has no time zone, so it is ambiguous |
| Add `[Range]` validation to years and years/dates | `Education.StartYear`/`EndYear`, `Experience.StartDate`/`EndDate` — nothing stops `EndYear = 1990` with `StartYear = 2020` |
| Stop rendering "0" for empty optional years | `Views/Profile/Index.cshtml:133` prints `@edu.StartYear` raw |

---

## 5. Things You Already Did Well

Worth saying out loud, because these are the habits that make the rest of the code reviewable:

1. **Ownership checks on every query.** `x.Id == id && x.PersonId == profileData.Id` appears
   consistently across all five section controllers. This is the single most important thing in the
   whole project and you got it right everywhere. The comment at `AddressController.cs:177` even
   explains *why* in plain English.
2. **ViewModels everywhere.** No entity class is bound directly to a form, so over-posting
   (`CreatedBy`, `Deleted`, `PersonId`) is impossible. Most practice projects skip this.
3. **`[ValidateAntiForgeryToken]`-ready POST-only Logout** (`AccountController.cs:134`) instead of a
   GET link — many real apps get this wrong.
4. **Real DI + a service layer.** `IImageService` behind an interface and registered with
   `AddScoped` is exactly the right shape, and it is why 1.1 and 2.5 can be fixed in one file.
   `IProfileCompletenessService` is the same idea, and keeping the completeness math *out* of the
   controller and the view is why 4.3 is a one-file change.
5. **Audit fields** (`CreatedAt` / `CreatedBy` / `UpdatedAt` / `UpdatedBy`) on every entity, set
   consistently.
6. **Friendly validation messages.** `AccountController.Login` shows one generic
   "Invalid email or password" instead of leaking which half was wrong.
7. **Database-level cascades** in the migration — you did this even before you needed it.
8. **An excellent README** with a mermaid ER diagram, a routes table, and an honest
   *Known Limitations* section. Two items I found in this review (2.2 public CV, 4.6 Register
   view model) were already listed there — you documented your own gaps correctly.
9. **Genuinely useful inline comments** explaining *why*, not *what*. That is a professional habit.
10. **The AJAX pattern was applied consistently.** When Experience and Social Link were converted,
    all five sections ended up with the same shape: a `List` GET returning a partial, three `[FromBody]`
    POSTs returning `{ message }`, and a `showToast()` on success. Copying a *pattern* rather than
    inventing a second one is exactly the habit that makes a codebase grow safely.

> A note on the comments: several have since been trimmed to the bare minimum. The rule of thumb is one
> comment per *why*, not one per line — long explanations go in this document, not in the code.

---

## 6. Suggested Order of Work

Do them in this order — each one is small, and each one teaches something.

### Week 1 — correctness
1. **1.1** Fix or delete the five `/…/Create` pages (415 bug)
2. **1.2** Show errors in all 15 AJAX modals instead of `console.error` — and finish the one that
   already started (real message, remove-on-retry, no fixed `id`)
3. **1.3** Remove `TempData` from the AJAX endpoints — **done**
4. **1.4** `[Required]` on non-nullable `DateOnly` / `int` — still open
5. **1.9** Put `data-profile-page` back on the admin pager links
6. **1.10** Un-comment the five `+ Add` buttons once 1.1 is resolved

### Week 2 — security basics
7. **2.1** Add `app.UseAntiforgery()` + `[ValidateAntiForgeryToken]` on POSTs
8. **2.3** Replace first-user-is-admin with an explicit admin email
9. **2.4** Fix `PasswordSignInAsync`, turn lockout on
10. **2.5** Add a 2 MB upload limit + the `wwwroot` path guard

### Week 3 — code quality
11. **3.x** Delete the dead code (11 views, 2 partials, the commented JS, the `.gitignore` typo)
12. **4.4** Extract the base controller — removes 36 copy-pasted blocks
13. **4.5** Delete the manual child-removal loops and rely on the cascade
14. **4.3** Replace the five `ToListAsync()` calls in `ProfileController.Index` with `AnyAsync` —
    the lists are no longer rendered, only counted

### Week 4 — grow
15. **4.1** Go async everywhere — **done**
16. **4.6** `RegisterViewModel` / `LoginViewModel` with `[Compare]` for confirm-password — **done**
17. **2.2** Add `IsPublic` to `Person` so CVs are private by default
18. **4.9** Create the test project and write 5 tests around `ImageService` + ownership checks

Also done along the way: **1.5** (the contact validation script), **4.2** (`AsNoTracking()`) and
**4.8** (admin pagination).

---

*First reviewed against commit state on 2026-10-04. Re-verified line by line on 2026-10-05 against
`dd43aa1` plus the working-tree changes — `dotnet build` clean, 0 errors. Every file path, line number
and count in this document was re-checked against the source on that date.*