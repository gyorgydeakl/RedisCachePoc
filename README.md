
# Lokális futtatás:
## Kliens:

Gyökérmappából:
``` powershell
cd RedisCachePocClient
npm i -f
npm start
```

## Backend:
### Manuálisan:
A Indítsuk el a MovieReviewer http és a MoviePlanner http (nem https) konfigurációt visual studioból vagy más IDE-ből

## Redis
Dockerrel a legegyszerűbb, ha már van a gépen:

``` powershell
docker run --name redis-poc -p 6379:6379 -d redis:latest
```

# Cache működése
A redis lényegében egy in-memory adatbázis, ami kulcs-érték párokat tárol - egy nagy json-t. Mivel ezt memóriában teszi, ezért kevés adatra sokkal gyorsabb, mint egy rendes adatbázis, ami a háttértárat használja.

A cachelést kétféleképpen is meg lehet oldani .NET-ben. 

## Output Caching
A legtöbb cachelés ugyanazzal az algoritmussal működik, és van is olyan beépített módszer a .NET-ben, amivel nem kell ezt manuálisan megírni.

Mindössze az elején be kell konfigurálni
``` CSharp
using StackExchange.Redis; // Ez a nuget package kell!

// ...

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisCache");
});
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromSeconds(30)));
});

// ...

app.UseOutputCache()
```

És mostantól csak meg kell jelölnünk, hogy melyik endpointot akarjuk cachelni:
```CSharp
app
.MapGet("users", async () => GetUsers())
// Itt jelöljük meg, akár felülírhatjuk a default lejárati időt
.CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(10))); 
```

Vagy pedig ha controllerekkel dolgozunk, akkor 
```CSharp
[OutputCache(Duration = 60)]
public IActionResult Index()
{
    return View();
}
```

Innentől kezdve működni fog a cache, ugyanazon endpoint többszörös hívása már a cache-elt értéket fogja visszaadni. 

A háttérben az történik, hogy az alábbi kulcs érték pár kerül be a redis-be:
```json
{
    "__MSOCV_GET\u001eHTTP\u001eLOCALHOST:5273/USERS\u001eQ\u001e*=": "\u0002��������\b\u0000�\u0001\u0002;\u0001>application/json; charset=utf-8A\u0001:Wed, 30 Jul 2025 11:01:58 GMT�\u0018[{\"id\":\"c779dcd4-1b78-412f-8c21-06e8274e71b5\",\"username\":\"Leda.Deckow\",\"bio\":\"Culpa dolores ipsa. Temporibus ut sed quisquam assumenda sunt iste. Veniam porro quo consequatur corrupti nihil molestias consequatur.\",\"email\":\"Maeve_Purdy38@yahoo.com\"},{\"id\":\"6eadcd33-cac8-405e-a652-178c89c91a0e\",\"username\":\"Jensen.Marks\",\"bio\":\"Dignissimos aliquam vel laudantium omnis magnam maxime. Non cumque ratione inventore eius quibusdam ipsam voluptatem eius odio. Consectetur non corrupti. Eveniet consequuntur provident voluptas molestias. Quod quas qui dolor eum.\",\"email\":\"Stephon.Crooks41@gmail.com\"}]"
}
```
Ahogy láthatjuk, a beszúrt érték nem csak egy sima json stringgé alakítva, hanem tartalmaz néhány metaadatot is.

## Manuális caching
Ha egyedi működésre van szükségünk, akkor használhatunk egy sima redis klienst is. Ekkor nekünk kell megmondani, hogy milyen kulcs érték párok kerüljenek be a redis adatbázisba, illetve minden egyéb logikát. 

Mindenek előtt be kell regisztrálni egy `IConnectionMultiplexer` objektumot.
```CSharp
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var options = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("RedisCache")!);
    options.AllowAdmin = true;
    return ConnectionMultiplexer.Connect(options);
});
```

Ezek után tetszőlegesen írhatunk és olvashatunk a redis adatbázisbl kulcs-érték párokat
Például:
```CSharp
app.MapGet("users", async (AppDbContext db, IConnectionMultiplexer redis) =>
{
    const string key = "users:all";

    var cache = redis.GetDatabase();
    string? cachedJson = await cache.StringGetAsync(key);

    if (!string.IsNullOrEmpty(cachedJson))
    {
        var dtoList = JsonSerializer.Deserialize<List<UserDto>>(cachedJson)!;
        return TypedResults.Ok(dtoList);
    }

    var dtoListFromDb = await db.Users.Select(u => u.ToDto()).ToListAsync();

    await cache.StringSetAsync(
        key,
        value: JsonSerializer.Serialize(dtoListFromDb),
        expiry: TimeSpan.FromMinutes(10),
        flags: CommandFlags.FireAndForget);

    return TypedResults.Ok(dtoListFromDb);
})
```


## Manuális és Output caching együttműködése
A POC magában foglalja azt a szituációt, hogy több alkalmazás is használja ugyanazt a cache-t. Ez azonban egy output cachinget használó és egy manuális cachinget használó alkalmazás között nem triviális. A probléma a következő.

Tegyük fel, hogy van 2 alkalmazás, MoviePlanner és MovieReviewer. Mindkét alkalmazás ugyanazt a user listát éri el, de MoviePlanner output cachinget használ, míg MovieReviewer manuális cachinget.

Amikor MoviePlanner elcachel valamit, akkor nem állíthatja be szabadon a kulcsot, a kulcs előre rögzített módszerrel generálódik, specifikusan annak az alkalmazásnak a processére. Pl.: "__MSOCV_GET\u001eHTTP\u001eLOCALHOST:5273/USERS\u001eQ\u001e*=". Ebben a kulcsban benne van a domain és a port is.

Emiatt, ha a MoviewReviewer fel akarja használni a MoviePlanner által cachelt értéket, ezt nem fogja tudni megtenni (vagy csak nagyon hackelős módszerrel), mert a kulcsot nem ismeri vagy nem tudja magának előállítani.
