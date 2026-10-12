// Pencere, çizim ve klavye. Oyunun kuralları YilanOyunu.cs'te.
using System.Globalization;
using Raylib_cs;
using YilanAiProject;
class Program
{

    const int Kare = 40;
    const int UstBar = 44;
    const int Genislik = YilanOyunu.Sutunlar * Kare;
    const int Yukseklik = YilanOyunu.Satirlar * Kare + UstBar;
    const double AdimAraligi = 0.1;   // saniyede 10 adım
    const double BitisBekleme = 2.0;  // ölen yılan kaç saniye ekranda kalsın

    // QarDinLab paleti
    static readonly Color Zemin = Renk(0x04060A);
    static readonly Color Yuzey = Renk(0x0E1622);
    static readonly Color Yesil = Renk(0x3EB842);
    static readonly Color Sari = Renk(0xF2C230);
    static readonly Color Gri = Renk(0x7A8894);
    static readonly Color Yazi = Renk(0xE8EDF2);

    static YilanOyunu PenceresizOynatma(float[] gen)
    {
        // 1. yeni bir oyun ve bu genle bir ag kurmam isteniyor.
        SinirAgi yeniAg = new SinirAgi(gen);
        YilanOyunu yeniOyun = new YilanOyunu();
        while (yeniOyun.Bitti == false)
        {
            float[] yeniOyunaCevap = yeniOyun.Girecekler();
            float[] YeniOyunCiktilari = yeniAg.ilerle(yeniOyunaCevap);
            int AginEnYuksekIndexi = yeniAg.EnYuksekDegerliIndex(YeniOyunCiktilari);
            if (AginEnYuksekIndexi == 0)
            {
                yeniOyun.YonuDegistir(yeniOyun.Yon);
            }
            else if (AginEnYuksekIndexi == 1)
            {
                yeniOyun.YonuDegistir(YilanOyunu.solYon(yeniOyun.Yon));
            }
            else
            {
                yeniOyun.YonuDegistir(YilanOyunu.sagYon(yeniOyun.Yon));
            }
            yeniOyun.Adim();
        }
        return yeniOyun;

    }


    const string KayitDosyasi = "kayit.csv";

    // dotnet run              -> 100 nesil eğit, kayit.csv'ye yaz
    // dotnet run -- oynat 12  -> 12. neslin en iyisini pencerede oynat
    // dotnet run -- oynat 12 5 -> 12. neslin 5. sıradakini oynat
    static void Main(string[] args)
    {
        if (args.Length >= 2 && args[0] == "oynat")
        {
            int secilenNesil = int.Parse(args[1]);
            int secilenSira = args.Length >= 3 ? int.Parse(args[2]) : 1;
            Oynat(secilenNesil, secilenSira);
            return;
        }
        Egit();
    }

    static void Egit()
    {
        var rnd = new Random();
        float[][] genler = new float[20][];
        YilanOyunu[] DegeriHesaplananYilanlar = new YilanOyunu[genler.Length];
        YilanOyunu yilan = new YilanOyunu();
        for (int i = 0; i < genler.Length; i++)
        {
            genler[i] = new Evrim().RandomNumberCreator(rnd);
        }

        using var kayit = new StreamWriter(KayitDosyasi);
        kayit.WriteLine("nesil;sira;puan;yem;genler...");

        for (int nesil = 0; nesil < 100; nesil++)
        {
            float DegerTutucu = float.MinValue;
            float[] puanlar = new float[genler.Length];
            for (int sira = 0; sira < genler.Length; sira++)
            {
                yilan = PenceresizOynatma(genler[sira]);
                DegeriHesaplananYilanlar[sira] = yilan;
                puanlar[sira] = yilan.Puan();
            }
            for (int a = 0; a < DegeriHesaplananYilanlar.Length; a++)
            {
                if (DegerTutucu < DegeriHesaplananYilanlar[a].Puan())
                {
                    DegerTutucu = DegeriHesaplananYilanlar[a].Puan();
                }
            }
            System.Console.WriteLine($"Nesil {nesil + 1}: en iyi {DegerTutucu}");

            // Bu neslin 20 yılanı, puana göre büyükten küçüğe: 1. sıra = en iyi
            int[] sirali = Enumerable.Range(0, genler.Length)
                .OrderByDescending(i => puanlar[i])
                .ToArray();
            for (int s = 0; s < sirali.Length; s++)
            {
                int i = sirali[s];
                string genYazi = string.Join(";", genler[i].Select(g => g.ToString(CultureInfo.InvariantCulture)));
                kayit.WriteLine($"{nesil + 1};{s + 1};{puanlar[i].ToString(CultureInfo.InvariantCulture)};{DegeriHesaplananYilanlar[i].Skor};{genYazi}");
            }

            genler = new Evrim().YeniNesil(genler, puanlar, rnd);
        }
        System.Console.WriteLine($"Kaydedildi: {KayitDosyasi}");
    }

