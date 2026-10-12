// Oyunun kuralları. Burada Raylib yok: çizim, tuş, pencere bilmez.
// Bu sayede aynı oyunu hem sen (klavyeyle) hem AI (koddan) oynatabilir,
// ve eğitimde pencere açmadan binlerce oyun hızlıca oynatılabilir.



public enum Yon { Yukari, Asagi, Sol, Sag }

// (satır, sütun) düzeni: yukarı = satır -1, sağa = sütun +1
public readonly record struct Konum(int Satir, int Sutun)
{
    public static Konum operator +(Konum a, Konum b) => new(a.Satir + b.Satir, a.Sutun + b.Sutun);
}

public class YilanOyunu
{
    public const int aclikSiniri = 75;
    public const int Satirlar = 20;
    public const int Sutunlar = 20;

    static readonly Random rastgele = new();

    readonly List<Konum> govde = new();   // govde[0] = baş

    public IReadOnlyList<Konum> Govde => govde;
    public Konum Bas => govde[0];
    public Yon Yon { get; private set; } = Yon.Sag;
    public Konum Yem { get; private set; }
    public int Skor { get; private set; }        // yenen yem sayısı
    public int AdimSayisi { get; private set; }  // kaç tick geçti
    int AdimSayaci = 0;
    public bool Bitti { get; private set; }

    public YilanOyunu()
    {
        // Ortada, sağa bakan 3 karelik yılan
        var orta = new Konum(Satirlar / 2, Sutunlar / 2);
        govde.Add(orta);
        govde.Add(orta + new Konum(0, -1));
        govde.Add(orta + new Konum(0, -2));
        YemKoy();
    }

