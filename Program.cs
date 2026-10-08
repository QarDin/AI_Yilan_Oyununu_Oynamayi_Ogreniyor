// Pencere, çizim ve klavye. Oyunun kuralları YilanOyunu.cs'te.
using Microsoft.VisualBasic;
using Raylib_cs;
using YilanAiProject;
class Program
{

    const int Kare = 28;
    const int UstBar = 44;
    const int Genislik = YilanOyunu.Sutunlar * Kare;
    const int Yukseklik = YilanOyunu.Satirlar * Kare + UstBar;
    const double AdimAraligi = 0.1;   // saniyede 10 adım

    // QarDinLab paleti
    static readonly Color Zemin = Renk(0x04060A);
    static readonly Color Yuzey = Renk(0x0E1622);
    static readonly Color Yesil = Renk(0x3EB842);
    static readonly Color Sari = Renk(0xF2C230);
    static readonly Color Gri = Renk(0x7A8894);
    static readonly Color Yazi = Renk(0xE8EDF2);

    static void Main()
    {

        Raylib.InitWindow(Genislik, Yukseklik, "Yılan");
        Raylib.SetTargetFPS(60);   // çizim 60 FPS, oyun adımı ayrı sayaçla

        var oyun = new YilanOyunu();
        var rnd = new Random();
        var ag = new SinirAgi(new Evrim().RandomNumberCreator(rnd));
        var skorlar = new List<int>();   // biten oyunların yem sayıları
        double sayac = 0;

        while (!Raylib.WindowShouldClose())
        {
            // oyun.Adim();
            // IsKeyPressed: "son kareden beri basıldı mı?" Kısa dokunuş kaçmaz.
            if (Raylib.IsKeyPressed(KeyboardKey.Up)) oyun.YonuDegistir(Yon.Yukari);
            if (Raylib.IsKeyPressed(KeyboardKey.Down)) oyun.YonuDegistir(Yon.Asagi);
            if (Raylib.IsKeyPressed(KeyboardKey.Left)) oyun.YonuDegistir(Yon.Sol);
            if (Raylib.IsKeyPressed(KeyboardKey.Right)) oyun.YonuDegistir(Yon.Sag);

            // >>> AI buraya bağlanacak: tuşların yerine
            //     oyun.YonuDegistir(ağın seçtiği yön);

            sayac += Raylib.GetFrameTime();
            while (sayac >= AdimAraligi)
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
                Console.WriteLine(string.Join(", ", oyun.Girecekler()));

                if (oyun.Bitti)
                {
                    skorlar.Add(oyun.Skor);
                    Console.WriteLine($"Oyun {skorlar.Count}: {oyun.Skor} yem, {oyun.AdimSayisi} adım {oyun.OnumDoluMu()},{oyun.SagimDoluMu()} {oyun.SolumDoluMu()}");
                    oyun = new YilanOyunu();   // yeni ev: her şey başlangıç değerinde
                }
            }


            Raylib.BeginDrawing();
            Raylib.ClearBackground(Zemin);
            Ciz(oyun, skorlar);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static void Ciz(YilanOyunu oyun, List<int> skorlar)
    {
        // Üst bilgi (varsayılan font Türkçe harf göstermez, o yüzden düz harf)
        string son5 = skorlar.Count == 0
            ? "-"
            : skorlar.TakeLast(5).Average().ToString("0.0");
        Raylib.DrawText($"Yem {oyun.Skor}", 14, 12, 20, Yazi);
        Raylib.DrawText($"Oyun {skorlar.Count + 1}    Son 5 ortalama {son5}", 120, 14, 18, Gri);

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