    // kayit.csv'den istenen nesil + sıradaki yılanın genini okur
    static float[] KayittanGenOku(int nesil, int sira)
    {
        foreach (string satir in File.ReadLines(KayitDosyasi).Skip(1))
        {
            string[] parcalar = satir.Split(';');
            if (int.Parse(parcalar[0]) == nesil && int.Parse(parcalar[1]) == sira)
            {
                return parcalar.Skip(4)
                    .Select(p => float.Parse(p, CultureInfo.InvariantCulture))
                    .ToArray();
            }
        }
        throw new Exception($"Kayıtta nesil {nesil}, sıra {sira} yok");
    }

    static void Oynat(int nesil, int sira)
    {
        var ag = new SinirAgi(KayittanGenOku(nesil, sira));
        var oyun = new YilanOyunu();

        Raylib.InitWindow(Genislik, Yukseklik, "Yılan");
        Raylib.SetTargetFPS(60);   // çizim 60 FPS, oyun adımı ayrı sayaçla

        var skorlar = new List<int>();   // biten oyunların yem sayıları
        double sayac = 0;
        double bekleme = 0;   // yılan ölünce son hâli ekranda kalsın

        while (!Raylib.WindowShouldClose())
        {
            if (bekleme > 0)
            {
                bekleme -= Raylib.GetFrameTime();
                if (bekleme <= 0)
                {
                    skorlar.Add(oyun.Skor);
                    oyun = new YilanOyunu();   // aynı gen, yeni oyun
                    sayac = 0;
                }
            }
            else
            {
                sayac += Raylib.GetFrameTime();
            }
            while (bekleme <= 0 && sayac >= AdimAraligi)
            {
                sayac -= AdimAraligi;
                float[] OyunCevap = oyun.Girecekler();
                float[] OyunCiktilari = ag.ilerle(OyunCevap);
                int AginEnYuksekIndexi = ag.EnYuksekDegerliIndex(OyunCiktilari);
                if (AginEnYuksekIndexi == 0)
                {
                    oyun.YonuDegistir(oyun.Yon);
                }
                else if (AginEnYuksekIndexi == 1)
                {
                    oyun.YonuDegistir(YilanOyunu.solYon(oyun.Yon));
                }
                else
                {
                    oyun.YonuDegistir(YilanOyunu.sagYon(oyun.Yon));
                }
                oyun.Adim();

                if (oyun.Bitti)
                {
                    Console.WriteLine($"Oyun {skorlar.Count + 1}: {oyun.Skor} yem, {oyun.AdimSayisi} adım");
                    bekleme = BitisBekleme;
                }
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Zemin);
            Ciz(oyun, skorlar, $"Nesil {nesil}");
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }


    static void Ciz(YilanOyunu oyun, List<int> skorlar, string baslik)
    {
        // Üst bilgi (varsayılan font Türkçe harf göstermez, o yüzden düz harf)
        string son5 = skorlar.Count == 0
            ? "-"
            : skorlar.TakeLast(5).Average().ToString("0.0");
        Raylib.DrawText($"Yem {oyun.Skor}", 14, 12, 20, Yazi);
        Raylib.DrawText($"Oyun {skorlar.Count + 1}    Son 5 ortalama {son5}", 120, 14, 18, Gri);
        Raylib.DrawText(baslik, Genislik - 14 - Raylib.MeasureText(baslik, 20), 12, 20, Sari);

        // Oyun alanı
        Raylib.DrawRectangle(0, UstBar, Genislik, Yukseklik - UstBar, Yuzey);

        // Yem
        KareCiz(oyun.Yem, Sari, 6);

        // Yılan: gövde soluk yeşil, baş tam yeşil
        for (int i = oyun.Govde.Count - 1; i >= 1; i--)
            KareCiz(oyun.Govde[i], Raylib.Fade(Yesil, 0.55f), 3);
        KareCiz(oyun.Bas, Yesil, 2);
    }

    static void KareCiz(Konum k, Color renk, int bosluk)
    {
        Raylib.DrawRectangle(
            k.Sutun * Kare + bosluk,
            UstBar + k.Satir * Kare + bosluk,
            Kare - 2 * bosluk,
            Kare - 2 * bosluk,
            renk);
    }

    static Color Renk(int hex) =>
        new((byte)(hex >> 16), (byte)(hex >> 8), (byte)hex, (byte)255);
}
