using System;

namespace raytracing
{
    /// <summary>
    /// Klasa predstavlja vektor u trodimenzionalnom prostoru.
    /// </summary>
    public class Vector
    {
        private double x, y, z;

        /// <summary>
        /// Glavni konstruktor koji stvara vektor s komponentama x, y i z.
        /// </summary>
        /// <param name="x">x komponenta vektora</param>
        /// <param name="y">y komponenta vektora</param>
        /// <param name="z">z komponenta vektora</param>
        public Vector ( double x, double y, double z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Konstruktor koji stvara vektor odreden dvijema tockama. Tocka first
        /// predstavlja hvatiste vektora, a tocka second vrh vektora.
        /// </summary>
        /// <param name="first">tocka koja predstavlja pocetak, odnosno hvatiste vektora</param>
        /// <param name="second">tocka koja zadaje vrh vektora</param>
        public Vector ( Point first, Point second )
        {
            x = second.getX() - first.getX();
            y = second.getY() - first.getY();
            z = second.getZ() - first.getZ();
        }

        /// <summary>
        /// Vraca x komponentu vektora.
        /// </summary>
        /// <returns>x komponenta vektora</returns>
        public double getX ()
        {
            return x;
        }

        /// <summary>
        /// Vraca y komponentu vektora.
        /// </summary>
        /// <returns>y komponenta vektora</returns>
        public double getY ()
        {
            return y;
        }

        /// <summary>
        /// Vraca z komponentu vektora.
        /// </summary>
        /// <returns>z komponenta vektora</returns>
        public double getZ ()
        {
            return z;
        }

        /// <summary>
        /// Metoda normalizira vektor, odnosno stvara jedinicni vektor.
        /// </summary>
        public void normalize ()
        {
            double length = this.getLength();
            x /= length;
            y /= length;
            z /= length;
        }

        /// <summary>
        /// Metoda vraca duzinu vektora.
        /// </summary>
        /// <returns>duzina vektora</returns>
        public double getLength ()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Metoda sluzi za oduzimanje dva vektora.
        /// </summary>
        /// <param name="v">vektor za koji se oduzima</param>
        /// <returns>vektor koji je jednak razlici</returns>
        public Vector sub ( Vector v )
        {
            return new Vector(x - v.x, y - v.y, z - v.z);
        }

        /// <summary>
        /// Metoda sluzi za zbrajanje dva vektora.
        /// </summary>
        /// <param name="v">vektor s kojim se zbraja</param>
        /// <returns>vektor koji je jednak zbroju</returns>
        public Vector add ( Vector v )
        {
            return new Vector(x + v.x, y + v.y, z + v.z);
        }

        /// <summary>
        /// Metoda sluzi za mnozenje vektora skalarom.
        /// </summary>
        /// <param name="factor">skalar s kojim se mnozi vektor</param>
        /// <returns>vektor koji je jednak umnosku vektora s skalarom</returns>
        public Vector multiple ( double factor )
        {
            return new Vector(x * factor, y * factor, z * factor);
        }

        /// <summary>
        /// Koristi se za racunanje skalarnog produkta izmedu dva vektora.
        /// </summary>
        /// <param name="v">vektor s kojim se racuna skalarni produkt</param>
        /// <returns>skalarni produkt dva vektora</returns>
        public double dotProduct ( Vector v )
        {
            return x * v.x + y * v.y + z * v.z;
        }

        /// <summary>
        /// Koristi se za racunanje vektorskog produkta.
        /// </summary>
        /// <param name="v">vektor s kojim se racuna produkt</param>
        /// <returns>vektorski produkt dva vektora</returns>
        public Vector crossProduct ( Vector v )
        {
            return new Vector(
                y * v.z - v.y * z, 
                z * v.x - v.z * x, 
                x * v.y - v.x * y);
        }

        /// <summary>
        /// Metoda vraca kut u radijanima (od 0 do PI) izmedu doticnog vektora i
        /// vektora v.
        /// </summary>
        /// <param name="v">vektor na odnosu kojeg se odreduje kut</param>
        /// <returns>kut izmedu dva vektora (u radijanima od 0 do PI)</returns>
        public double getAngle ( Vector v )
        {
            return Math.Acos(this.dotProduct(v) / (this.getLength() * v.getLength()));
        }

        /// <summary>
        /// Vraca reflektirani vektor s obzirom na normalu.
        /// </summary>
        /// <param name="normal">normala jedinične dužine</param>
        /// <returns>reflektirani vektor</returns>
        public Vector getReflectedVector ( Vector normal )
        {
            // Prvo trebam izračunati projekciju na normalu.
            var normalProjection = normal.multiple(-2 * this.dotProduct(normal));

            // Nakon toga, računam reflektirani vektor.
            return this.add(normalProjection);
        }

        /// <summary>
        /// Vraca refraktirani vektor s obzirom na normalu i indekse refrakcije
        /// sredstva upadnog vektora i refraktiranog vektora. Ovaj vektor mora biti jedinične dužine.
        /// </summary>
        /// <param name="normal">normala</param>
        /// <param name="nI">index loma sredstva</param>
        /// <returns>refraktirani vektor</returns>
        public Vector getRefractedVector ( Vector normal, double nI )
        {
            // Formula dobivena postupkom opisanim Bram de Greve sa Stanford-a

            // Prvo računam produkt s normalom
            var normalDotProduct = this.dotProduct(normal);
            
            // Znači da idemo iz materijala u vanjski svijet.
            if (normalDotProduct > 0) nI = 1 / nI;
            else normalDotProduct = -normalDotProduct;

            // Trebaju mi odvojene komponente ovog vektora.
            var normalProjection = normal.multiple(normalDotProduct);
            var surfaceProjection = this.sub(normalProjection);

            // Kosinus upadnog kuta je jednak duljini projekcije na površinu.
            var angleOfIncidenceCosine = surfaceProjection.getLength();

            // Može se nekada dogoditi radi operacija s float-ovima
            if (angleOfIncidenceCosine > 1.0) angleOfIncidenceCosine = 1.0;

            // Kvadrat sinusa izlaznog kuta je, po Snellovoj formuli, jednak kvadratu umnoška
            // indeksa loma i sinusa upadnog kuta, pa možemo još samo napraviti supstituciju
            // za kvadrat upadnog kuta.
            var outputAngleSineSquared = nI * nI * 
                                         (1.0 - angleOfIncidenceCosine * angleOfIncidenceCosine);

            // Može se nekada dogoditi kada je nI prevelik.
            if (outputAngleSineSquared > 1.0) outputAngleSineSquared = 1.0;

            // Koeficijent kojim ćemo pomnožiti normalu.
            var normalCoefficient = nI * angleOfIncidenceCosine -
                                    Math.Sqrt(1 - outputAngleSineSquared);

            // Konačan rezultat.
            return this.multiple(nI).add(normal.multiple(normalCoefficient));
        }
    }
}