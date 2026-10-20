# LeltárKezelő – .NET MAUI teszt feladat

Egyszerű leltáralkalmazás: lokális SQLite adatbázisban tárolt termékek, keresése, listázása, felvétele, szerkesztése és törlése.

## Funkciók

| Követelmény | Megvalósítás |
|---|---|
| Lokális SQLite | `sqlite-net-pcl`, `ProductRepository` (WAL mód, lusta, szálbiztos inicializálás) |
| Terméklista | `CollectionView` virtualizációval, `MeasureFirstItem` méretezéssel, **lapozott betöltéssel** (50 elem/oldal, `RemainingItemsThreshold`) |
| Több ezer elem | Első indításkor 5000 demótermék generálódik; a lekérdezés `LIMIT/OFFSET`-tel, indexelt oszlopon rendezve fut |
| 0 készlet jelölése | Halvány piros háttér, piros sáv a sor bal szélén, piros név és mennyiség (`DataTrigger` az `IsOutOfStock`-ra) |
| Keresés | Név **vagy** vonalkód részletre; a névkeresés kis-/nagybetű- és ékezetfüggetlen („kenyer” → „Kenyér”); 300 ms debounce |
| Termék felvétele | Külön oldal (`AddProductPage`), validációval, mentés után azonnal megjelenik a listában |
| Részletező oldal | `ProductDetailPage`: adatok módosítása és mentése, plusz törlés megerősítéssel |

## Architektúra

```
Models/        Product (SQLite entitás)
Data/          adatbázis-konstansok, demóadat-generátor
Services/      IProductRepository + SQLite implementáció, navigáció és dialógus absztrakció
ViewModels/    MVVM (CommunityToolkit.Mvvm source generátorok)
Views/         XAML oldalak, közös ProductFormView űrlap
Messages/      ProductChangedMessage (WeakReferenceMessenger)
Helpers/       szám- és szövegnormalizálás
```

**MVVM + DI.** Minden oldal, ViewModel és szolgáltatás a `MauiProgram`-ban van regisztrálva; a Shell a navigált oldalakat a DI konténerből példányosítja. A ViewModelek csak interfészeken keresztül érik el az adatbázist, a navigációt és a dialógusokat, így UI nélkül is tesztelhetők.

**Navigáció.** Shell route-ok: `addproduct`, `productdetails?id={id}`. A részletező oldal csak az azonosítót kapja meg (`IQueryAttributable`), és friss adatot olvas az adatbázisból – nem a lista objektumát módosítja.

**Állapotkezelés.** A lista ViewModelje singleton, így a keresés és a görgetési állapot navigáció után is megmarad. Mentés/törlés után a form ViewModel egy `ProductChangedMessage`-et küld:
- *Added* → a lista újratölt (a szűrő törlődik, hogy az új termék biztosan látszódjon),
- *Updated* → csak a módosult elem cserélődik helyben (nincs újratöltés, nincs ugrás a lista tetejére),
- *Deleted* → az elem kikerül a listából.

**Validáció.** Kötelező név és vonalkód, egyedi vonalkód (alkalmazás- és adatbázisszinten is: unique index), nem negatív ár és mennyiség. A számmezők tizedesvesszőt és -pontot is elfogadnak.

## Futtatás

1. Visual Studio 2022 (17.12+), .NET MAUI workload, .NET 9 SDK.
2. Indítási cél: **Windows Machine** vagy Android emulátor.
3. Első indításkor létrejön a `leltar.db3` az alkalmazás adatkönyvtárában (`FileSystem.AppDataDirectory`), 5000 demótermékkel. Ez a `Data/DatabaseConstants.cs`-ben kikapcsolható (`SeedDemoData = false`).

## NuGet csomagok

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
<PackageReference Include="sqlite-net-pcl" Version="1.9.172" />
<PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.10" />
```

## Megjegyzések

- A `Product` a feladatban megadott mezőkön felül `Id`-t (elsődleges kulcs) és `SearchName`-et (normalizált, indexelt keresőkulcs) tartalmaz.
- A sqlite-net a `decimal` értékeket REAL-ként tárolja; pénzügyi pontosság kellene, egész fillérként (`long`) vagy szövegként érdemes tárolni.
- Nagyon nagy adatmennyiségnél az `OFFSET` alapú lapozás kiváltható keyset paginationnel (`WHERE SearchName > ?`), a `LIKE '%…%'` keresés pedig FTS5 indexszel.

## Továbbfejlesztési lehetőségek

### Adatkezelés és architektúra
- **Szerveroldali szinkronizáció (offline-first):** az SQLite maradjon helyi gyorsítótár, a változások HTTPS-en keresztül szinkronizálódjanak egy REST API-val. Így az alkalmazás térerő nélkül is használható (raktárban gyakori), az adatok pedig több eszköz között közösek. Ehhez kell: változásnapló.
- **Hitelesítés:** REST API esetén bejelentkezés (pl. OAuth2 / OpenID Connect), a token biztonságos tárolása `SecureStorage`-ban.
- **Több raktár kezelése:** a `Product` csak a törzsadatokat tárolja; a készlet egy külön `Stock` táblába kerül (`ProductId`, `WarehouseId`, `Quantity`), egy `Warehouse` táblával. A listában választható legyen a raktár, vagy jelenjen meg az összesített készlet.
- **Készletmozgás-napló:** a mennyiség közvetlen felülírása helyett bevételezés / kiadás / leltárkorrekció tételek (`StockMovement`: termék, raktár, mennyiség, típus, időpont, felhasználó). Így visszakövethető, ki mikor mit módosított.

### Funkciók
- **Vonalkód-olvasás kamerával** (pl. ZXing.Net.MAUI): keresés és új termék felvétele vonalkód beolvasásával, kézi gépelés helyett.
- **Szűrés és rendezés:** csak a készlethiányos termékek megjelenítése; rendezés név, ár vagy mennyiség szerint.
