EventHub API
EventHub je platforma za organizaciju i pretragu lokalnih događaja. Omogućava organizatorima da kreiraju i upravljaju događajima, dok posetioci mogu pretraživati događaje, kupovati karte i pratiti svoje omiljene organizatore. ER dijagram se nalazi u folderu /docs.

Arhitektura
Projekat je organizovan po Clean Architecture principima uz CQRS patern (MediatR), što obezbeđuje lako održavanje i čisto razdvajanje domenske logike od ostatka sistema.

Dijagram baze podataka
Ključne funkcionalnosti
Korisnički nalog: Registracija i prijava sa JWT autentifikacijom.

Upravljanje događajima: Kreiranje, objavljivanje i upravljanje životnim ciklusom događaja.

Prodaja karata: Rezervacija mesta, podrška za različite tipove karata i QR kodovi za ulaz.

Statistika: Dashboard za organizatore sa uvidom u prodaju i izvozom podataka.

Pretraga: Napredna pretraga događaja sa filterom "blizu mene".

Bezbednost: Idempotency middleware za sprečavanje duplih kupovina.

Uputstvo za pokretanje
Klonirajte repozitorijum.

U korenu projekta pokrenite aplikaciju:
docker-compose up -d  

Primenite migracije baze podataka:
dotnet ef database update

Pristup servisima
Nakon pokretanja, servisi su dostupni na sledećim adresama:

API Swagger: http://localhost:8080/swagger/index.html

Hangfire Dashboard: http://localhost:8080/hangfire

Seq (Logovi): http://localhost:5341

Frontend: http://localhost:5173