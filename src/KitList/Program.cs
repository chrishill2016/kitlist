using KitList;
using KitList.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var firebaseOptions = new FirebaseOptions();
builder.Configuration.GetSection("Firebase").Bind(firebaseOptions);
builder.Services.AddSingleton(firebaseOptions);
builder.Services.AddSingleton<FirebaseJs>();

// Without Firebase settings the app runs in demo mode, storing data in this browser only.
if (firebaseOptions.IsConfigured)
    builder.Services.AddSingleton<IDocumentStore, FirestoreDocumentStore>();
else
    builder.Services.AddSingleton<IDocumentStore, LocalDocumentStore>();

builder.Services.AddSingleton<KitRepository>();
builder.Services.AddSingleton<AuthService>();

await builder.Build().RunAsync();
