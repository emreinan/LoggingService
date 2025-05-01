# Logging Broker API – Request Documentation

## 1. POST Endpoint

**URL:**

```
POST http://localhost:3012/api/v1/logs
```

**Headers:**

- Content-Type: application/json

**Request Body Örneği:**

```json
{
  "Source": "test-service",
  "LogLevel": "debug",
  "Message": "Test",
  "Parameters": {
    "user": "test-user",
    "action": "test action"
  },
  "EventUnixTimeMs": 1742498360601
}
```

**Davranış:**

- **Mesaj (Value):** Sadece `Message` alanı, yani “Test” değeri gönderilir.
- **Etiket (Stream Labels):**
    - `Source`, `LogLevel`, `brokerReceivedTimeMs` ve `originalEventTimeMs` bilgileri eklenir.
    - `Parameters` içindeki her anahtar, “param_” önekiyle ayrı etiket olarak kaydedilir (örneğin, `param_user` ve `param_action`).
- Bu sayede, push edilen logun değeri sade metin (mesaj) olarak kalırken, filtreleme amacıyla kullanılacak ek bilgiler etiket olarak eklenmiş olur.

---

## 2. GET Endpoint – Query Parameters

**URL:**

```
GET http://localhost:3012/api/v1/logs
```

**Desteklenen Query Parametreleri (Hepsi Opsiyoneldir):**

- **sources:**
    
    Logların belirli kaynaklardan çekilmesi için kullanılır.
    
    *Örnek:*
    
    `sources=zoom-service|auth-service`
    
    `sources=order-service`
    
- **levels:**
    
    Log seviyelerini filtrelemek için kullanılır.
    
    *Örnek:*
    
    `levels=inf|debug`
    
    `levels=warning`
    
    `levels=warn`
    
    *(Kısa form kullanılsa da “inf” ifadesi, “Information” kayıtlarını da kapsayacak şekilde eşleşme sağlanır.)*
    
- **startTime & endTime:**
    
    Unix milisaniye cinsinden zaman aralığını belirtir.
    
    *Örnek:*
    
    `startTime=1680000000000&endTime=1685000000000`
    
    Bu parametreler belirtilmediğinde varsayılan olarak son 7 günün logları çekilir.
    
- **contains:**
    
    Yalnızca log mesajı (value kısmı) üzerinde arama yapmak için kullanılır.
    
    *Örnek:*
    
    `contains=test`
    
    `contains=test|log`
    
- **caseInsensitive:**
    
    Arama duyarlılığını belirler. Varsayılan değeri **true** (yani arama büyük/küçük harf duyarsız yapılır).
    
    *Örnek:*
    
    `caseInsensitive=false` (Böylece arama, büyük/küçük harf duyarlı çalışır.)
    
- **Dinamik Parametre Filtreleri:**
    
    Query parametrelerinde “param_” önekiyle başlayan her değer, ilgili etiket filtrelerine eklenir.
    
    *Örnek:*
    
    `param_action=login`
    
    Böylece, log kaydında “action” etiket değeri bu filtre ile eşleştirilir.
    

> Önemli: Sadece yukarıda belirtilen parametre isimleri kabul edilir. Desteklenmeyen bir query parametresi eklenirse, hata dönecektir.
> 

---

## Örnek GET İstekleri

1. **Tüm Loglar (Varsayılan, son 7 gün):**
    
    ```
    GET http://localhost:3012/api/v1/logs
    ```
    
2. **Belirli Kaynaklardan Log Çekme:**
    
    ```
    GET http://localhost:3012/api/v1/logs?sources=deneme-service
    GET http://localhost:3012/api/v1/logs?sources=zoom-service|auth-service
    ```
    
3. **Seviye Filtrelemesi (Örneğin: info ve debug):**
    
    ```
    GET http://localhost:3012/api/v1/logs?levels=info
    GET http://localhost:3012/api/v1/logs?levels=warn
    GET http://localhost:3012/api/v1/logs?levels=info|debug
    GET http://localhost:3012/api/v1/logs?levels=info|debug|warning

    ```
    
    *(Kullanıcı “inf” yazsa bile, “Information” kayıtları da getirilecektir.)*
    
4. **Zaman Aralığı Belirterek (Milisaniye):**
    
    ```
    GET http://localhost:3012/api/v1/logs?startTime=1680000000000&endTime=1685000000000
    ```

    
5. **Mesaj İçinde Arama (Contains):**
    
    ```
    GET http://localhost:3012/api/v1/logs?contains=test
    GET http://localhost:3012/api/v1/logs?contains=test|log
    ```
    
6. **Case Duyarlı Arama:**
    
    ```
    GET http://localhost:3012/api/v1/logs?contains=Test&caseInsensitive=false
    GET http://localhost:3012/api/v1/logs?contains=Log|Test&caseInsensitive=false

    ```
    
7. **Dinamik Parametre ile Filtreleme (Örneğin: Action değeri):**
    
    ```
    GET http://localhost:3012/api/v1/logs?param_action=login
    GET http://localhost:3012/api/v1/logs?param_user=user
    ```
    
8. **Karma Kombinasyon Örneği:**
    
    ```
    GET http://localhost:3012/api/v1/logs?sources=zoom-service|auth-service&levels=inf|debug&param_user=user&contains=log&caseInsensitive=true&startTime=1680000000000&endTime=1685000000000
    ```