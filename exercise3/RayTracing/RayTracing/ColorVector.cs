﻿using System;

namespace raytracing
{
    /// <summary>
    /// Pomocna klasa koja predstavlja vektor boje. Boja je odredena s tri komponente:
    /// crvenom, zelenom i plavom. Svaka komponenta odredena je decimalnim brojem
    /// od 0 do 1. Koristi se kod odredivanja boje pomocu intenziteta.
    /// </summary>
    public class ColorVector
    {
        private float red, green, blue;

        /// <summary>
        /// Inicijalni kontruktor koji postavlja vrijednost crvene, zelene i plave
        /// komponente boje.
        /// </summary>
        /// <param name="red">crvena komponenta boje</param>
        /// <param name="green">zelena komponenta boje</param>
        /// <param name="blue">plava komponenta boje</param>
        public ColorVector (float red, float green, float blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        /// <summary>
        /// vraca crvenu komponentu boje
        /// </summary>
        /// <returns>crvena komponenta boje</returns>
        public float getRed ()
        {
            return this.red;
        }

        /// <summary>
        /// vraca zelenu komponentu boje
        /// </summary>
        /// <returns>zelena komponenta boje</returns>
        public float getGreen ()
        {
            return this.green;
        }

        /// <summary>
        /// vraca plavu komponentu boje
        /// </summary>
        /// <returns>plava komponenta boje</returns>
        public float getBlue ()
        {
            return this.blue;
        }

        /// <summary>
        /// Sluzi za zbrajanje dviju boja.
        /// </summary>
        /// <param name="c">boja s kojom se zbraja</param>
        /// <returns>zbroj boja</returns>
        public ColorVector add ( ColorVector c )
        {
            return new ColorVector(red + c.getRed(), green + c.getGreen(), blue + c.getBlue());
        }

        /// <summary>
        /// Sluzi za mnozenje komponenata boje skalarom.
        /// </summary>
        /// <param name="factor">skalar s kojim se mnoze parametri boja</param>
        /// <returns>umnozak boje i skalara</returns>
        public ColorVector multiple ( double factor )
        {
            return new ColorVector((float)factor * red, (float)factor * green, (float)factor * blue);
        }

        /// <summary>
        /// Sluzi za mnozenje komponenata dviju boja.
        /// </summary>
        /// <param name="c">vektor boje s kojim se mnozi</param>
        /// <returns>umnozak dviju boja</returns>
        public ColorVector multiple ( ColorVector c )
        {
            return new ColorVector(c.getRed() * red, c.getGreen() * green, c.getBlue() * blue);
        }

        /// <summary>
        /// Sluzi za mnozenje vektora boje s vektorom koeficijenata. Koristi se kod
        /// odredivanja boje lokalnog osvjetljenja.
        /// </summary>
        /// <param name="c">vektor koeficijenata s kojim se mnozi</param>
        /// <returns>vektor boje koji je rezultat mnozenja vektora boje s vektorom koeficijenata</returns>
        public ColorVector multiple ( PropertyVector c )
        {
            return new ColorVector(c.getRedParameter() * red, c.getGreenParameter() * green,
                    c.getBlueParameter() * blue);
        }

        /// <summary>
        /// Sluzi za provjeravanje i ispravljanje vektora boje s obzirom na dozvoljene
        /// granice boja od 0 do 1. U slucaju da su granice premasene vrijednosti se
        /// zaokruzuju na najvisu, odnosno najmanju.
        /// </summary>
        public void correct ()
        {
            CorrectColor(ref red);
            CorrectColor(ref green);
            CorrectColor(ref blue);
        }

        private static void CorrectColor(ref float color)
        {
            // Ako je boja manja od 0, postavi je na 0, a ako je veća od 1, postavi je na 1.
            color = color < 0 ? 0 : color > 1 ? 1 : color;
        }
    }
}