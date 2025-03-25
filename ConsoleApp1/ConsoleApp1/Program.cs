using System;
using System.Collections.Generic;

// --------------------------------------------
// 1) Wyjątki
// --------------------------------------------
public class OverfillException : Exception
{
    public OverfillException(string message) : base(message) { }
}

public class DangerousOperationException : Exception
{
    public DangerousOperationException(string message) : base(message) { }
}

// --------------------------------------------
// 2) Interfejs do notyfikacji o sytuacjach niebezpiecznych
// --------------------------------------------
public interface IHazardNotifier
{
    void NotifyHazard(string message);
}

// --------------------------------------------
// 3) Enum - rodzaje produktów dla kontenerów chłodniczych
// --------------------------------------------
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

// --------------------------------------------
// 4) Klasa bazowa Kontener
// --------------------------------------------
public abstract class Kontener
{
    public double MasaLadunku { get; protected set; }
    public double WagaWlasna { get; protected set; }
    public double Wysokosc { get; protected set; }
    public double Glebokosc { get; protected set; }
    public double MaksymalnaLadownosc { get; protected set; }
    public string NumerSeryjny { get; protected set; }

    public Kontener(double wagaWlasna, double wysokosc, double glebokosc, double maksLadownosc)
    {
        WagaWlasna = wagaWlasna;
        Wysokosc = wysokosc;
        Glebokosc = glebokosc;
        MaksymalnaLadownosc = maksLadownosc;
    }

    // Załadowanie ładunku
    public virtual void Zaladuj(double masa)
    {
        double nowaMasa = MasaLadunku + masa;
        if (nowaMasa > MaksymalnaLadownosc)
        {
            throw new OverfillException($"Ładunek {masa} kg przekracza pojemność kontenera (max {MaksymalnaLadownosc} kg).");
        }
        MasaLadunku = nowaMasa;
    }

    // Rozładowanie kontenera (domyślnie do zera)
    public virtual void Rozladuj()
    {
        MasaLadunku = 0;
    }

    // Waga całkowita (kontener + ładunek)
    public double CalkowitaWaga => WagaWlasna + MasaLadunku;

    // Generowanie unikalnego numeru seryjnego
    protected static string GenerujNumer(string typKontenera, ref int licznik)
    {
        licznik++;
        return $"KON-{typKontenera}-{licznik}";
    }

    // Wypisanie informacji o kontenerze (wymóg: "Wypisanie informacji o danym kontenerze")
    public virtual void WypiszInformacjeOKontenerze()
    {
        Console.WriteLine($"Kontener: {NumerSeryjny}");
        Console.WriteLine($"  Masa ładunku: {MasaLadunku} kg");
        Console.WriteLine($"  Waga własna: {WagaWlasna} kg");
        Console.WriteLine($"  Waga całkowita: {CalkowitaWaga} kg (max ładowność {MaksymalnaLadownosc} kg)");
        Console.WriteLine();
    }
}

// --------------------------------------------
// 5) KontenerNaCiecze
// --------------------------------------------
public class KontenerNaCiecze : Kontener, IHazardNotifier
{
    private static int _licznik = 0;
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
        // Limit 50% jeśli niebezpieczny, 90% jeśli zwykły
        double limit = LadunekNiebezpieczny ? 0.5 : 0.9;
        double maksMozliwe = MaksymalnaLadownosc * limit;

        if (MasaLadunku + masa > maksMozliwe)
        {
            NotifyHazard($"Przekroczono {limit * 100}% pojemności (max {maksMozliwe} kg).");
            throw new DangerousOperationException($"Niebezpieczna operacja: >{limit * 100}% pojemności.");
        }
        base.Zaladuj(masa);
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[UWAGA] {NumerSeryjny}: {message}");
    }
}

// --------------------------------------------
// 6) KontenerNaGaz
// --------------------------------------------
public class KontenerNaGaz : Kontener, IHazardNotifier
{
    private static int _licznik = 0;
    public double Cisnienie { get; private set; }

    public KontenerNaGaz(double wagaWlasna, double wysokosc, double glebokosc,
                         double maksLadownosc, double cisnienie)
        : base(wagaWlasna, wysokosc, glebokosc, maksLadownosc)
    {
        Cisnienie = cisnienie;
        NumerSeryjny = GenerujNumer("G", ref _licznik);
    }

    public override void Rozladuj()
    {
        // Zostaje 5% ładunku
        MasaLadunku *= 0.05;
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"[UWAGA] {NumerSeryjny}: {message}");
    }
}

