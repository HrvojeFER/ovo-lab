using System;
using System.Drawing;

namespace raytracing
{
    /// <summary>
    /// Klasa predstvlja scenu kod modela crtanja slike pomocu ray tracinga. Sastoji
    /// se od izvora svjetlosti i konacnog broja objekata.
    /// </summary>
    public class Scene
    {
        const int MAXDEPTH = 5; //maksimalna dubina rekurzije
        private int numberOfObjects;
        private Sphere[] sphere;
        private Point lightPosition;
        private ColorVector backgroundColors = new ColorVector(0, 0, 0);
        private ColorVector light = new ColorVector((float)1, (float)1, (float)1);
        private ColorVector ambientLight = new ColorVector((float)0.5, (float)0.5, (float)0.5);

        /// <summary>
        /// Inicijalni konstruktor koji postavlja poziciju svijetla i parametre svih
        /// objekata u sceni.
        /// </summary>
        /// <param name="lightPosition">pozicija svijetla</param>
        /// <param name="numberOfObjects">broj objekata u sceni</param>
        /// <param name="sphereParameters">parametri svih kugli</param>
        public Scene ( Point lightPosition, int numberOfObjects, SphereParameters[] sphereParameters )
        {
            this.lightPosition = lightPosition;
            this.numberOfObjects = numberOfObjects;
            sphere = new Sphere[numberOfObjects];
            for(int i = 0; i < numberOfObjects; i++)
            {
                sphere[i] = new Sphere(sphereParameters[i]);
            }
        }

        /// <summary>
        /// Metoda provjerava da li postoji sjena na tocki presjeka. Vraca true ako
        /// se zraka od mjesta presjeka prema izvoru svjetlosti sjece s nekim objektom.
        /// </summary>
        /// <param name="intersection">tocka presjeka</param>
        /// <returns>true ako postoji sjena u tocki presjeka, false ako ne postoji</returns>
        private bool shadow ( Point intersection )
        {
            var shadowRay = new Ray(intersection, lightPosition);

            for (var i = 0; i < numberOfObjects; ++i) 
                if (sphere[i].intersection(shadowRay)) 
                    return true;  

            return false;
        }

