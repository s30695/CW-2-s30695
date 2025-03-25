using System;
using System.Collections.Generic;


public class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

public class DangerousOperationException : Exception
{
    public DangerousOperationException(string message) : base(message) { }
}

public interface IHazardNotifier
{
    void NotifyHazard(string message);
}

// -----------------------------------------------------
// 3) Enum Produkt - typy produktów dla kontenerów chłodniczych
// -----------------------------------------------------
public enum Produkt
{
    Banany,
    Czekolada,
    Ryby,
    Mieso,
    Lody,
    MrozonaPizza,
    Ser,
    Kielbasa,
    Maslo,
    Jajka
}

// -----------------------------------------------------
// 4) Klasa bazowa Kontener
// -----------------------------------------------------
public abstract class Kontener
{
    public double MasaLadunku { get; protected set; } // w kg
    public double WagaWlasna { get; protected set; }  // w kg
    public double Wysokosc { get; protected set; }    // w cm
    public double Glebokosc { get; protected set; }   // w cm
    public double MaksymalnaLadownosc { get; protected set; } // w kg
    public string NumerSeryjny { get; protected set; }

    // Konstruktor bazowy
    public Kontener(double wagaWlasna, double wysokosc, double glebokosc, double maksLadownosc)
    {
        WagaWlasna = wagaWlasna;
        Wysokosc = wysokosc;
        Glebokosc = glebokosc;
        MaksymalnaLadownosc = maksLadownosc;
    }

    // Metoda do załadowania ładunku
    public virtual void Zaladuj(double masa)
    {
        double nowaMasa = MasaLadunku + masa;
        if (nowaMasa > MaksymalnaLadownosc)
        {
            throw new OverfillException(
                $"Próba załadowania {masa} kg przekracza pojemność kontenera (max {MaksymalnaLadownosc} kg).");
        }
        MasaLadunku = nowaMasa;
    }

    // Metoda do rozładowania kontenera (całkowite)
    public virtual void Rozladuj()
    {
        MasaLadunku = 0;
    }

    // Całkowita waga kontenera (waga własna + masa ładunku)
    public double CalkowitaWaga => WagaWlasna + MasaLadunku;

    // Pomocnicza metoda do generowania numerów seryjnych
    // Format: KON-{typ kontenera}-{liczba unikalna}
    protected static string GenerujNumer(string typKontenera, ref int licznik)
    {
        licznik++;
        return $"KON-{typKontenera}-{licznik}";
    }
}

// -----------------------------------------------------
// 5) KontenerNaCiecze
//    - L (Liquid), może być niebezpieczny lub zwykły
// -----------------------------------------------------
public class KontenerNaCiecze : Kontener, IHazardNotifier
{
    private static int _licznik = 0;  // służy do unikalnego numeru seryjnego

    public bool LadunekNiebezpieczny { get; private set; }

    public KontenerNaCiecze(double wagaWlasna, double wysokosc, double glebokosc,
                            double maksLadownosc, bool ladunekNiebezpieczny)
        : base(wagaWlasna, wysokosc, glebokosc, maksLadownosc)
    {
        LadunekNiebezpieczny = ladunekNiebezpieczny;
        NumerSeryjny = GenerujNumer("L", ref _licznik);
    }

    public override void Zaladuj(double masa)
    {
        // Jeśli ładunek niebezpieczny, można załadować maks 50% pojemności
        // Jeśli zwykły, do 90%
        double limit = LadunekNiebezpieczny ? 0.5 : 0.9;
        double maksMozliwe = MaksymalnaLadownosc * limit;

        if (MasaLadunku + masa > maksMozliwe)
        {
            // Zgłaszamy niebezpieczną sytuację
            NotifyHazard($"Próba załadowania {masa} kg narusza limit " +
                         $"({limit * 100}% pojemności = {maksMozliwe} kg).");
            throw new DangerousOperationException(
                $"Niebezpieczna operacja: przekroczono {limit * 100}% pojemności kontenera.");
        }

        base.Zaladuj(masa);
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[ALERT] Kontener {NumerSeryjny}: {message}");
    }
}

// -----------------------------------------------------
// 6) KontenerNaGaz
//    - G (Gas), przechowuje dodatkowo info o ciśnieniu
// -----------------------------------------------------
public class KontenerNaGaz : Kontener, IHazardNotifier
{
    private static int _licznik = 0;
    public double Cisnienie { get; private set; } // w atmosferach

    public KontenerNaGaz(double wagaWlasna, double wysokosc, double glebokosc,
                         double maksLadownosc, double cisnienie)
        : base(wagaWlasna, wysokosc, glebokosc, maksLadownosc)
    {
        Cisnienie = cisnienie;
        NumerSeryjny = GenerujNumer("G", ref _licznik);
    }

    public override void Rozladuj()
    {
        // W kontenerze gazowym zostaje 5% ładunku
        MasaLadunku *= 0.05;
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[ALERT] Kontener {NumerSeryjny}: {message}");
    }
}