// --------------------------------------------
// 7) KontenerChlodniczy
// --------------------------------------------
public class KontenerChlodniczy : Kontener
{
    private static int _licznik = 0;

    public Produkt TypProduktu { get; private set; }
    public double Temperatura { get; private set; }

    // Minimalne temperatury (stała tabela)
    private static readonly Dictionary<Produkt, double> MinimalnaTemp = new Dictionary<Produkt, double>
    {
        { Produkt.Banany,       13.3 },
        { Produkt.Czekolada,    18   },
        { Produkt.Ryby,         2    },
        { Produkt.Mieso,        -15  },
        { Produkt.Lody,         -18  },
        { Produkt.MrozonaPizza, -30  },
        { Produkt.Ser,          7.2  },
        { Produkt.Kielbasa,     5    },
        { Produkt.Maslo,        20.5 },
        { Produkt.Jajka,        19   }
    };

    public KontenerChlodniczy(double wagaWlasna, double wysokosc, double glebokosc,
                              double maksLadownosc, Produkt produkt, double temperatura)
        : base(wagaWlasna, wysokosc, glebokosc, maksLadownosc)
    {
        TypProduktu = produkt;
        double minTemp = MinimalnaTemp[produkt];
        if (temperatura < minTemp)
        {
            throw new ArgumentException($"Zbyt niska temp. {temperatura}°C dla produktu {produkt} (min {minTemp}°C).");
        }
        Temperatura = temperatura;
        NumerSeryjny = GenerujNumer("C", ref _licznik);
    }

    // Opcjonalnie krótkie info
    public override void WypiszInformacjeOKontenerze()
    {
        base.WypiszInformacjeOKontenerze();
        Console.WriteLine($"  Typ produktu: {TypProduktu}, temperatura: {Temperatura}°C\n");
    }
}

// --------------------------------------------
// 8) Klasa Kontenerowiec (statek)
// --------------------------------------------
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

    // Załadowanie pojedynczego kontenera
    public void ZaladujKontener(Kontener k)
    {
        if (_kontenery.Count >= MaksLiczbaKontenerow)
            throw new InvalidOperationException($"Przekroczono liczbę kontenerów (max {MaksLiczbaKontenerow}).");

        double wagaAktualnaTon = ObliczAktualnaWageKontenerow() / 1000.0;
        double wagaPoDoladowaniu = wagaAktualnaTon + (k.CalkowitaWaga / 1000.0);

        if (wagaPoDoladowaniu > MaksWagaWszystkichKontenerowTon)
        {
            throw new InvalidOperationException($"Przekroczono dopuszczalną masę (max {MaksWagaWszystkichKontenerowTon} t).");
        }
        _kontenery.Add(k);
    }

    // Załadowanie listy kontenerów
    public void ZaladujKontenery(IEnumerable<Kontener> kontenery)
    {
        foreach (var k in kontenery) ZaladujKontener(k);
    }

    // Usunięcie kontenera
    public void UsunKontener(Kontener k)
    {
        _kontenery.Remove(k);
    }

    // Rozładowanie kontenera
    public void RozladujKontener(Kontener k)
    {
        k.Rozladuj();
    }

    // Zastąpienie kontenera innym
    public void ZastapKontener(Kontener stary, Kontener nowy)
    {
        if (!_kontenery.Contains(stary))
            throw new ArgumentException("Brak takiego kontenera na statku.");

        UsunKontener(stary);
        ZaladujKontener(nowy);
    }

    // Statyczna metoda do przenoszenia kontenera między statkami
    public static void PrzeniesKontener(Kontener k, Kontenerowiec zrodlo, Kontenerowiec cel)
    {
        if (!zrodlo._kontenery.Contains(k))
            throw new ArgumentException("Kontener nie znajduje się na statku źródłowym.");

        zrodlo.UsunKontener(k);
        cel.ZaladujKontener(k);
    }

    // Wypisanie informacji o statku i jego ładunku
    public void WypiszInformacjeOStatku()
    {
        Console.WriteLine($"--- {NazwaStatku} ---");
        Console.WriteLine($"  Prędkość max: {MaksymalnaPredkoscWezly} węzłów");
        Console.WriteLine($"  Kontenery: {_kontenery.Count}/{MaksLiczbaKontenerow}");
        Console.WriteLine($"  Waga ładunku: {ObliczAktualnaWageKontenerow()} kg (max {MaksWagaWszystkichKontenerowTon} t)");
        Console.WriteLine("  Lista kontenerów:");
        foreach (var kontener in _kontenery)
        {
            Console.WriteLine($"    - {kontener.NumerSeryjny} | masa ładunku: {kontener.MasaLadunku} kg | waga całk.: {kontener.CalkowitaWaga} kg");
        }
        Console.WriteLine();
    }

    // Łączna waga w kg
    public double ObliczAktualnaWageKontenerow()
    {
        double suma = 0;
        foreach (var k in _kontenery) suma += k.CalkowitaWaga;
        return suma;
    }
}