        /// <summary>
        /// Metoda koja pomocu pracenja zrake racuna boju u tocki presjeka. Racuna se
        /// osvjetljenje u tocki presjeka te se zbraja s doprinosima osvjetljenja koje
        /// donosi reflektirana i refraktirana zraka.
        /// </summary>
        /// <param name="ray">pracena zraka</param>
        /// <param name="depth">dubina rekurzije</param>
        /// <returns>vektor boje u tocki presjeka</returns>
        public ColorVector traceRay ( Ray ray, int depth )
        {
            // Da bi sprječio beskonačnu rekurziju,
            // upute govore da koristim provjeru maksimalne dubine, pa 
            // ako sam prešao maksimalnu dubinu, vraćam samo pozadinsku boju.
            if (depth > MAXDEPTH) return backgroundColors;

            // Posavljam presjek na null prvo tako da kasnije mogu provjeriti
            // da li presjek postoji.
            Point intersection = null;
            
            // Spremam i sferu s kojom sam našao presjek za računanje doprinosa osvjetljenja.
            Sphere intersectionSphere = null;
            
            // Čuvam udaljenost presjeka od ishodišta zrake
            // jer zraka najvjerojatnije presjeca mali broj objekata, pa
            // bi bilo bezveze svaki je put izračunati.
            var intersectionDistance = double.MaxValue;

            // Da nađem presjek prelazim sve objekte.
            for(var i = 0 ; i < numberOfObjects ; ++i)
            {
                // Prvo provjeravam da li postoji presjek.
                if (!sphere[i].intersection(ray)) continue;
                
                // Ako postoji presjek treba mi udaljenost presjeka od ishodišta zrake.
                var currentIntersection = sphere[i].getIntersectionPoint();
                var currentDistance = 
                    currentIntersection.getDistanceFrom(ray.getStartingPoint());

                // Treba mi presjek s najmanjom udaljenosti, pa
                // ako je veća trenutačna udaljenost od udaljenosti prije izračunatog presjeka,
                // ne uzimam trenutačan presjek. 
                if (currentDistance > intersectionDistance) continue;
                    
                intersection = currentIntersection;
                intersectionDistance = currentDistance;
                intersectionSphere = sphere[i];
            }

            // Ako nisam našao presjek vraćam samo pozadinsku boju.
            if (intersection == null) return backgroundColors;

            // Sad kada sam našao najbliži presjek trebam izračunati doprinose
            // ove zrake (Phong-ov model - ambijentna, difuzna i spekularna komponenta), te
            // doprinose reflektiranih i refraktiranih zraka.
            var colorContributions = new ColorVector(0.0f, 0.0f, 0.0f);

            // Ambijentna komponenta.
            var ambientComponent = ambientLight.multiple(intersectionSphere.getKa());
            // Dodavanje ambijentne komponente.
            colorContributions = colorContributions.add(ambientComponent);

            // Za izračun ostalih komponenti mi treba normalizirana normala presjeka.
            var intersectionNormal = intersectionSphere.getNormal(intersection);
            intersectionNormal.normalize();

            // Ako nismo naišli na sjenu dodajemo difuznu i spekularnu komponentu, te
            // doprinose od reflektirane i refraktirane zrake.
            if (!shadow(intersection))
            {
                // Za izračun difuzne i spekularne komponente treba mi
                // vektor od presjeka do izvora svjetla i normala presjeka.
                var intersectionToLightSourceVector = new Vector(
                    lightPosition.getX() - intersection.getX(),
                    lightPosition.getY() - intersection.getY(),
                    lightPosition.getZ() - intersection.getZ());
                // Za prirodnije svjetlo normaliziram.
                intersectionToLightSourceVector.normalize();

                // Difuzna komponenta.
                // Izračun difuzne komponente.
                var diffuseComponent = light
                    .multiple(intersectionSphere.getKd())
                    .multiple(intersectionToLightSourceVector.dotProduct(intersectionNormal));
                // Treba ispraviti difuznu komponentu da ne poništava ambijentnu kad bude pretamna.
                diffuseComponent.correct();

                // Spekularna komponenta.
                // Za spekularnu komponentu mi treba zraka od izvora svjetla odbijena od presjeka.
                var reflectedLightRay = intersectionToLightSourceVector
                    .multiple(-1).getReflectedVector(intersectionNormal);
                // Za prirodnije osvjetljenje normaliziram.
                reflectedLightRay.normalize();
                // Trebam izračunati projekciju odbijene zrake na smjer praćene zrake.
                var reflectionProjection = reflectedLightRay.dotProduct(
                    ray.getDirection());

                // Izračun spekularne komponente.
                var specularComponent = light
                    .multiple(intersectionSphere.getKs())
                    .multiple(Math.Pow(reflectionProjection, intersectionSphere.getN()));
                // Ovaj put ispravljam spekularnu komponentu da ne nadjača doprinose
                // difuzne i ambijentne.
                specularComponent.correct();

                // Računam zbroj svih doprinosa od svjetla.
                var lightContributions = diffuseComponent
                    .add(specularComponent);
                // Ispravljam doprinose od svjetla jer mogu poništiti ambijentnu komponentu.
                lightContributions.correct();

                // Dodajem doprinose od svjetla doprinosima boje.
                colorContributions = colorContributions
                    .add(lightContributions);
            }

            // Računanje doprinosa reflektiranih i refraktiranih zraka.
            // Da bih dobio doprinose reflektirane i refraktirane zrake trebam ih pratiti i
            // povećati dubinu za jedan.
            // Smjer reflektirane zrake.
            var reflectedRayDirection = ray.getDirection()
                .getReflectedVector(intersectionNormal);
            // Doprinos reflektirane zrake.
            var reflectionContribution = traceRay(
                    new Ray(intersection, reflectedRayDirection),
                    depth + 1)
                .multiple(intersectionSphere.getReflectionFactor());

            // Smjer refraktirane zrake.
            var refractedRayDirection = ray.getDirection().getRefractedVector(
                intersectionNormal,
                intersectionSphere.getNi());
            // Doprinos refraktirane zrake.
            var refractionContribution = traceRay(
                    new Ray(intersection, refractedRayDirection),
                    depth + 1)
                .multiple(intersectionSphere.getRefractionFactor());

            // Zbrajam doprinose rekurzivnih poziva i
            // ispravljam ih da ne ponište ostale doprinose.
            var recursionContribution = reflectionContribution;
            recursionContribution.correct();
            // Na kraju ih dodajem ostalim komponentama.
            colorContributions = colorContributions.add(recursionContribution);

            // Zbroj doprinosa treba još samo ispraviti da je u granicama [0, 1] ako
            // smo na početnoj dubini.
            colorContributions.correct();

            return colorContributions;
        }
    }
}