    // Yön kutudan böyle çıkar: "satıra ne eklenir, sütuna ne eklenir"
    public static Konum AdimVektoru(Yon yon) => yon switch
    {
        Yon.Yukari => new Konum(-1, 0),
        Yon.Asagi => new Konum(1, 0),
        Yon.Sol => new Konum(0, -1),
        _ => new Konum(0, 1),   // Sağ
    };
    public static Yon sagYon(Yon yon) => yon switch
    {
        Yon.Yukari => Yon.Sag,
        Yon.Sag => Yon.Asagi,
        Yon.Asagi => Yon.Sol,
        Yon.Sol => Yon.Yukari

    };
    public static Yon solYon(Yon yon) => yon switch
    {
        Yon.Yukari => Yon.Sol,
        Yon.Sag => Yon.Yukari,
        Yon.Asagi => Yon.Sag,
        Yon.Sol => Yon.Asagi

    };
    public bool OnumDoluMu()
    {
        // 1. olarak bas bilgisini almak.
        Konum basDanSonraki = Bas + AdimVektoru(Yon);
        // 2. adim atilacak kareyi bulmak.
        bool kontrol = CarparMi(basDanSonraki.Satir, basDanSonraki.Sutun);
        // 3. kontrol u donmek
        return kontrol;
    }
    public bool SagimDoluMu()
    {
        Konum BasinSagi = Bas + AdimVektoru(sagYon(Yon));
        bool kontrol = CarparMi(BasinSagi.Satir, BasinSagi.Sutun);
        return kontrol;
    }
    public bool SolumDoluMu()
    {
        Konum BasinSolu = Bas + AdimVektoru(solYon(Yon));
        bool kontrol = CarparMi(BasinSolu.Satir, BasinSolu.Sutun);
        return kontrol;
    }
    public bool YemBuYondeMi(Yon yon) => yon switch
    {

        Yon.Asagi => Bas.Satir < Yem.Satir,
        Yon.Yukari => Bas.Satir > Yem.Satir,
        Yon.Sag => Yem.Sutun > Bas.Sutun,
        Yon.Sol => Yem.Sutun < Bas.Sutun
    };
    // Klavye de AI de yönü buradan verir. Geri dönüş (kendi boynuna) yok sayılır.
    public void YonuDegistir(Yon yeni)
    {
        var a = AdimVektoru(Yon);
        var b = AdimVektoru(yeni);
        bool tersYon = a.Satir == -b.Satir && a.Sutun == -b.Sutun;
        if (!tersYon)
            Yon = yeni;
    }
    public bool CarparMi(int satir, int stun)
    {
        if (satir < 0)
        {
            return true;
        }
        else if (satir >= Satirlar)
        {
            return true;
        }
        else if (stun < 0)
        {
            return true;
        }
        else if (stun >= Sutunlar)
        {
            return true;
        }
        foreach (var i in Govde)
        {
            if (i.Satir == satir && i.Sutun == stun)
            {
                return true;
            }

        }
        return false;
    }
    // Oyunu bir tick ilerletir.
    public void Adim()
    {

        if (Bitti) return;

        AdimSayisi++;
        AdimSayaci++;
        if (AdimSayaci == aclikSiniri)
        {
            // burada yilanin isi son bulacak.
            Bitti = true;
            return;
        }
        var yeniBas = Bas + AdimVektoru(Yon);
        bool yiyor = yeniBas == Yem;

        if (Carpar(yeniBas, yiyor))
        {
            Bitti = true;

            return;
        }

        govde.Insert(0, yeniBas);

        if (yiyor)
        {
            Skor++;
            AdimSayaci = 0;
            YemKoy();
        }
        else
        {
            govde.RemoveAt(govde.Count - 1);   // yemediyse kuyruk bir kare kısalır
        }
    }
    public float[] GirecekDegerler = new float[7];
    public float[] Girecekler()
    {
        if (OnumDoluMu())
        {
            GirecekDegerler[0] = 1;
        }
        else
        {
            GirecekDegerler[0] = 0;
        }
        if (SolumDoluMu())
        {
            GirecekDegerler[1] = 1;
        }
        else
        {
            GirecekDegerler[1] = 0;
        }
        if (SagimDoluMu())
        {
            GirecekDegerler[2] = 1;
        }
        else
        {
            GirecekDegerler[2] = 0;
        }
        if (YemBuYondeMi(Yon))
        {
            GirecekDegerler[3] = 1;
        }
        else
        {
            GirecekDegerler[3] = 0;
        }
        if (YemBuYondeMi(solYon(Yon)))
        {
            GirecekDegerler[4] = 1;
        }
        else
        {
            GirecekDegerler[4] = 0;
        }
        if (YemBuYondeMi(sagYon(Yon)))
        {
            GirecekDegerler[5] = 1;
        }
        else
        {
            GirecekDegerler[5] = 0;
        }
        if (YemBuYondeMi(sagYon(sagYon(Yon))))
        {
            GirecekDegerler[6] = 1;
        }
        else
        {
            GirecekDegerler[6] = 0;
        }
        return GirecekDegerler;
    }
    bool Carpar(Konum k, bool yiyor)
    {
        if (k.Satir < 0 || k.Satir >= Satirlar || k.Sutun < 0 || k.Sutun >= Sutunlar)
            return true;

        // Yemiyorsa kuyruk bu adımda kalkacak, oraya girmek çarpma sayılmaz
        int bakilacak = yiyor ? govde.Count : govde.Count - 1;
        for (int i = 0; i < bakilacak; i++)
            if (govde[i] == k)
                return true;

        return false;
    }
    public float Puan()
    {
        return (float)(Skor * 40 + AdimSayisi * 0.1);
    }
    void YemKoy()
    {
        var bosKareler = new List<Konum>();
        for (int s = 0; s < Satirlar; s++)
            for (int c = 0; c < Sutunlar; c++)
            {
                var k = new Konum(s, c);
                if (!govde.Contains(k))
                    bosKareler.Add(k);
            }

        if (bosKareler.Count == 0)   // tüm harita doldu: kazandı
        {
            Bitti = true;
            return;
        }

        Yem = bosKareler[rastgele.Next(bosKareler.Count)];
    }
}