// --------------------------------------------
// 9) Program (metoda Main) - demonstracja
// --------------------------------------------
public class Program
{
    public static void Main(string[] args)
    {
        try
        {

            KontenerNaCiecze kontenerSok = new KontenerNaCiecze(900, 200, 300, 4000, false);
            KontenerNaCiecze kontenerChemia = new KontenerNaCiecze(1200, 220, 310, 7000, true);
            KontenerNaGaz kontenerHel = new KontenerNaGaz(800, 180, 250, 3000, 10.0);
            KontenerChlodniczy kontenerBanany = new KontenerChlodniczy(1500, 250, 400, 5000, Produkt.Banany, 14);
            KontenerChlodniczy kontenerLody = new KontenerChlodniczy(1800, 260, 400, 6000, Produkt.Lody, -18);

            kontenerSok.Zaladuj(3000);        // 90% z 4000 = 3600 -> OK
            kontenerChemia.Zaladuj(3500);     // 50% z 7000 = 3500 -> OK
            kontenerHel.Zaladuj(2000);        // max 3000 -> OK
            kontenerBanany.Zaladuj(2000);     // wystarczy że nie przekracza 5000
            kontenerLody.Zaladuj(4000);       // max 6000

            Kontenerowiec statekA = new Kontenerowiec("statekA", 25, 4, 80);
            Kontenerowiec statekB = new Kontenerowiec("statekB", 20, 5, 120);

            statekA.ZaladujKontener(kontenerSok);
            statekA.ZaladujKontener(kontenerChemia);
            // Uwaga: jeśli spróbujemy załadować kolejny, może przekroczyć liczbę 3 kontenerów lub dopuszczalną wagę

            var listaKontenerow = new List<Kontener> { kontenerHel, kontenerBanany };
            statekA.ZaladujKontenery(listaKontenerow);


            Console.WriteLine(">>> STAN STATKU A PO ZAŁADOWANIU (Posejdon) <<<");
            statekA.WypiszInformacjeOStatku();


            Console.WriteLine("Usuwamy kontener z chemią ze statku A...");
            statekA.UsunKontener(kontenerChemia);


            Console.WriteLine("Rozładowujemy (opróżniamy) kontener Sok...");
            statekA.RozladujKontener(kontenerSok);


            Console.WriteLine("Załadujmy kontener Lody na statek B (Neptun)...");
            statekB.ZaladujKontener(kontenerLody);


            Console.WriteLine(">>> STAN STATKU B (Neptun) <<<");
            statekB.WypiszInformacjeOStatku();


            Console.WriteLine("Zastępujemy kontener Banany kontenerem Chemia na statku A...");
            // Najpierw załadujemy kontener Chemia z powrotem, bo usunęliśmy go
            // (można też pominąć i użyć innego kontenera)
            statekA.ZaladujKontener(kontenerChemia);
            statekA.ZastapKontener(kontenerBanany, kontenerChemia);


            Console.WriteLine("Przenosimy kontener Hel ze statku A na B...");
            Kontenerowiec.PrzeniesKontener(kontenerHel, statekA, statekB);


            Console.WriteLine(">>> Informacje o kontenerze Hel (po przeniesieniu) <<<");
            kontenerHel.WypiszInformacjeOKontenerze();


            Console.WriteLine(">>> STAN OSTATECZNY STATKU A  <<<");
            statekA.WypiszInformacjeOStatku();
            Console.WriteLine(">>> STAN OSTATECZNY STATKU B  <<<");
            statekB.WypiszInformacjeOStatku();
        }
        catch (OverfillException ex)
        {
            Console.WriteLine($"[Błąd pojemności] {ex.Message}");
        }
        catch (DangerousOperationException ex)
        {
            Console.WriteLine($"[Operacja niebezpieczna] {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Inny błąd] {ex.Message}");
        }
    }
}
