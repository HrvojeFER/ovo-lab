using System;
using System.Collections.Specialized;

namespace raytracing
{
    /// <summary>
    /// Klasa predstavlja kuglu u prostoru. Nasljeduje apstraktnu klasu Object. Kugla
    /// je odredena svojim polozajem, radijusom, bojom, parametrima materijala i
    /// udjelima pojedninih zraka (osnovne, odbijene i lomljene).
    /// </summary>
    public class Sphere:Object
    {
        private double radius;
        const double Epsilon = 0.0001;
        private Point IntersectionPoint;

        /// <summary>
        /// Inicijalni konstruktor koji postavlja sve parametre kugle. Za prijenos
        /// parametara koristi se pomocna klasa SphereParameters.
        /// </summary>
        /// <param name="sphereParameters">parametri kugle</param>
        public Sphere ( SphereParameters sphereParameters )
            : base(sphereParameters.getCenterPosition(), sphereParameters.getRaysContributions(),
                sphereParameters.getMaterialParameters(), sphereParameters.getN(),
                sphereParameters.getNi())
        {
            this.radius = sphereParameters.getRadius();
        }

        /// <summary>
        /// Metoda ispituje postojanje presjeka zrake ray s kuglom. Ako postoji presjek
        /// postavlja tocku presjeka IntersectionPoint, te
        /// vraca logicku vrijednost true.
        /// </summary>
        /// <param name="ray">zraka za koju se ispituje postojanje presjeka sa kuglom</param>
        /// <returns>logicku vrijednost postojanja presjeka zrake s kuglom</returns>
        public override bool intersection ( Ray ray )
        {
            // Išao sam metodom da nađem parametarsku jednadžbu linije zrake i
            // jednadžbu ravnine kojoj je normala vektor od centra sfere do ishodišta zrake, te
            // nađem točku njihovog presjeka, pa ako je udaljenost od te točke do radiusa
            // manja od radijusa postoji presjek, a ako ne, presjek ne postoji.

            // Prvo mi trebaju vektor centra sfere i vektor ishodišta zrake.
            var sphereCenterVector = new Vector(
                centerPosition.getX(),
                centerPosition.getY(),
                centerPosition.getZ());
            var rayStartVector = new Vector(
                ray.getStartingPoint().getX(), 
                ray.getStartingPoint().getY(), 
                ray.getStartingPoint().getZ());

            // Spremam normalu u varijablu da ne stvaram više istih vektora.
            var planeNormal = this.getNormal(ray.getStartingPoint());

            // Računam parametar koji ću uvrstiti u parametarsku jednadžbu linije zrake.
            // Slika kako sam došao do ovoga je u intersection.jpg
            var lineParameter = 
                planeNormal.dotProduct(sphereCenterVector.sub(rayStartVector)) /
                planeNormal.dotProduct(ray.getDirection());
            
            // Trebam još provjeriti, zbog ove metode, da li je točka na liniji u smjeru vektora zrake.
            // Ako je u smjeru vektora zrake, onda je parametar linije zrake pozitivan.
            if (lineParameter < 0) return false;

            // Uvrstim parametar u jednadžbu linije i iz nje dobim vektor točke presjeka s ravninom.
            var planePoint = rayStartVector.add(ray.getDirection().multiple(lineParameter));

            // Računam udaljenost od centra sfere do točke presjeka na ravnini.
            var planePointDistance = planePoint.sub(sphereCenterVector).getLength();

            // Ako je udaljenost veća od radijusa, nema presjeka.
            if (planePointDistance > radius) return false;
            
            // Sad kada imam presjek ravnine, samo se trebam pomaknuti u
            // smjeru obrnutom od smjera zrake za sqrt(radius ^ 2 - udaljenost ^ 2)
            // za bliži presjek, a za dalji u smjeru zrake.
            var step = Math.Sqrt(radius * radius - planePointDistance * planePointDistance);

            // Ako je zraka unutar sfere trebamo odabrati dalji presjek, a ako je van sfere onda bliži.
            // Po uputama koristim epsilon jer bi se, na primjer, refraktirana zraka
            // mogla nalaziti točno na granici sfere
            var direction = 
                ray.getStartingPoint().getDistanceFrom(centerPosition) < radius + Epsilon ? 
                ray.getDirection() :
                // Jedinični vektor obrnutog smjera dobimo množenjem jediničnog vektora s -1.
                ray.getDirection().multiple(-1);

            // Pretvaranje u točku.
            IntersectionPoint = new Point(
                planePoint.getX() + direction.getX() * step,
                planePoint.getY() + direction.getY() * step,
                planePoint.getZ() + direction.getZ() * step);

            return true;
        }

        /// <summary>
        /// Vraca tocku presjeka kugle sa zrakom koja je bliza pocetnoj tocki zrake.
        /// </summary>
        /// <returns>tocka presjeka zrake s kuglom koja je bliza izvoru zrake</returns>
        public override Point getIntersectionPoint ()
        {
            return IntersectionPoint;
        }

        /// <summary>
        /// Vraca normalu na kugli u tocki point
        /// </summary>
        /// <param name="point">point na kojoj se racuna normala na kugli</param>
        /// <returns>normal vektor normale</returns>
        public override Vector getNormal ( Point point )
        {
            return new Vector(centerPosition, point);
        }
    }
}