// -----------------------------------------------------
// 7) KontenerChlodniczy
//    - C (Cold), przechowuje typ produktu i temperaturę
// -----------------------------------------------------
public class KontenerChlodniczy : Kontener
{
    private static int _licznik = 0;

    public Produkt TypProduktu { get; private set; }
    public double Temperatura { get; private set; } // w °C

    // Minimalne temperatury dla poszczególnych produktów (przykładowe wartości).
    private static readonly Dictionary<Produkt, double> MinimalnaTemp = new Dictionary<Produkt, double>
    {
        { Produkt.Banany, 13.3 },
        { Produkt.Czekolada, 18 },
        { Produkt.Ryby, 2 },
        { Produkt.Mieso, -15 },
        { Produkt.Lody, -18 },
        { Produkt.MrozonaPizza, -30 },
        { Produkt.Ser, 7.2 },
        { Produkt.Kielbasa, 5 },
        { Produkt.Maslo, 20.5 },
        { Produkt.Jajka, 19 }
    };

    public KontenerChlodniczy(double wagaWlasna, double wysokosc, double glebokosc,
                              double maksLadownosc, Produkt produkt, double temperatura)
        : base(wagaWlasna, wysokosc, glebokosc, maksLadownosc)
    {
        TypProduktu = produkt;

        // Sprawdź czy temperatura nie jest niższa niż wymagana
        double wymaganeMin = MinimalnaTemp[produkt];
        if (temperatura < wymaganeMin)
        {
            throw new ArgumentException(
                $"Temperatura {temperatura}°C jest niższa niż wymagana {wymaganeMin}°C dla produktu {produkt}.");
        }

        Temperatura = temperatura;
        NumerSeryjny = GenerujNumer("C", ref _licznik);
    }
}

// -----------------------------------------------------
// 8) Klasa Kontenerowiec
// -----------------------------------------------------
public class Kontenerowiec
{
    public string NazwaStatku { get; private set; }
    public double MaksymalnaPredkoscWezly { get; private set; }
    public int MaksLiczbaKontenerow { get; private set; }
    public double MaksWagaWszystkichKontenerowTon { get; private set; }

    private List<Kontener> _kontenery = new List<Kontener>();

    public Kontenerowiec(string nazwa, double predkosc, int maksKont, double maksWagaTon)
    {
        NazwaStatku = nazwa;
        MaksymalnaPredkoscWezly = predkosc;
        MaksLiczbaKontenerow = maksKont;
        MaksWagaWszystkichKontenerowTon = maksWagaTon;
    }

    // Metoda do załadowania kontenera
    public void ZaladujKontener(Kontener k)
    {
        if (_kontenery.Count >= MaksLiczbaKontenerow)
        {
            throw new InvalidOperationException(
                $"Przekroczono maksymalną liczbę kontenerów ({MaksLiczbaKontenerow}).");
        }

        double aktualnaWagaTon = ObliczAktualnaWageKontenerow() / 1000.0; // kg na tony
        double wagaPoDoladowaniu = (aktualnaWagaTon + k.CalkowitaWaga / 1000.0);

        if (wagaPoDoladowaniu > MaksWagaWszystkichKontenerowTon)
        {
            throw new InvalidOperationException(
                $"Załadowanie kontenera przekroczy maksymalną wagę statku " +
                $"({MaksWagaWszystkichKontenerowTon} ton). Aktualnie: {wagaPoDoladowaniu:F2} ton.");
        }

        _kontenery.Add(k);
    }

    // Załadowanie listy kontenerów
    public void ZaladujKontenery(IEnumerable<Kontener> kontenery)
    {
        foreach (var k in kontenery)
        {
            ZaladujKontener(k);
        }
    }

    // Usunięcie kontenera
    public void UsunKontener(Kontener k)
    {
        _kontenery.Remove(k);
    }

    // Rozładowanie (opróżnienie) ładunku w danym kontenerze
    public void RozladujKontener(Kontener k)
    {
        k.Rozladuj();
    }

    // Zastąpienie kontenera innym (pod warunkiem, że stary istnieje na statku)
    public void ZastapKontener(Kontener stary, Kontener nowy)
    {
        if (_kontenery.Contains(stary))
        {
            UsunKontener(stary);
            ZaladujKontener(nowy);
        }
        else
        {
            throw new ArgumentException("Nie można zastąpić kontenera – nie ma go na statku.");
        }
    }

    // Przeniesienie kontenera między dwoma statkami
    public static void PrzeniesKontener(Kontener k, Kontenerowiec zrodlo, Kontenerowiec cel)
    {
        if (!zrodlo._kontenery.Contains(k))
        {
            throw new ArgumentException("Kontener nie znajduje się na statku źródłowym.");
        }

        // Najpierw usuwamy ze statku źródłowego
        zrodlo.UsunKontener(k);

        // Następnie ładujemy na statek docelowy
        cel.ZaladujKontener(k);
    }

