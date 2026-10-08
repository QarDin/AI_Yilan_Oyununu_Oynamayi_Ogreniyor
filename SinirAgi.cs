using System;
namespace YilanAiProject
{


    public class SinirAgi
    {
        public const int FirstLayer = 7;
        public const int SecondLayer = 6;
        public const int LastLayer = 3;
        public const int GenLayer = FirstLayer * SecondLayer + SecondLayer * LastLayer + SecondLayer + LastLayer;
        private readonly float[] gen;
        public SinirAgi(float[] Genler)
        {
            if (Genler.Length != GenLayer)
            {
                throw new ArgumentException("Gen uzunlugu " + GenLayer + " olmali");
            }
            this.gen = Genler;
        }
        public float[] ilerle(float[] girdi)
        {
            float[] GizliNoronOutput = new float[SecondLayer];
            int p = 0;
            float toplam = 0;
            for (int g = 0; g < SecondLayer; g++)
            {

                for (int i = 0; i < girdi.Length; i++)
                {

                    toplam += girdi[i] * gen[p];
                    p++;
                }
                GizliNoronOutput[g] = (float)Math.Tanh(toplam + gen[p]);
                p++;
                toplam = 0;
            }
            float[] LastLayerOutput = new float[LastLayer];
            //int j = 0;//  gerek kalmadi.
            float toplam2 = 0;
            for (int k = 0; k < LastLayer; k++)
            {
                for (int a = 0; a < SecondLayer; a++)
                {
                    toplam2 += GizliNoronOutput[a] * gen[p]; // o zaman burada p olmak zorunda cunku belirli bir sayiya kadar cikti bu 0 lanmadi devamini buradan goturecegim o zaman benim yaptigim int j ye gerek kalmiyor.
                    p++;
                }
                LastLayerOutput[k] = (float)Math.Tanh(toplam2 + gen[p]); // bias her zaman genden gelecek.
                p++;
                toplam2 = 0;
            }

            return LastLayerOutput;
        }
        public int EnYuksekDegerliIndex(float[] LastLayerOutPut)
        {
            int indexHolder = 0;
            for (int i = 1; i < LastLayerOutPut.Length; i++)
            {
                // if (LastLayerOutPut[i] > LastLayerOutPut[i + 1])// dizi siniri asilir program  patlar...
                // {
                //     indexHolder = i;
                // }

                if (LastLayerOutPut[i] > LastLayerOutPut[indexHolder])
                {
                    indexHolder = i;
                }
            }
            return indexHolder;


        }
    }
}