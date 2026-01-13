## TournamentApp - Projekt Zaliczeniowy GraphQL
Projekt jest implementacją aplikacji do obsługi turniejów w systemie pucharowym, zrealizowaną jako API w technologii GraphQL. Aplikacja została stworzona w środowisku Visual Studio 2022.

### Opis projektu
Aplikacja realizuje wymagania projektowe oparte o dostarczony diagram klas. Główne funkcjonalności obejmują:

Implementację API w standardzie GraphQL.

Rejestrację i logowanie użytkowników z wykorzystaniem tokenów JWT.

Zarządzanie turniejami (tworzenie, dołączanie uczestników).

Automatyczne generowanie drabinki turniejowej po rozpoczęciu turnieju.

Pobieranie informacji o meczach zalogowanego użytkownika bez podawania jego identyfikatora w argumencie zapytania.

### Wykorzystane technologie
Platforma: .NET 8 (ASP.NET Core Web API)

GraphQL: Biblioteka HotChocolate

Baza danych: SQLite (Entity Framework Core)

Autoryzacja: JWT Bearer Authentication

### Instrukcja uruchomienia
Otwórz plik rozwiązania TournamentApp.sln w programie Visual Studio 2022.

Zbuduj projekt, aby przywrócić brakujące pakiety NuGet.

Uruchom aplikację (F5).

Baza danych tournament.db zostanie utworzona automatycznie przy pierwszym uruchomieniu.

WAŻNE: Aplikacja nie posiada interfejsu graficznego pod domyślnym adresem. Aby korzystać z API, należy przejść pod adres endpointu GraphQL:

https://localhost:TWOJ_PORT/graphql

Pod tym adresem dostępny jest interfejs Banana Cake Pop służący do testowania zapytań.

## Instrukcja testowania (Scenariusz użycia)
Poniżej znajduje się lista kroków niezbędnych do przetestowania funkcjonalności aplikacji w interfejsie Banana Cake Pop.

#### 1. Rejestracja i uzyskanie tokena
Wykonaj poniższą mutację, aby zarejestrować użytkownika i otrzymać token dostępowy.

GraphQL

mutation Register {
  register(input: {
    firstName: "Jan",
    lastName: "Student",
    email: "jan@student.pl",
    password: "123"
  }) {
    token
  }
}
#### 2. Autoryzacja
Większość operacji w systemie wymaga uwierzytelnienia.

Skopiuj wartość pola token z odpowiedzi (sam ciąg znaków, bez cudzysłowów).

W interfejsie Banana Cake Pop otwórz ustawienia (ikona koła zębatego).

Przejdź do zakładki Authorization.

W polu Type wybierz: Bearer.

W polu Token wklej skopiowany ciąg znaków.

#### 3. Tworzenie turnieju
Jako zalogowany użytkownik utwórz nowy turniej.

GraphQL

mutation Create {
  createTournament(name: "Turniej Zaliczeniowy", startDate: "2024-06-20T10:00:00Z") {
    id
    name
  }
}
#### 4. Dołączenie do turnieju
Dołącz do turnieju o wskazanym ID (np. ID 1).

GraphQL

mutation Join {
  addParticipant(tournamentId: 1) {
    participants { firstName }
  }
}
Uwaga: Aby turniej mógł wystartować, wymaganych jest minimum dwóch uczestników. W celach testowych należy zarejestrować drugiego użytkownika, podmienić token w ustawieniach i również wykonać mutację dołączenia.

#### 5. Rozpoczęcie turnieju
Wygenerowanie drabinki meczowej dla turnieju.

GraphQL

mutation Start {
  startTournament(tournamentId: 1) {
    id
    tournament { status }
    matches {
      player1 { firstName }
      player2 { firstName }
    }
  }
}
#### 6. Pobranie własnych meczów
Weryfikacja głównego wymagania projektowego: pobranie meczów, w których bierze udział zalogowany użytkownik (na podstawie tokena JWT).

GraphQL

query MyMatches {
  myMatches {
    id
    round
    bracket { tournament { name } }
    player1 { firstName }
    player2 { firstName }
    winner { firstName }
  }
}