    // Wypisanie informacji o statku i jego ładunku
    public void WypiszInformacje()
    {
        Console.WriteLine($"--- Statek: {NazwaStatku} ---");
        Console.WriteLine($"Maks. prędkość (węzły): {MaksymalnaPredkoscWezly}");
        Console.WriteLine($"Ładowność kontenerów: {MaksLiczbaKontenerow} szt., {MaksWagaWszystkichKontenerowTon} ton.");
        Console.WriteLine($"Obecna liczba kontenerów: {_kontenery.Count}, ich łączna waga: {ObliczAktualnaWageKontenerow()} kg");
        Console.WriteLine("Lista kontenerów:");

        foreach (var kontener in _kontenery)
        {
            Console.WriteLine($"  - {kontener.NumerSeryjny}, masa ładunku: {kontener.MasaLadunku} kg, " +
                              $"całk. waga: {kontener.CalkowitaWaga} kg");
        }
        Console.WriteLine();
    }

    // Oblicz całkowitą wagę kontenerów w kg
    public double ObliczAktualnaWageKontenerow()
    {
        double suma = 0;
        foreach (var k in _kontenery)
        {
            suma += k.CalkowitaWaga;
        }
        return suma;
    }
}

// -----------------------------------------------------
// 9) Klasa Program z metodą Main - demonstracja
// -----------------------------------------------------
public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            // Tworzymy dwa statki
            Kontenerowiec statekA = new Kontenerowiec("Posejdon", 25.0, 5, 100); // 100 ton
            Kontenerowiec statekB = new Kontenerowiec("Neptun", 20.0, 3, 50);   // 50 ton

            // Tworzymy kontenery różnych typów
            KontenerNaCiecze kontenerMleko = new KontenerNaCiecze(
                wagaWlasna: 1000, wysokosc: 200, glebokosc: 300, maksLadownosc: 5000,
                ladunekNiebezpieczny: false);

            KontenerNaCiecze kontenerPaliwo = new KontenerNaCiecze(
                wagaWlasna: 1200, wysokosc: 220, glebokosc: 300, maksLadownosc: 8000,
                ladunekNiebezpieczny: true);

            KontenerNaGaz kontenerHel = new KontenerNaGaz(
                wagaWlasna: 800, wysokosc: 200, glebokosc: 200, maksLadownosc: 3000,
                cisnienie: 10.5);

            KontenerChlodniczy kontenerBanany = new KontenerChlodniczy(
                wagaWlasna: 1500, wysokosc: 250, glebokosc: 400, maksLadownosc: 6000,
                produkt: Produkt.Banany, temperatura: 15);

            KontenerChlodniczy kontenerLody = new KontenerChlodniczy(
                wagaWlasna: 2000, wysokosc: 250, glebokosc: 400, maksLadownosc: 7000,
                produkt: Produkt.Lody, temperatura: -18);

            // Zaladuj ładunek do kontenerów (z demonstracją ewentualnych wyjątków)
            kontenerMleko.Zaladuj(4000);        // ok (zwykły ładunek, limit 90% z 5000 = 4500)
            //kontenerMleko.Zaladuj(600);      // to by przekroczyło limit i rzuciło wyjątek

            kontenerPaliwo.Zaladuj(4000);       // ok (niebezpieczny - 50% z 8000 = 4000)
            //kontenerPaliwo.Zaladuj(1);       // przekroczenie -> wyrzuciłoby DangerousOperationException

            kontenerHel.Zaladuj(2500);          // ok (zwykłe sprawdzenie Overfill)

            // Zaladuj kontenery na statek A
            statekA.ZaladujKontener(kontenerMleko);
            statekA.ZaladujKontener(kontenerPaliwo);
            statekA.ZaladujKontener(kontenerHel);

            // Możemy też ładować listą
            var listaChlodniczych = new List<Kontener> { kontenerBanany, kontenerLody };
            statekA.ZaladujKontenery(listaChlodniczych);

            // Wypisz info o statku A
            statekA.WypiszInformacje();

            // Przenieś jeden z kontenerów na statek B
            Kontenerowiec.PrzeniesKontener(kontenerHel, statekA, statekB);

            Console.WriteLine("Po przeniesieniu kontenera z Helem na statek B:");
            statekA.WypiszInformacje();
            statekB.WypiszInformacje();

            // Zamień (zastąp) kontener bananów kontenerem lodów na statku A
            statekA.ZastapKontener(kontenerBanany, kontenerLody);

            Console.WriteLine("Po zastąpieniu kontenera bananów kontenerem lodów na statku A:");
            statekA.WypiszInformacje();

            // Rozładuj kontener z paliwem
            statekA.RozladujKontener(kontenerPaliwo);

            // Rozładuj kontener gazowy (zostaje 5% masy)
            statekB.RozladujKontener(kontenerHel);
            Console.WriteLine($"Po rozładunku w kontenerze Hel (zostaje 5%): {kontenerHel.MasaLadunku} kg");

        }
        catch (OverfillException ex)
        {
            Console.WriteLine($"Błąd pojemności: {ex.Message}");
        }
        catch (DangerousOperationException ex)
        {
            Console.WriteLine($"Niebezpieczna operacja: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Inny błąd: {ex.Message}");
        }
    }
}

