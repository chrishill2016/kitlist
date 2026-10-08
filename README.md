# Club Kit

A small installable web app (PWA) for keeping track of a diving club's kit: BCDs, regulators, cylinders and fins.

- Kit list with search and category filters
- Details for each item: make, model, serial number, size, condition, location, purchase date and cost, notes and photos
- Service schedules for each item (for example, cylinder visual inspection every 12 months and hydrostatic test every 60 months), with a service history
- **Session sheets** that replace the weekly spreadsheet. Each sheet lists who has which cylinder, BCD, fins and reg, with start and end pressures:
  - Start pressures carry over from each cylinder's last end pressure.
  - Kit given to two people, or kit that needs repair or is overdue for service, is flagged.
  - The sheet can be printed, or shared to WhatsApp or email as text.
- A dashboard showing what's overdue, what's due in the next 30 days, and what needs repair
- Google sign-in, with members set up as **Viewer**, **Editor** or **Admin**

It's written in C# with Blazor WebAssembly, so it runs entirely in the browser and is hosted free on GitHub Pages. Data is stored in Firebase (Firestore), and sign-in uses Firebase Authentication. Both are free on Firebase's Spark plan.

## Demo mode

If no Firebase settings are filled in, the app runs in **demo mode** with sample data that's saved only in your browser. Use it to try the app before setting up Firebase.

## Setup

### 1. Turn on GitHub Pages

In the GitHub repo, go to **Settings → Pages** and set **Source** to **GitHub Actions**. Every push to `main` then builds, tests and deploys the app to `https://<your-user>.github.io/kitlist/`.

### 2. Create a Firebase project (free)

1. Go to the [Firebase console](https://console.firebase.google.com/) and add a project. You don't need Google Analytics, and the free Spark plan is enough.
2. **Turn on Google sign-in.** Go to **Build → Authentication**, click **Get started**, open **Sign-in method**, and enable **Google**.
3. **Allow the site to sign in.** In **Authentication → Settings → Authorized domains**, add your GitHub Pages domain, for example `chrishill2016.github.io`.
4. **Create the database.** Go to **Build → Firestore Database**, click **Create database**, choose a location near you (such as `europe-west2` for London), and start in **production mode**.
5. **Set the security rules.** In **Firestore → Rules**, replace the contents with [`firestore.rules`](firestore.rules) and click **Publish**.
6. **Register the web app.** Go to **Project settings → General → Your apps**, click the web icon `</>` and register an app (you don't need Firebase Hosting). Copy `apiKey`, `authDomain`, `projectId` and `appId` into [`src/KitList/wwwroot/appsettings.json`](src/KitList/wwwroot/appsettings.json), then commit and push.
   These values identify the project but aren't secret. Access is controlled by the security rules and the authorized domains.

### 3. Make yourself the first admin

In **Firestore → Data**, click **Start collection**:

- Collection ID: `members`
- Document ID: your Google email address in **lower case**, for example `you@gmail.com`
- Fields (all strings): `email` = your address, `name` = your name, `role` = `Admin`

Sign in to the app and add everyone else from the **Members** page:

| Role   | Can do |
|--------|--------|
| Viewer | See kit and service history |
| Editor | Also add, edit and delete kit, record services and add photos |
| Admin  | Also manage members |

## Notes

- **Photos** are shrunk in the browser and stored in Firestore, because Firebase Storage needs a paid plan. Each photo is about 100–200 KB, which suits a club-sized inventory.
- **Kit numbers:** session sheets show cylinders, BCDs and regs by their name, so name them by their club number (`1`, `2`…). Fins are shown by size. A cylinder's size is shown next to its number when one is set, e.g. `2 (10L)`.
- **Service intervals** are only starting defaults. Change them per item to match your club's rules or your country's test regime.
- Firestore's free tier allows 50,000 reads and 20,000 writes a day. Roughly 50 items and a handful of users is well within that.

## Developing

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```sh
dotnet run --project src/KitList   # serves the app locally (in demo mode until Firebase is configured)
dotnet test                        # runs the unit tests
```

To use Google sign-in locally, `localhost` must be in Firebase's authorized domains (it is by default).

| Path | What's there |
|------|--------------|
| `src/KitList/Models` | Kit, member and service-schedule types, and the due-date rules |
| `src/KitList/Services` | Data access (Firestore or demo localStorage) and sign-in |
| `src/KitList/Pages` | Dashboard, session sheets, kit list, kit details and edit page, and members page |
| `src/KitList/wwwroot/js` | Small JavaScript wrappers for the Firebase SDK and photo resizing |
| `firestore.rules` | Database security rules |
| `.github/workflows/deploy.yml` | Build, test and deploy to GitHub Pages |
