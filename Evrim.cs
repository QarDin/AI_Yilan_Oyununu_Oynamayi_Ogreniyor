
using YilanAiProject;

public class Evrim
{
    public float[] Caprazlama(float[] anne, float[] baba, Random rnd)
    {

        float[] cocuk = new float[anne.Length];
        for (int i = 0; i < anne.Length; i++)
        {
            double number = rnd.NextDouble();
            if (number <= 0.5)
                cocuk[i] = anne[i];
            else
                cocuk[i] = baba[i];

        }
        return cocuk;
    }
    public float[] RandomNumberCreator(Random rnd)
    {
        float[] RandomNumber = new float[SinirAgi.GenLayer];
        for (int i = 0; i < SinirAgi.GenLayer; i++)
        {
            RandomNumber[i] = ((float)rnd.NextDouble() * 2) - 1;
        }
        return RandomNumber;
    }
    public float[] Mutation(float[] InputArray, Random rnd)
    {
        float[] MutationArray = new float[InputArray.Length];

        float eklenecekDeger = 0.025f;
        float degertutucu = 0;
        float x = 0;
        for (int i = 0; i < MutationArray.Length; i++)
        {
            x = InputArray[i];
            degertutucu = (float)rnd.NextDouble();
            if (degertutucu < 0.10)
            {
                float secondZar = (((float)rnd.NextDouble() * 2) - 1);
                x += secondZar * eklenecekDeger;
            }
            MutationArray[i] = x;
        }
        return MutationArray;

    }
    public float[][] YeniNesil(float[][] genler, float[] puanlar, Random rnd)
    {
        float[][] yeniGenler = new float[genler.Length][];
        float[] kopya = (float[])puanlar.Clone();
        int[] elitDizi = new int[3];
        for (int z = 0; z < 3; z++)
        {
            int indexHolder = 0;
            float tutucu = float.MinValue;
            for (int j = 0; j < puanlar.Length; j++)
            {
                if (tutucu < kopya[j])
                {
                    tutucu = kopya[j];
                    indexHolder = j;
                }
            }
            elitDizi[z] = indexHolder;
            kopya[indexHolder] = float.MinValue;

        }
        for (int k = 0; k < elitDizi.Length; k++)
        {
            yeniGenler[k] = genler[elitDizi[k]];
        }
        for (int i = elitDizi.Length; i < genler.Length; i++)
        {
            int anne = EbeveynSec(puanlar, rnd);
            int baba = EbeveynSec(puanlar, rnd);

            float[] cocuk = Caprazlama(genler[anne], genler[baba], rnd);
            yeniGenler[i] = Mutation(cocuk, rnd);
        }

        return yeniGenler;
    }

    private int EbeveynSec(float[] puanlar, Random rnd)
    {

        int a = rnd.Next(puanlar.Length);
        int b = rnd.Next(puanlar.Length);


        if (puanlar[a] > puanlar[b])
        {
            return a;
        }
        return b;
    }
}
